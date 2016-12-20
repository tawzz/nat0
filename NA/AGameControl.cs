using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml.Linq;

namespace ations
{
  public partial class AGame : DependencyObject, INotifyPropertyChanged
  {
    #region settings for control_flow: change to determine game ablauf (eg how many rounds...)
    int rounds = 1;
    string[] phases = { "growth", "actions", "production" };
    int[] turnrounds = { 1, 1, 0 }; // how many times players take turns, set to 100 when indeterminate (finish by passing)
    int[] actPerMoveDefault = { 1, 1, 0 }; // how many actions does this player have (eg sun tzu: 2 actions in first turn)
    int longDelay = 500, shortDelay = 100, minAnimationDuration = 1000; // default wait times (wenn nichts zu tun)
    #endregion

    #region control_flow properties, members
    //public bool ShowButton { get { return showButton; } set { if (showButton != value) { showButton = value; if (value) IsOkStartEnabled = true; NotifyPropertyChanged(); } } }
    //bool showButton;
    public string Message { get { return message; } set { message = value; NotifyPropertyChanged(); } }
    string message;
    public string LongMessage { get { return longMessage; } set { longMessage = value; NotifyPropertyChanged(); } }
    string longMessage;
    public string Caption { get { return caption; } set { caption = value; NotifyPropertyChanged(); } }
    string caption = "Start";
    public string Title { get { return title; } set { title = value; NotifyPropertyChanged(); } }
    string title;
    public bool Interrupt { get; set; }
    public bool IsRunning { get; private set; }
    public Action Kickoff { get; private set; } //weg
    //public Action<object> ResourceUpdated { get; private set; }
    public List<Storyboard> ResourceUpdateAnimationQueue { get; set; }
    public FrameworkElement UIRoundmarker { get; set; }

    int ipl, iph, ird, iturnround, iact, moveCounter; bool gov; string phase = ""; // gameloop counters

    #endregion

