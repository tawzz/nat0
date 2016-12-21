using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
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
  #region design time
  public class AGameDesignTime { public Game Game { get; set; } public AGameDesignTime() { Game = Game.GameInstance; } }
  #endregion
  public partial class Game : DependencyObject, INotifyPropertyChanged
  {
    #region constants
    public const int MAX_PLAYERS = 5;
    public const int MAX_AGE = 2;
    public const int MAX_ROUNDS = 8;
    public static int[] LevelGrowth = { 4, 3, 2, 1 };

    #endregion

    #region properties and constructor/singleton

    public int NumPlayers { get; set; }
    public ObservableCollection<Player> Players { get; set; }
    public Player MainPlayer
    {
      get { return mainPlayer; }
      set
      {
        if (mainPlayer != value)
        {
          var oldmain = mainPlayer;
          mainPlayer = value;
          Message = "Hi!"; //testing!
          if (oldmain != null) oldmain.IsMainPlayer = false;
          if (mainPlayer != null) mainPlayer.IsMainPlayer = true;
          NotifyPropertyChanged();
        }
      }
    }
    Player mainPlayer;
    public Progress Progress { get; set; }
    public Stats Stats { get; set; }

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
    public Action Kickoff { get; private set; }
    public List<Storyboard> AnimationQueue { get; set; }
    public FrameworkElement UIRoundmarker { get; set; }


    //constructor: singleton
    private static readonly Game instance = new Game(); public static Game GameInstance { get { return instance; } }
    private Game() { Initialize(); }

    //initialization helpers
    void InitPlayers(int num)
    {
      Players = new ObservableCollection<Player>();
      string[] names = { "Felix", "Amanda", "Taka", "Franzl", "Bertl" };
      string[] civs = { "america", "egypt", "arabia", "china", "ethiopia" }; //TODO: choose civs,random civs,daten eingeben!
      Brush[] brushes = {
        new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0050EF")), //blue
        new SolidColorBrush(Color.FromArgb(255, 96, 169, 23)),//Green
        new SolidColorBrush(Color.FromArgb(255, 250, 104, 0)),//orange
        new SolidColorBrush(Color.FromArgb(255,170, 0, 255)),//violet
        Brushes.Sienna
      };
      for (int i = 0; i < NumPlayers; i++) Players.Add(new Player(names[i], civs[i], brushes[i], Levels.Chieftain, i));
      MainPlayer = Players[0];
    }

    #endregion

    #region initialize

    void Initialize()
    {
      ResChoices = new ObservableCollection<Res>();
      AnimationQueue = new List<Storyboard>();

      NumPlayers = 2;
      InitPlayers(NumPlayers);
      Progress = new Progress(7);// NumPlayers + 2); //testing
      Stats = new Stats(this);

      SetupForStart();
      //Stats.WarCard = ACard.MakeCard("hyksos_invasion", 1); //testing design
      //Stats.EventCard = ACard.MakeEventCard("attila"); //testing design
    }
    public void SetupForStart() //called after initialization
    {
      Title = "Nations Start!"; Message = "press Start!"; Caption = "Start"; LongMessage = "game initialization complete";
      IsOkStartEnabled = true; Kickoff = GameLoop;
    }

    #endregion

    #region  resource selection/update

    public ObservableCollection<Res> ResChoices { get; set; }
    public bool ShowResChoices { get { return showResChoices; } set { if (showResChoices != value) { showResChoices = value; NotifyPropertyChanged(); } } }
    bool showResChoices;
    public Res SelectedResource { get { return (Res)GetValue(SelectedResourceProperty); } set { SetValue(SelectedResourceProperty, value); } }
    public static readonly DependencyProperty SelectedResourceProperty = DependencyProperty.Register("SelectedResource", typeof(Res), typeof(Game), null);
    public int Number { get; set; }
    public Dictionary<string, int> NumEach { get; set; } //derzeit nicht in verwendung, fuer variable num resource selection

    public void ResourceUpdated(FrameworkElement ui)
    {
      var sb = Storyboards.Scale(ui, TimeSpan.FromSeconds(.3), new Point(1, 1), new Point(5, 2), null, true);
      AnimationQueue.Add(sb);
    }
    public void NumDeployedUpdated(FrameworkElement ui)
    {
      var sb = Storyboards.Scale(ui, TimeSpan.FromSeconds(.3), new Point(1, 1), new Point(5, 2), null, true);
      AnimationQueue.Add(sb);
    }


    #endregion

    //player action parts
    #region 1. determine what user can do & enable
    public bool ArchitectAvailable { get { return (Stats.Architects > 0 || MainPlayer.HasPrivateArchitect); } }
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
      foreach (var f in Progress.Fields.Where(x => x.Card != null)) f.Card.CanBuy = CalculateCanBuy(f);
    }
    public void MarkCiv()
    {
      foreach (var f in MainPlayer.Civ.Fields.Where(x => !x.IsEmpty)) f.Card.CanActivate = true; // refine!
    }
    public void MarkArchitects() { CanTakeArchitect = ArchitectAvailable && MainPlayer.HasWIC && CalcCanAffordArchitect(); }
    public void MarkTurmoils() { CanTakeTurmoil = true; }
    public void MarkWorkers() { CanSelectWorker = MainPlayer.Res.n("worker") > 0 && CanDeploy; }
    public void MarkPossiblePlacesForProgressCard(string type)
    {
      UnmarkPlaces();
      foreach (var f in GetPossiblePlacesForCard(type)) f.Card.CanActivate = true;
    }
    public void MarkPossiblePlacesForWIC()
    {
      UnmarkPlaces();
      foreach (var f in MainPlayer.Civ.Fields.Where(x=>x.Type == "wonder")) f.Card.CanActivate = true;
    }
    public IEnumerable<Field> GetPossiblePlacesForCard(string type)
    {
      Console.WriteLine(MainPlayer.Name);
      //var card = field.Card;
      //Debug.Assert(card != null, "GetPossiblePlacesForCard called with null card!");
      return MainPlayer.Civ.Fields.Where(x => x.TypesAllowed.Contains(type)).ToArray();
    }
    bool CalculateCanBuy(Field field)
    {
      var card = field.Card;
      var canbuy = false;
      if (card != null)
      {
        card.Cost = 3 - field.Row; //ignores special rules
        canbuy = card.Cost <= MainPlayer.Res.n("gold");
        var type = card.Type;
        if (type == "battle")
        {
          var milcards = MainPlayer.Cards.Where(x => x.Type == "military").ToArray();
          var havemil = milcards.Any(x => x.NumDeployed > 0);
          canbuy = canbuy && MainPlayer.Cards.Where(x => x.Type == "military").Any(x => x.NumDeployed > 0);
        }
        if (type == "war") canbuy = canbuy && !Stats.IsWar;
        if (type == "colony") canbuy = canbuy && MainPlayer.Res.n("military") >= card.X.aint("milmin");
      }
      return canbuy;
    }
    public int CalcArchitectCost(Card card)
    {
      Debug.Assert(card != null, "CalcArchitectCost for null card called!");
      var costs = Card.GetArchCostArray(card);
      var idx = card.NumDeployed;
      Debug.Assert(idx <= costs.Length, "CalcArchitectCost for wonder that was already ready!!!");
      return costs[idx];
    }
    public int NumArchitects(Card card) { return Card.GetArchCostArray(card).Length; }
    public bool CalcCanAffordArchitect() { return MainPlayer.Res.n("coal") >= CalcArchitectCost(MainPlayer.WIC.Card); }
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
    public void OnClickWorker(Res res)
    {
      if (CanSelectWorker)// eliminate check
      {
        WorkerSelected = true;
        UnselectTurmoils(); UnselectArchitects(); UnselectProgress(); UnselectPreviousCiv();
        if (SelectedCivField != null) { Message = "Deploy?"; SelectedAction = DeployAvailableWorker; }
        else { Message = "Select a field to which to deploy"; SelectedAction = PartialDeploy; }
      }
    }
    public void OnClickProgressCard(Field field)
    {
      UnselectCiv(); UnselectPreviousCiv(); UnselectArchitects(); UnselectWorker(); UnselectTurmoils();
      var isNewField = field != SelectedProgressField;
      UnselectProgress();
      if (isNewField)
      {
        SelectedProgressField = field;
        SelectedProgressField.Card.IsSelected = true;
        if (IsOneStepBuy(field)) { Message = "Buy " + field.Card.Name + "?"; SelectedAction = BuyProgressCard; }
        else { Message = "Select Place on Civ Board"; MarkPossiblePlacesForProgressCard(field.Card.Type); SelectedAction = PartialBuy; }
      }
    }
    public void OnClickCivCard(Field field)
    {
      UnselectTurmoils();
      PreviousSelectedCivField = SelectedCivField;
      SelectedCivField = field;

      if (PreviousSelectedCivField == SelectedCivField)
      {
        UnselectCiv(); UnselectPreviousCiv();
      }
      else if (field.Type == "wonder") //selecting place for wonder ready
      {
        field.Card.IsSelected = true;
        Message = "You selected field for new wonder";
      }
      else if (SelectedProgressField != null && (field.Card.buildmil() || field.Card.colony()))
      { //Buy
        UnselectWorker(); UnselectArchitects(); UnselectPreviousCiv();
        field.Card.IsSelected = true;
        Message = "Buy " + field.Card.Name + "?";
        SelectedAction = BuyProgressCard;
      }
      else if (field.Index == CardType.WIC && CanTakeArchitect)
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
    bool IsOneStepBuy(Field field)
    {
      var card = field.Card;
      var possible = GetPossiblePlacesForCard(card.Type).ToArray();
      var isciv = card.civ();
      return !isciv || possible.Length == 1;
    }

    Field SelectedProgressField { get; set; }
    Field SelectedCivField { get; set; }
    Field PreviousSelectedCivField { get; set; }
    public bool ArchitectSelected { get { return architectSelected; } set { if (architectSelected != value) { architectSelected = value; NotifyPropertyChanged(); } } }
    bool architectSelected;
    public bool TurmoilSelected { get { return turmoilSelected; } set { if (turmoilSelected != value) { turmoilSelected = value; NotifyPropertyChanged(); } } }
    bool turmoilSelected;
    public bool WorkerSelected { get { return MainPlayer.Res.get("worker").IsSelected; } set { MainPlayer.Res.get("worker").IsSelected = value; NotifyPropertyChanged(); } }
    #endregion

    #region 3. perform resulting actions: SelectedAction, tasks
    //umbauen zu:
    //public enum ActionStep { PartialBuy, PartialDeploy,TakeArchitect,TakeTurmoil,BuyDeployFromField,DeployFromWorkers}
    public Action SelectedAction { get; set; }
    public void PartialBuy() { }//dummy
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
    public void TakeArchitect() { }//dummy
    public async Task TakeArchitectTask()
    {
      var card = MainPlayer.WIC.Card;
      Debug.Assert(card != null, "TakeArchitect with empty wic!");

      var cost = CalcArchitectCost(card);
      MainPlayer.Pay(cost, "coal");

      card.NumDeployed++;
      if (card.NumDeployed >= NumArchitects(card)) await WonderReadyTask();

      Stats.Architects--;

      Message = MainPlayer.Name + " hired an architect";
    }
    public async Task WonderReadyTask()
    {
      UnselectAll();
      MarkPossiblePlacesForWIC();
      Debug.Assert(SelectedCivField == null, "WonderReadyTask: started with SelectedCivField != null");
      while (SelectedCivField == null)
      {
        Message = "pick a wonder space";
        await WaitForButtonClick();
      }
      MainPlayer.MoveCivCard(MainPlayer.WIC.Card, MainPlayer.WIC, SelectedCivField);
      SelectedCivField.Card.NumDeployed = 0;
      //SelectedCivField = null;
    }
    public void TakeTurmoil() { Message = "not implemented"; }
    public void BuyProgressCard() { }//dummy
    public async Task BuyProgressCardTask()
    {
      var card = SelectedProgressField.Card;
      var fieldBuy = SelectedProgressField;
      var fieldPlace = SelectedCivField;
      card.CanBuy = false;

      if (card.civ())
      {
        if (fieldPlace == null) fieldPlace = GetPossiblePlacesForCard(fieldBuy.Card.Type).First();
        MainPlayer.AddCivCard(card, fieldPlace);
        Progress.Remove(fieldBuy);
        MainPlayer.Pay(card.Cost);
      }
      else
      {
        Progress.Remove(fieldBuy);
        MainPlayer.Pay(card.Cost);
        if (card.war()) { Stats.UpdateWarPosition(MainPlayer, card); }
        else if (card.golden()) await BuyGoldenAgeTask(card.X.astring("res"), card.X.aint("n"));
        //{
        //  var resname = card.X.astring("res");
        //  var num = card.X.aint("n");
        //  var canaffordvp = MainPlayer.Res.n("gold") >= Stats.Age;
        //  if (canaffordvp)
        //  {
        //    var res = await WaitForPickResourceCompleted(new string[] { resname, "vp" }, num, "golden age");
        //    if (res.Name=="vp") MainPlayer.Pay(Stats.Age);
        //  }
        //else
        //{
        //  MainPlayer.UpdateResBy(resname, num);
        //}
        //}
        else if (card.battle()) { await WaitForPickResourceCompleted(new string[] { "wheat", "coal", "book" }, MainPlayer.RaidValue, "battle"); }
      }
      Message = MainPlayer.Name + " bought " + card.Name;
    }
    public async Task BuyGoldenAgeTask(string resname, int num)
    {
      var canaffordvp = MainPlayer.Res.n("gold") >= Stats.Age;
      if (canaffordvp)
      {
        var res = await WaitForPickResourceCompleted(new string[] { resname, "vp" }, num, "golden age");
        if (res.Name == "vp") MainPlayer.Pay(Stats.Age);
      }
      else { MainPlayer.UpdateResBy(resname, num); }
    }
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
        MainPlayer.Pay(deploymentCost, "coal");
        Message = MainPlayer.Name + " deployed from " + sourceCard.Name + " to " + targetCard.Name;
      }
    }
    public void DeployAvailableWorker()
    {
      var card = SelectedCivField.Card;
      card.NumDeployed++;
      var deploymentCost = Stats.Age; //simplified
      MainPlayer.Pay(deploymentCost, "coal");
      MainPlayer.Res.dec("worker", 1);//TODO: ersetze durch Deploy in APlayer
      Message = MainPlayer.Name + " deployed to " + card.Name;
    }

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
      while (AnimationQueue.Count < minAnimations) await Task.Delay(200);//give ui time to trigger resourceUpdated event
      Console.WriteLine(LongMessage = "Animation Queue ready -  starting animations...");
      while (AnimationQueue.Count > 0)
      {
        var sb = AnimationQueue[0];
        AnimationQueue.RemoveAt(0);
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
    async Task<Res> MakeSureUserPicksAResource()
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
    async Task<Res> WaitForPickResourceCompleted(IEnumerable<string> resnames, int num, string forWhat)
    {
      Debug.Assert(ResChoices != null && ResChoices.Count == 0, "ResChoices not cleared for growth resource pick");
      Debug.Assert(SelectedResource == null, "start resource pick with resource already selected!");

      foreach (var rname in resnames) ResChoices.Add(new Res(rname));
      Number = num;
      ShowResChoices = true; Caption = "Pick"; Message = MainPlayer.Name + ", pick " + forWhat + " resource";

      var res = await MakeSureUserPicksAResource();

      ShowResChoices = false; ResChoices.Clear(); SelectedResource = null;
      Message = MainPlayer.Name + " picked " + res.Name.ToCapital();

      if (res.Name == "vp") Number = 1;

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
          foreach (var rname in resarr) ResChoices.Add(new Res(rname));
          ShowResChoices = true; Message = "pick worker type";

          res = await MakeSureUserPicksAResource();
          ShowResChoices = false; ResChoices.Clear(); SelectedResource = null;

          if (res.Name == wfree2.CostRes) wfree1 = wfree2;
        }
        Message = MainPlayer.Name + " picked " + wfree1.CostRes.ToCapital() + " worker";
        MainPlayer.CheckOutWorker(wfree1);
      }
      else
      {
        MainPlayer.UpdateResBy(res.Name, Number);
      }
      return res;
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


    #region other safe helpers
    public void RandomPlayerOrder()
    {
      var plarr = Players.OrderBy(x => Rand.N()).ToArray();
      Players.Clear();
      foreach (var pl in plarr) Players.Add(pl);
      for (int i = 0; i < Players.Count; i++) Players[i].Index = i;
      MainPlayer = Players[0];
    }
    public event PropertyChangedEventHandler PropertyChanged; public void NotifyPropertyChanged([CallerMemberName] string propertyName = null) { this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
    #endregion



























  }
}
