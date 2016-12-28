using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace ations
{
  public enum cl { none, prog, cciv, worker, arch, turm, pspecial, cspecial, res }// p1, p2, res, pbm, pw, pnat, pb, pwar, pga, cbm, cbmwork, cact, cplace, cnat, cw, cdyn, work, ok, pass, cancel, respi, choicepi, multipi }
  public enum ctx { none, start, special, pbuy2, wready } //, deploy, def }

  public partial class Game
  {
    #region 1. determine what user can do & enable
    public bool ArchitectEnabled { get { return architectEnabled; } set { architectEnabled = value; NotifyPropertyChanged(); } }
    bool architectEnabled;
    public bool TurmoilEnabled { get { return turmoilEnabled; } set { turmoilEnabled = value; NotifyPropertyChanged(); } }
    bool turmoilEnabled;
    public bool WorkerEnabled { get { return MainPlayer.Res.get("worker").IsSelectable; } set { MainPlayer.Res.get("worker").IsSelectable = value; NotifyPropertyChanged(); } }
    public bool IsOkStartEnabled { get { return isOkStartEnabled; } set { if (isOkStartEnabled != value) { isOkStartEnabled = value; NotifyPropertyChanged(); } } }
    bool isOkStartEnabled;
    public bool IsPassEnabled { get { return isPassEnabled; } set { if (isPassEnabled != value) { isPassEnabled = value; NotifyPropertyChanged(); } } }
    bool isPassEnabled;
    public bool IsCancelEnabled { get { return isCancelEnabled; } set { if (isCancelEnabled != value) { isCancelEnabled = value; NotifyPropertyChanged(); } } }
    bool isCancelEnabled;

    public void MarkAllPlayerChoicesAccordingToContext(ctx context)
    {
      if (context == ctx.wready) { MarkPossiblePlacesForWIC(); return; }
      MarkPossibleProgressCards();
      MarkArchitects();
      MarkTurmoils();
      MarkWorkers();
      MarkCiv();
      if (context == ctx.pbuy2) { MarkPossiblePlacesForProgressCard(ProgressField.Card.Type); }
    }
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
      //UnmarkProgresscards();
      foreach (var f in Progress.Fields.Where(x => x.Card != null)) f.Card.IsEnabled = CalculateCanBuy(f);
    }
    public void MarkCiv() //enables ALL non empty cards for now!
    {
      foreach (var f in MainPlayer.Civ.Fields.Where(x => !x.IsEmpty)) f.Card.IsEnabled = true; // refine!
    }
    public void MarkArchitects() { ArchitectEnabled = ArchitectAvailable && MainPlayer.HasWIC && CalcCanAffordArchitect(); }
    public void MarkTurmoils() { TurmoilEnabled = true; }
    public void MarkWorkers() { WorkerEnabled = MainPlayer.Res.n("worker") > 0 && CanDeploy; }
    public void MarkPossiblePlacesForProgressCard(string type)
    {
      //UnmarkPlaces();
      foreach (var f in GetPossiblePlacesForCard(type)) f.Card.IsEnabled = true;
    }
    public void MarkPossiblePlacesForWIC()
    {
      //UnmarkPlaces();
      foreach (var f in MainPlayer.Civ.Fields.Where(x => x.Type == "wonder")) f.Card.IsEnabled = true;
    }

    #endregion

    #region 2. get user click & select objects:

    public bool OkStartClicked { get; set; }
    public bool CancelClicked { get; set; }
    public bool PassClicked { get; set; }
    public bool ArchitectSelected { get { return architectSelected; } set { if (architectSelected != value) { architectSelected = value; NotifyPropertyChanged(); } } }
    bool architectSelected;
    public bool TurmoilSelected { get { return turmoilSelected; } set { if (turmoilSelected != value) { turmoilSelected = value; NotifyPropertyChanged(); } } }
    bool turmoilSelected;
    public bool WorkerSelected { get { return MainPlayer.Res.get("worker").IsSelected; } set { MainPlayer.Res.get("worker").IsSelected = value; NotifyPropertyChanged(); } }
    public Field DeployTarget { get { var st = Steps.FirstOrDefault(x => x.Obj is Field); return st != null ? st.Obj as Field : null; } }// objects.FirstOrDefault(x => (x as Field) != null) as Field; } }
    public Field DeploySource { get { return Steps.Count > 1 ? Steps[1].Obj as Field : null; } }
    public Field ProgressField { get { return Steps.Count > 0 && Steps[0].Click == cl.prog ? Steps[0].Obj as Field : null; } }
    public Field CivBoardPlace { get { return Steps.Count > 1 ? Steps[1].Obj as Field : null; } }
    public Field SelectedField { get; set; }// all purpose field selection, eg., wonder ready 
    public bool ActionComplete { get; set; }
    List<Step> Steps = new List<Step>();
    ctx Context; //current context
    Info Info; //current info (message,validation action and action)

    public void OnClickProgressField(Field field)
    {
      var interpretation = Context == ctx.special ? new Step(cl.pspecial, field) //unused for now
        : IsOneStepBuy(field) ? new Step(cl.prog, field) : new Step(cl.prog, field, ctx.pbuy2);
      UpdateSteps(interpretation);
    }
    public void OnClickCivField(Field field)
    {
      var interpretation = Context == ctx.special ? new Step(cl.cspecial, field)
        : Context == ctx.wready ? new Step(cl.cciv, field, ctx.wready)
        : new Step(cl.cciv, field);
      UpdateSteps(interpretation);
    }
    public void OnClickArchitect() { UpdateSteps(new Step(cl.arch)); }
    public void OnClickTurmoil() { Message = "no implemented"; }
    public void OnClickCivResource(Res res)
    {
      var interpretation = Context == ctx.special ? new Step(cl.res, res) : new Step(cl.worker, res);
      UpdateSteps(interpretation);
    }

    void UpdateSteps(Step stp)
    {
      Steps = RefreshSequence(stp);
      UpdateContextAndMessage(Steps);
      UpdateUI(Context);
    }
    List<Step> RefreshSequence(Step stp)
    {
      var newsteps = Steps.Any(x => x.Obj == stp.Obj) ? EraseAllStepsBeforeAndIncludingObject(stp.Obj) : Steps.Plus(stp);
      if (newsteps.Count() == 0) return new List<Step>();

      var revsteps = newsteps.ToArray().Reverse().ToList();
      List<Step> result = null; cl[] key = null;

      foreach (var k in dStart.Keys)
      {
        var rk = k.Reverse().ToArray();
        var len = rk.Length;
        int i = 0;
        while (i < len && rk[i] != revsteps[0].Click) i++;
        if (i >= len) continue; //not a match at all

        var success = true;
        for (int j = i; j < len; j++) { if (j - i >= revsteps.Count || revsteps[j - i].Click != rk[j]) { success = false; break; } } // not a match!

        if (success)
        {
          result = revsteps.ToArray().Take(len - i).Reverse().ToList();
          key = k;
          Info = dStart[k][result.Count - 1];

          var totalSteps = key.Length;
          var stepsSoFar = result.Count;
          Info.Process?.Invoke(result);
          if (Info.IsValid != null && !Info.IsValid(result)) continue;

          if (i == 0) { ActionComplete = true; break; } //got complete action
        }
      }

      return result ?? new List<Step>();
    }

    #endregion

    #region 3. perform resulting actions: tasks

    public async Task BuyProgressCardTask()
    {
      var card = ProgressField.Card;
      var fieldBuy = ProgressField;
      var fieldPlace = CivBoardPlace;
      //card.IsEnabled = false;

      if (card.civ())
      {
        if (fieldPlace == null) fieldPlace = GetPossiblePlacesForCard(fieldBuy.Card.Type).FirstOrDefault();
        Debug.Assert(fieldPlace != null, "BuyProgressCard: so ein MIST!!!!");
        var needRaidUpdate = MainPlayer.AddCivCard(card, fieldPlace);
        Progress.Remove(fieldBuy);
        MainPlayer.Pay(card.Cost);

        //actions after buy building/military
        if (needRaidUpdate) { MainPlayer.RecomputeRaid(); }//TODO: special modifications to raid if necessary
      }
      else
      {
        Progress.Remove(fieldBuy);
        MainPlayer.Pay(card.Cost);
        if (card.war()) { Stats.UpdateWarPosition(MainPlayer, card); }
        else if (card.golden()) await BuyGoldenAgeTask(card.X.astring("res"), card.X.aint("n"));
        else if (card.battle()) { await WaitForPickResourceCompleted(new string[] { "wheat", "coal", "book" }, MainPlayer.RaidValue, "battle"); }
      }
      Message = MainPlayer.Name + " bought " + card.Name;
      //actions after buy building/military


      //weired!!!!!!!!!!!!!!!!!!!! vs hat da irgendwo ein breakpoint falsch?!?
      var iscol = card.colony();
      var isadv = card.adv();
      var iscoloradv = iscol || isadv;
      if (iscoloradv)
      {
        int n = 4;
        MainPlayer.UpdateStabAndMil();
      }
      else
      {
        int n = 5;
      }
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
      var sourceCard = DeploySource.Card;
      var targetCard = DeployTarget.Card;
      sourceCard.NumDeployed--;

      DeployTo(targetCard);

      Message = MainPlayer.Name + " deployed from " + sourceCard.Name + " to " + targetCard.Name;
    }
    public void DeployAvailableWorker()
    {
      var card = DeployTarget.Card;
      MainPlayer.DeployWorker();
      DeployTo(card);
      Message = MainPlayer.Name + " deployed to " + card.Name;
    }
    public void DeployTo(Card card)
    {
      card.NumDeployed++;
      var deploymentCost = Stats.Age; //simplified
      MainPlayer.Pay(deploymentCost, "coal");
      MainPlayer.UpdateStabAndMil();
      if (card.mil()) MainPlayer.RecomputeRaid(); //recheck special cards
    }
    public async Task TakeArchitectTask()
    {
      var card = MainPlayer.WICField.Card;
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
      Context = ctx.wready;
      UpdateUI(Context);
      SelectedField = null;
      while (SelectedField == null)
      {
        Message = "pick a wonder space";
        await WaitForButtonClick();
      }
      MainPlayer.WonderReady(SelectedField);
    }
    public async Task TakeTurmoilTask() { Message = "not implemented"; await Task.Delay(200); }

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
    async Task WaitForThreeButtonClick()
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

    #endregion



  }
}