    #region control_flow tasks
    async Task WaitSeconds(double secs) { int delay = (int)(secs * 1000); await Task.Delay(delay); }
    async Task WaitForButtonClick()
    {
      IsOkStartEnabled = true; OkStartClicked = false;

      while (!OkStartClicked)
      {
        if (Interrupt) throw (new Exception("STOPPED BY PLAYER!"));
        await Task.Delay(100);
      }
      OkStartClicked = false; IsOkStartEnabled = false;
    }
    async Task WaitFor3ButtonClick()
    {
      IsOkStartEnabled = IsCancelEnabled = IsPassEnabled = true;
      OkStartClicked = CancelClicked = PassClicked = false;
      while (!OkStartClicked && !CancelClicked && !PassClicked)
      {
        if (Interrupt) throw (new Exception("STOPPED BY PLAYER!"));
        await Task.Delay(100);
      }
      IsOkStartEnabled = IsCancelEnabled = IsPassEnabled = false;
    }
    async Task WaitForAnimationQueueCompleted(int minAnimations = 0)
    {
      while (ResourceUpdateAnimationQueue.Count < minAnimations) await Task.Delay(200);//give ui time to trigger resourceUpdated event
      Console.WriteLine(LongMessage = "Animation Queue ready -  starting animations...");
      while (ResourceUpdateAnimationQueue.Count > 0)
      {
        var sb = ResourceUpdateAnimationQueue[0];
        ResourceUpdateAnimationQueue.RemoveAt(0);
        sb.Begin();
        await Task.Delay(minAnimationDuration);
        while (sb.GetCurrentState() == ClockState.Active && sb.GetCurrentTime() < sb.Duration)
        { await Task.Delay(100); }
      }
      Console.WriteLine(LongMessage = "Animation Queue abgearbeitet!");
    }
    async Task WaitForRoundMarkerAnimationCompleted()
    {
      var sb = Storyboards.MoveTo(UIRoundmarker, Stats.RoundMarkerPosition, TimeSpan.FromSeconds(1), null);
      //var sb = Storyboards.Scale(testui, TimeSpan.FromSeconds(.3), new Point(1, 1), new Point(5, 2), null, true);
      //sb.Completed += (s, _) => testcompleted(sb, testui); // brauche garkein completed in wirklichkeit! nur testing!
      sb.Begin();
      while (sb.GetCurrentState() == ClockState.Active && sb.GetCurrentTime() < sb.Duration) { await Task.Delay(200); }
    }
    async Task<ARes> MakeSureUserPicksAResource()
    {
      while (SelectedResource == null)
      {
        await Task.Delay(shortDelay);
        await WaitForButtonClick();

        var res = SelectedResource;
        if (res == null) { Message = "YOU DID NOT PICK ANYTHING!"; }
      }
      return SelectedResource;
    }
    async Task WaitForPickResourceCompleted(IEnumerable<string> resnames, int num, string forWhat)
    {
      Debug.Assert(ResChoices != null && ResChoices.Count == 0, "ResChoices not cleared for growth resource pick");
      Debug.Assert(SelectedResource == null, "start resource pick with resource already selected!");

      foreach (var rname in resnames) ResChoices.Add(new ARes(rname));
      Number = num;
      ShowResChoices = true; Caption = "Pick"; Message = MainPlayer.Name + ", pick " + forWhat + " resource";

      var res = await MakeSureUserPicksAResource();

      ShowResChoices = false; ResChoices.Clear(); SelectedResource = null;
      Message = MainPlayer.Name + " picked " + res.Name.ToCapital();

      if (res.Name == "worker")
      {
        var workers = MainPlayer.ExtraWorkers;
        var wfree1 = workers.FirstOrDefault(x => !x.IsCheckedOut);
        var wfree2 = workers.LastOrDefault(x => !x.IsCheckedOut);
        if (wfree1.CostRes != wfree2.CostRes)
        {
          Debug.Assert(ResChoices != null && ResChoices.Count == 0, "ResChoices not cleared for worker pick");
          Debug.Assert(SelectedResource == null, "start worker pick with resource already selected!");

          var resarr = MainPlayer.ExtraWorkers.Where(x => !x.IsCheckedOut).Select(x => x.CostRes).Distinct().ToArray();
          foreach (var rname in resarr) ResChoices.Add(new ARes(rname));
          ShowResChoices = true; Message = "pick worker type";

          res = await MakeSureUserPicksAResource();
          ShowResChoices = false; ResChoices.Clear(); SelectedResource = null;

          if (res.Name == wfree2.CostRes) wfree1 = wfree2;
        }
        Message = MainPlayer.Name + " picked " + wfree1.CostRes.ToCapital() + " worker";
        MainPlayer.CheckOutWorker(wfree1);
      }
      else { MainPlayer.Res.inc(res.Name, Number); }

    }
    async Task GrowthPhaseComplete()
    {
      //growth phase --------------------------
      Debug.Assert(IsOkStartEnabled == false, "Start growth phase with button enabled!!!");
      Title = "Round " + (ird + 1); LongMessage = "growth phase is starting...";
      Title += ": Growth";

      Debug.Assert(ipl == 0, "growth phase starting with ipl != 0!");
      while (ipl < NumPlayers)
      {
        MainPlayer = Players[ipl];

        // just 1 growth pick action for now

        var reslist = new List<string> { "wheat", "coal", "gold" };
        if (MainPlayer.ExtraWorkers.Count(x => !x.IsCheckedOut) > 0) reslist.Add("worker");
        var num = LevelGrowth[MainPlayer.Level];

        if (Interrupt) throw (new Exception("STOPPED BY PLAYER!"));

        await WaitForPickResourceCompleted(reslist, num, "growth");

        await WaitForAnimationQueueCompleted(1);

        ipl++;
      }
    }
    #endregion

    #region 1. determine what user can do & enable
    public bool CanTakeArchitect { get { return canTakeArchitect; } set { canTakeArchitect = value; NotifyPropertyChanged(); } }
    bool canTakeArchitect;
    public bool CanTakeTurmoil { get { return canTakeTurmoil; } set { canTakeTurmoil = value; NotifyPropertyChanged(); } }
    bool canTakeTurmoil;
    public bool CanDeploy { get { return MainPlayer.Res.n("coal") > Stats.Age; } } //simplified
    public bool CanSelectWorker { get { return MainPlayer.Res.get("worker").IsSelectable; } set { MainPlayer.Res.get("worker").IsSelectable = value; NotifyPropertyChanged(); } }
    public bool CanBuyProgressCard { get { return canBuyProgressCard; } set { if (canBuyProgressCard != value) { canBuyProgressCard = value; NotifyPropertyChanged(); } } }
    bool canBuyProgressCard;
    public bool CanPlaceCard { get { return canPlaceCard; } set { if (canPlaceCard != value) { canPlaceCard = value; NotifyPropertyChanged(); } } }
    bool canPlaceCard;

    public bool IsOkStartEnabled { get { return isOkStartEnabled; } set { if (isOkStartEnabled != value) { isOkStartEnabled = value; NotifyPropertyChanged(); } } }
    bool isOkStartEnabled;
    public bool IsPassEnabled { get { return isPassEnabled; } set { if (isPassEnabled != value) { isPassEnabled = value; NotifyPropertyChanged(); } } }
    bool isPassEnabled;
    public bool IsCancelEnabled { get { return isCancelEnabled; } set { if (isCancelEnabled != value) { isCancelEnabled = value; NotifyPropertyChanged(); } } }
    bool isCancelEnabled;

    public void MarkAllPlayerChoices()
    {
      MarkPossibleProgressCards();
      MarkCiv(); //TODO: check which cards can be activated
      MarkArchitects();
      MarkTurmoils();
      MarkWorkers();

    }
    public void MarkPossibleProgressCards()
    {
      UnmarkProgresscards();
      foreach (var f in Progress.Fields.Where(x => x.Card != null)) f.Card.CanBuy = CalculateCanBuy0(f);
    }
    public void MarkCiv()
    {
      foreach (var f in MainPlayer.Civ.Fields.Where(x => !x.IsEmpty)) f.Card.CanActivate = true; // refine!
    }
    public void MarkArchitects() { CanTakeArchitect = ArchitectAvailable && CalcArchitectCost() >= 0; }
    public void MarkTurmoils() { CanTakeTurmoil = true; }
    public void MarkWorkers() { CanSelectWorker = MainPlayer.Res.n("worker") > 0 && CanDeploy; }
    #endregion

    #region 2. get user click & select objects:
    public bool OkStartClicked { get; set; }
    public bool CancelClicked { get; set; }
    public bool PassClicked { get; set; }

    public void OnClickArchitects()
    {
      UnselectProgress(); UnselectCiv(); UnselectPreviousCiv(); UnselectTurmoils(); UnselectWorker();
      Message = "Take Architect?";
      ArchitectSelected = true;
      SelectedAction = TakeArchitect;
    }
    public void OnClickTurmoils()
    {
      UnselectProgress(); UnselectCiv(); UnselectArchitects(); UnselectWorker();
      Message = "Take Turmoil?";
      TurmoilSelected = true;
      SelectedAction = TakeTurmoil;
    }
    public void OnClickWorker(ARes res)
    {
      if (CanSelectWorker)// eliminate check
      {
        WorkerSelected = true;
        UnselectTurmoils(); UnselectArchitects(); UnselectProgress(); UnselectPreviousCiv();
        if (SelectedCivField != null) { Message = "Deploy?"; SelectedAction = DeployAvailableWorker; }
        else { Message = "Select a field to which to deploy"; SelectedAction = PartialDeploy; }
      }
    }
    public void OnClickProgressCard(AField field)
    {
      UnselectCiv(); UnselectPreviousCiv(); UnselectArchitects(); UnselectWorker(); UnselectTurmoils();
      var isNewField = field != SelectedProgressField;
      UnselectProgress();
      if (isNewField)
      {
        SelectedProgressField = field;
        SelectedProgressField.Card.IsSelected = true;
        if (IsOneStepBuy(field)) { Message = "Buy " + field.Card.Name + "?"; SelectedAction = BuyProgressCard; }
        else { Message = "Select Place on Civ Board"; MarkPossiblePlaces(field); SelectedAction = PartialBuy; }
      }
    }
    public void OnClickCivCard(AField field)
    {
      UnselectTurmoils();
      PreviousSelectedCivField = SelectedCivField;
      SelectedCivField = field;

      if (PreviousSelectedCivField == SelectedCivField)
      {
        UnselectCiv(); UnselectPreviousCiv();
      }
      else if (SelectedProgressField != null && (field.Card.buildmil() || field.Card.colony()))
      { //Buy
        UnselectWorker(); UnselectArchitects(); UnselectPreviousCiv();
        field.Card.IsSelected = true;
        Message = "Buy " + field.Card.Name + "?";
        SelectedAction = BuyProgressCard;
      }
      else if (field.Index == CT.WIC && CanTakeArchitect)
      { //Architect
        UnselectProgress(); UnselectWorker(); UnselectPreviousCiv();
        field.Card.IsSelected = true;
        Message = "Take Architect?";
        ArchitectSelected = true;
        SelectedAction = TakeArchitect;
      }
      else if (CanDeploy && field.Card.buildmil())
      { //Deploy
        UnselectProgress(); UnselectArchitects();
        field.Card.IsSelected = true;
        if (WorkerSelected) { Message = "Deploy?"; SelectedAction = DeployAvailableWorker; }
        else if (PreviousSelectedCivField != null)
        {
          if (field.Card.NumDeployed > 0)
          {
            Message = "Deploy?";
            SelectedAction = DeployFromField;
          }
          else UnselectPreviousCiv();
        }
        else { Message = "Select deploy source"; SelectedAction = PartialDeploy; }
      }
    }

    AField SelectedProgressField { get; set; }
    AField SelectedCivField { get; set; }
    AField PreviousSelectedCivField { get; set; }
    public bool ArchitectSelected { get { return architectSelected; } set { if (architectSelected != value) { architectSelected = value; NotifyPropertyChanged(); } } }
    bool architectSelected;
    public bool TurmoilSelected { get { return turmoilSelected; } set { if (turmoilSelected != value) { turmoilSelected = value; NotifyPropertyChanged(); } } }
    bool turmoilSelected;
    public bool WorkerSelected { get { return MainPlayer.Res.get("worker").IsSelected; } set { MainPlayer.Res.get("worker").IsSelected = value; NotifyPropertyChanged(); } }
    #endregion

    #region 3. perform resulting actions: SelectedAction
    Action SelectedAction { get; set; }

    public void PartialBuy()
    {
      if (SelectedCivField != null && SelectedProgressField != null)
      {
        BuyProgressCard();
      }
      else Message = "You need to select a place on civ board!";
    }
    public void PartialDeploy()
    {
      if (SelectedCivField != null && PreviousSelectedCivField != null)
      {
        DeployFromField();
      }
      else if (SelectedCivField != null && WorkerSelected)
      {
        DeployAvailableWorker();
      }
      else Message = "You need to select a place on civ board!";
    }
    //1 step actions:
    public void TakeArchitect() { }
    public void TakeTurmoil() { }
    //1 or 2 step actions:
    public void BuyProgressCard()
    {
      var card = SelectedProgressField.Card;
      var fieldBuy = SelectedProgressField;
      var fieldPlace = SelectedCivField;
      card.CanBuy = false;

      if (card.civ())
      {
        if (fieldPlace == null) fieldPlace = GetPossiblePlacesForCard(fieldBuy).First();
        MainPlayer.Civ.Add(card, fieldPlace);
        Progress.Remove(fieldBuy);
        MainPlayer.Pay(card.Cost);
      }
      else
      {
        Progress.Remove(fieldBuy);
        MainPlayer.Pay(card.Cost);
        if (card.war()) { Stats.UpdateWarPosition(MainPlayer, card); }
        else if (card.golden()) BuyGoldenAge(card);
        else if (card.battle())
        {
          LongMessage = "TODO!!!! resource picker invokation";
          //AAnimations.AfterAnimation = null; // (x) => NextPlayer(); // ani in ui ausgeloest onResourceUpdate
          //PrepareResourcePicker(new string[] { "wheat", "coal", "book" }, MainPlayer.RaidValue, "battle");
        }
      }
      Message = MainPlayer.Name + " bought " + card.Name;

    }
    // 2 step actions:
    public void DeployFromField()
    {
      //need to check if that field even has workers on it! for now, no effect but action wasted if try to deploy from empty card
      // geht leicht: wenn 2te civCard selected, unmark erste ausser wenn moegliches deployment
      var sourceCard = SelectedCivField.Card;
      var targetCard = PreviousSelectedCivField.Card;
      if (sourceCard.NumDeployed > 0) //do this check earlier!
      {
        targetCard.NumDeployed++;
        sourceCard.NumDeployed--;
        var deploymentCost = Stats.Age; //simplified
        MainPlayer.Pay("coal", deploymentCost);
        //MainPlayer.Res.dec("worker", 1);//TODO: ersetze durch Deploy in APlayer
        Message = MainPlayer.Name + " deployed from " + sourceCard.Name + " to " + targetCard.Name;
      }
    }
    public void DeployAvailableWorker()
    {
      var card = SelectedCivField.Card;
      card.NumDeployed++;
      var deploymentCost = Stats.Age; //simplified
      MainPlayer.Pay("coal", deploymentCost);
      MainPlayer.Res.dec("worker", 1);//TODO: ersetze durch Deploy in APlayer
      Message = MainPlayer.Name + " deployed to " + card.Name;
    }

    #endregion

    #region 4. cleanup selections & enablings
    public void UnselectAll() { UnselectProgress(); UnselectCiv(); UnselectPreviousCiv(); UnselectArchitects(); UnselectTurmoils(); UnselectWorker(); }
    void UnselectProgress()
    {
      if (SelectedProgressField != null)
      {
        if (SelectedProgressField.Card != null) SelectedProgressField.Card.IsSelected = false;
        SelectedProgressField = null;
      }
    }
    void UnselectCiv()
    {
      if (SelectedCivField != null)
      {
        if (SelectedCivField.Card != null) SelectedCivField.Card.IsSelected = false;
        SelectedCivField = null;
      }
    }
    void UnselectPreviousCiv()
    {
      if (PreviousSelectedCivField != null)
      {
        if (PreviousSelectedCivField.Card != null) PreviousSelectedCivField.Card.IsSelected = false;
        PreviousSelectedCivField = null;
      }
    }
    void UnselectTurmoils() { TurmoilSelected = false; }
    void UnselectArchitects() { ArchitectSelected = false; }
    void UnselectWorker() { WorkerSelected = false; }

    void UnmarkAllPlayerChoices()
    {
      UnmarkPlaces();
      UnmarkProgresscards();
      UnmarkArchitects();
      UnmarkTurmoils();
      UnmarkWorkers();
    }
    public void UnmarkPlaces()
    {
      foreach (var f in MainPlayer.Civ.Fields) f.Card.CanActivate = false;
    }
    public void UnmarkWorkers() { CanSelectWorker = false; }
    public void UnmarkArchitects() { CanTakeArchitect = false; }
    public void UnmarkTurmoils() { CanTakeTurmoil = false; }
    public void UnmarkProgresscards()
    {
      foreach (var f in Progress.Fields) if (f.Card != null) f.Card.CanBuy = false;
    }

    #endregion

    public async void GameLoop()
    {
      try
      {
        IsOkStartEnabled = false;
        IsRunning = true;
        gov = false;
        while (ird < rounds)
        {
          Debug.Assert(iph == 0, "iph != 0 at beginning of round!!!");

          Title = "Round " + (ird + 1); LongMessage = "round " + ird + " is starting...";

          Stats.UpdateRound();
          Stats.UpdateAge();
          await WaitForRoundMarkerAnimationCompleted();

          Progress.Deal();

          //growth phase --------------------------
          await GrowthPhaseComplete(); //comment to go directly to action phase

          LongMessage = "growth phase ending..."; await Task.Delay(longDelay); LongMessage = "growth phase over!"; Console.WriteLine("\t" + LongMessage);
          ipl = 0;

          Stats.PickEventCard();

          //player action phase --------------------------
          Title = "Round " + (ird + 1) + ": Player Actions"; Caption = "Ok"; LongMessage = "player action phase is starting..."; Message = "";
          foreach (var pl in Players) pl.HasPassed = false;
          iturnround = 0;
          MainPlayer = Players[ipl];
          while (MainPlayer != null)
          {
            // begin player action
            MainPlayer.NumActions = 1; //ignore sun tzu
            Debug.Assert(iact == 0, "iact no 0 at begin of player turn");
            while (iact < MainPlayer.NumActions && !MainPlayer.HasPassed)
            {
              //begin action
              Message = MainPlayer.Name + ", choose action";
              ResourceUpdateAnimationQueue.Clear();
              SelectedAction = null;
              MarkAllPlayerChoices();

              await WaitFor3ButtonClick();

              if (PassClicked) { MainPlayer.HasPassed = true; UnselectAll(); } // next player turn
              else if (CancelClicked) { UnselectAll(); await Task.Delay(longDelay); }
              else if (SelectedAction != null && SelectedAction != PartialBuy)
              {
                SelectedAction();
                await WaitForAnimationQueueCompleted();
                iact++;
                UnselectAll(); // action completed
              }
              else if (SelectedAction == PartialBuy) // partial information, need more selections
              {
                while (!CancelClicked && !PassClicked && SelectedCivField == null)
                {
                  await Task.Delay(200);
                  await WaitFor3ButtonClick();
                }
                if (CancelClicked) UnselectAll();
                else if (PassClicked) { MainPlayer.HasPassed = true; UnselectAll(); }
                else if (SelectedAction != null)
                {
                  SelectedAction();
                  await WaitForAnimationQueueCompleted();
                  iact++;
                  UnselectAll(); // action completed
                }
              }
              else if (SelectedAction == PartialDeploy) // partial information, need more selections
              {
                while (!CancelClicked && !PassClicked && !WorkerSelected && PreviousSelectedCivField == null)
                {
                  await Task.Delay(200);
                  await WaitFor3ButtonClick();
                }
                if (CancelClicked) UnselectAll();
                else if (PassClicked) { MainPlayer.HasPassed = true; UnselectAll(); }
                else if (SelectedAction != null)
                {
                  SelectedAction();
                  await WaitForAnimationQueueCompleted();
                  iact++;
                  UnselectAll(); // action completed
                }
              }

              //clear buttons
              PassClicked = OkStartClicked = CancelClicked = false;


              //end action
              UnselectAll();
            }
            //end player action
            iact = 0;
            ipl = (ipl + 1) % NumPlayers;
            MainPlayer = Players.First(x => !x.HasPassed && x.Index >= ipl);
          }

          await WaitForButtonClick();
          //Debug.Assert(ipl == 0, "growth phase starting with ipl != 0!");
          //while (ipl < NumPlayers)
          //{
          //  MainPlayer = Players[ipl];

          //  // just 1 growth pick action for now

          //  var reslist = new List<string> { "wheat", "coal", "gold" };
          //  if (MainPlayer.ExtraWorkers.Count(x => !x.IsCheckedOut) > 0) reslist.Add("worker");
          //  var num = LevelGrowth[MainPlayer.Level];

          //  if (Interrupt) throw (new Exception("STOPPED BY PLAYER!"));

          //  await WaitForPickResourceTask(reslist, num, "growth");

          //  ipl++;
          //}
          //ipl = 0;

          //LongMessage = "growth phase ending..."; await Task.Delay(longDelay); LongMessage = "growth phase over!"; Console.WriteLine("\t" + LongMessage);






          await DoStuffAtEndOfRound();
          ird++;
        }
        LongMessage = "game is over";
      }
      catch (Exception e)
      {
        LongMessage = "interrupt!!!: " + e.Message;
      }
    }

    #region helpers
    bool IsOneStepBuy(AField field)
    {
      var card = field.Card;
      var possible = GetPossiblePlacesForCard(field).ToArray();
      var isciv = card.civ();
      return !isciv || possible.Length == 1;
    }
    public void BuyGoldenAge(ACard card)
    {
      //// check if even can afford vp option
      //var canaffordvp = MainPlayer.Res.n("gold") >= Stats.Age;
      //// if yes, 
      //if (canaffordvp)
      //{
      //  AAnimations.AfterAnimation = (picked) =>
      //  {
      //    var aa = AAnimations.AfterAnimation;
      //    var res = picked as ARes;
      //    if (res != null && res.VP) MainPlayer.Pay(Stats.Age); //ignore golden age bonus
      //    NextPlayer();
      //    AAnimations.AfterAnimation = aa;
      //  };
      //  PrepareVariableNumberResourcePicker(new Dictionary<string, int> { { card.X.astring("res"), card.X.aint("n") }, { "vp", 1 } }, "golden age");
      //}
    }



    #endregion
    #region helpers TODO:check which ones active
    public bool IsUnambiguousBuy(AField field) //same as IsOneStepBuy TODO: eliminate one of them!
    {
      var card = field.Card;
      var possible = GetPossiblePlacesForCard(field).ToArray();
      var isciv = card.civ();
      return !isciv || possible.Length == 1;
    }
    public void Buy(AField fieldBuy, AField fieldPlace = null)//assumes unambiguous buy if called with 1 param
    {
      ShowBuyButton = false;
      var card = fieldBuy.Card;
      card.CanBuy = false;

      if (card.civ())
      {
        if (fieldPlace == null) fieldPlace = GetPossiblePlacesForCard(fieldBuy).First();
        MainPlayer.Civ.Add(card, fieldPlace);
        Progress.Remove(fieldBuy);
        MainPlayer.Pay(card.Cost);
        //NextPlayer();//nein, er macht die resourceupdate animation und geht automatisch zu nextplayer!
      }
      else
      {
        //BUG!!! bei golden age und bei battle gibts ein problem weil 2x resource update und daher potentiell 2x
        // NextPlayer macht! >muss das loskoppeln!
        Progress.Remove(fieldBuy);
        MainPlayer.Pay(card.Cost);
        if (card.war()) { Stats.UpdateWarPosition(MainPlayer, card); }//auch hier gibts resource update
        else if (card.golden()) BuyGoldenAge0(card);
        else if (card.battle())
        {
          AAnimations.AfterAnimation = null; // (x) => NextPlayer(); // ani in ui ausgeloest onResourceUpdate
          PrepareResourcePicker(new string[] { "wheat", "coal", "book" }, MainPlayer.RaidValue, "battle");
        }
      }
    }

    public bool ArchitectAvailable { get { return (Stats.Architects > 0 || MainPlayer.HasPrivateArchitect); } }

    public int CalcArchitectCost() //returns -1 if cannot buy architect
    {
      if (ArchitectAvailable)
      {
        var wicField = MainPlayer.WIC;
        var wicCard = MainPlayer.WIC.Card;
        if (!wicField.IsEmpty && wicCard.Type == "wonder")
        {
          var archNeeded = wicCard.X.astring("arch").Split('_'); // returns eg{"2","2"}
          var indexNeeded = archNeeded.Length - wicCard.NumDeployed - 1;
          var coalNeeded = int.Parse(archNeeded[indexNeeded]);
          if (MainPlayer.Res.n("coal") >= coalNeeded) return coalNeeded;
        }
      }
      return -1;
    }

    bool CalculateCanBuy(AField field)
    {
      var card = field.Card;
      var canbuy = false;
      if (card != null)
      {
        card.Cost = 3 - field.Row; //ignores special rules
        canbuy = card.Cost <= MainPlayer.Res.n("gold");
        var type = card.Type;
        if (type == "battle") canbuy = canbuy && MainPlayer.Cards.Where(x => x.Type == "military").Any(x => x.NumDeployed > 0);
        if (type == "war") canbuy = canbuy && !Stats.IsWar;
        if (type == "colony") canbuy = canbuy && MainPlayer.Res.n("military") >= card.X.aint("milmin");
      }
      return canbuy;
    }
    public IEnumerable<AField> GetPossiblePlacesForCard(AField field)
    {
      Console.WriteLine(MainPlayer.Name);
      var card = field.Card;
      Debug.Assert(card != null, "GetPossiblePlacesForCard called with null card!");
      return MainPlayer.Civ.Fields.Where(x => x.TypesAllowed.Contains(card.Type)).ToArray();
    }
    public void MarkPossiblePlaces(AField field)
    {
      UnmarkPlaces();
      foreach (var f in GetPossiblePlacesForCard(field)) f.Card.CanActivate = true;
    }
    //public void UnselectCards()
    //{
    //  foreach (var f in Progress.Fields.Where(x => x.Card != null)) f.Card.IsSelected = false;
    //  foreach (var f in MainPlayer.Civ.Fields) f.Card.IsSelected = false;
    //}


    #endregion



    //*********************************************************************
    #region control_flow generalized + placeholder tasks (geben nur messages aus und warten)
    public async void GameLoopGeneralized()
    {
      try
      {
        StartGame();
        while (ird < rounds)
        {
          await DoStuffAtBeginningOfRound();
          while (iph < phases.Length)
          {
            await DoStuffAtBeginningOfPhase();
            while (iturnround < turnrounds[iph])
            {
              await DoStuffAtBeginningOfTurnRound();

              while (ipl < NumPlayers)
              {
                MainPlayer = Players[ipl];
                MainPlayer.NumActions = actPerMoveDefault[iph];
                await DoStuffAtBeginningOfPlayerMove();

                while (iact < MainPlayer.NumActions)
                {
                  await DoStuffAtBeginningOfPlayerAction();

                  if (Interrupt) throw (new Exception("STOPPED BY PLAYER!"));

                  //await WaitForButtonClick();
                  MainPlayer.Res.inc("gold", 5);
                  await WaitForAnimationQueueCompleted();
                  LongMessage = "Button Clicked!!!"; //var actionDone = true; //await DoAction();

                  await DoStuffAtEndOfPlayerAction();
                  iact++;
                }

                iact = 0;
                if (Interrupt) throw (new Exception("Ended!"));
                await DoStuffAtEndOfPlayerMove();
                ipl++;
              }
              ipl = 0;
              if (Interrupt) throw (new Exception("Ended!"));
              await DoStuffAtEndOfTurnRound();
              iturnround++;
            }
            iturnround = 0;
            if (Interrupt) throw (new Exception("Ended!"));
            await DoStuffAtEndOfPhase();
            iph++;
          }
          iph = 0;
          if (Interrupt) throw (new Exception("Ended!"));
          await DoStuffAtEndOfRound();
          ird++;
        }
        LongMessage = "game is over";
      }
      catch (Exception e)
      {
        LongMessage = "interrupt!!!: " + e.Message;
      }
    }
    async Task DoStuffAtBeginningOfRound() { LongMessage = "round " + ird + " is preparing..."; await Task.Delay(shortDelay); LongMessage = "round " + ird + " ready!"; Console.WriteLine(LongMessage); }
    async Task DoStuffAtBeginningOfPhase() { LongMessage = "phase " + iph + " is preparing..."; await Task.Delay(shortDelay); LongMessage = "phase " + iph + " ready!"; Console.WriteLine("\t" + LongMessage); }
    async Task DoStuffAtBeginningOfTurnRound() { LongMessage = "turn " + iturnround + " is preparing..."; await Task.Delay(shortDelay); LongMessage = "turn " + iturnround + " ready!"; Console.WriteLine("\t\t" + LongMessage); }
    async Task DoStuffAtBeginningOfPlayerMove() { LongMessage = "move for " + MainPlayer.Name + " is preparing..."; await Task.Delay(shortDelay); LongMessage = MainPlayer.Name + " ready to move"; Console.WriteLine("\t\t\t" + LongMessage); }
    async Task DoStuffAtBeginningOfPlayerAction() { LongMessage = "action for " + MainPlayer.Name + " is preparing..."; await Task.Delay(shortDelay); LongMessage = MainPlayer.Name + " next action!"; Console.WriteLine("\t\t\t\t" + LongMessage); }
    async Task DoStuffAtEndOfRound() { LongMessage = "round " + ird + " is ending..."; await Task.Delay(longDelay); LongMessage = "round " + ird + " over!"; Console.WriteLine(LongMessage); }
    async Task DoStuffAtEndOfPhase() { LongMessage = "phase " + iph + " is ending..."; await Task.Delay(longDelay); LongMessage = "phase " + iph + " over!"; Console.WriteLine("\t" + LongMessage); }
    async Task DoStuffAtEndOfTurnRound() { LongMessage = "turn " + iturnround + " is ending..."; await Task.Delay(longDelay); LongMessage = "turn " + iturnround + " over!"; Console.WriteLine("\t\t" + LongMessage); }
    async Task DoStuffAtEndOfPlayerMove() { LongMessage = "move for " + MainPlayer.Name + " is ending..."; await Task.Delay(longDelay); LongMessage = MainPlayer.Name + " move ends!"; Console.WriteLine("\t\t\t" + LongMessage); }
    async Task DoStuffAtEndOfPlayerAction() { LongMessage = "action " + iact + " for " + MainPlayer.Name + " is ending..."; await Task.Delay(longDelay); LongMessage = MainPlayer.Name + " action " + iact + " ends!"; Console.WriteLine("\t\t\t\t" + LongMessage); }
    #endregion
    #region testing
    //testing wait for animation completed
    public FrameworkElement testui { get; set; } //testing
    public Storyboard testsb { get; set; }
    public void testcompleted(object sb, object param = null)
    {
      (sb as Storyboard).Completed -= testcompleted;
      Console.WriteLine("\t\t\t\t\tsbcompleted!");
    }
    async Task WaitForAnimationCompleted()
    {
      testsb = Storyboards.Scale(testui, TimeSpan.FromSeconds(.3), new Point(1, 1), new Point(5, 2), null, true);
      testsb.Completed += (s, _) => testcompleted(testsb, testui); // brauche garkein completed in wirklichkeit! nur testing!
      testsb.Begin();
      while (testsb.GetCurrentState() == ClockState.Active && testsb.GetCurrentTime() < testsb.Duration) { await Task.Delay(200); }
    }
    public void StartGame()
    {
      IsRunning = true;
      gov = false;
    }
    async Task StartRoundStuff()
    {
      LongMessage = "round " + ird + " is preparing...";
      await Task.Delay(shortDelay);
      LongMessage = "round " + ird + " ready!";
      Console.WriteLine(LongMessage);
    }



    #endregion
    #region other, eg useful code
    public async Task RunStoryboard(Storyboard sb, FrameworkElement ui)
    {
      //sb.Completed += testcompleted; // brauche garkein completed in wirklichkeit! nur testing!
      sb.Begin(ui);
      while (sb.GetCurrentState() == ClockState.Active && sb.GetCurrentTime() < sb.Duration)
      {
        await Task.Delay(100);
      }
    }
    async Task<object> DoAction()
    {
      //var storyboard = (Storyboard)FindResource("fadeOut");
      //await RunStoryboard(storyboard, item);

      LongMessage = phase + " (" + moveCounter++ + ") " + MainPlayer.Name;
      //await Application.Current.Dispatcher.InvokeAsync(() => AAnimations.AniTest(new Point(0, 0)),
      //  DispatcherPriority.Render);
      await Task.Delay(3000);
      return true;
    }
    #endregion
  }
}
