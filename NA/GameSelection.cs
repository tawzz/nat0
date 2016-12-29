using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using ations;

namespace ations
{
  public enum cl { none, prog, civ, worker, arch, turm, pspecial, cspecial, res }
  public enum ctx { none, start, special, wready }

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
      else if (context == ctx.start)
      {
        MarkPossibleProgressCards();
        MarkArchitects();
        MarkTurmoils();
        MarkWorkers();
        MarkCiv();
        if (ProgressField != null) { MarkPossiblePlacesForProgressCard(ProgressField.Card.Type); }
      }
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
      foreach (var f in GetPossiblePlacesForType(type)) f.Card.IsEnabled = true;
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
    public bool ActionComplete { get; set; }

    ContextInfo Context { get; set; }
    Stack<ContextInfo> ContextStack { get; set; }
    List<Step> Steps { get { Debug.Assert(Context != null, "Steps accessed: Context is null"); return Context.Steps; } }
    Step Step { get { Debug.Assert(Context.Steps != null, "Step accessed: Context.Steps is null!"); return Context.Steps.LastOrDefault(); } }

    public void OnClickProgressField(Field field) { UpdateSteps(new Step(cl.prog, field)); }
    public void OnClickCivField(Field field) { UpdateSteps(new Step(cl.civ, field)); }
    public void OnClickArchitect() { UpdateSteps(new Step(cl.arch)); }
    public void OnClickTurmoil() { Message = "no implemented"; }
    public void OnClickCivResource(Res res) { UpdateSteps(new Step(cl.worker, res)); }

    void ContextInit(ctx newContext, string msg = "", bool clearStepsIfSameContext = true)
    {
      if (Context == null) { Context = new ContextInfo { Steps = new List<ations.Step>(), Id = newContext, BaseMessage = msg }; }
      else if (Context.Id == newContext && clearStepsIfSameContext) { Context.Steps = new List<Step>(); Context.BaseMessage = msg; }
      else { ContextStack.Push(Context); Context = new ContextInfo { Steps = new List<Step>(), Id = newContext, BaseMessage = msg }; }
      SetContextMessage();
      Context.StepDictionary = contextDictionaries[newContext];
    }
    void SetContextMessage() { Message = Context.BaseMessage.Replace("$Player", MainPlayer.Name); }
    void ContextEnd() { if (ContextStack.Count > 0) Context = ContextStack.Pop(); SetContextMessage(); }
    void ClearSteps() { Context.Steps.Clear(); }
    void UpdateSteps(Step newStep)
    {

      ActionComplete = RefreshSequence(newStep);
      Console.WriteLine("complete=" + ActionComplete);
      UpdateUI();
    }
    bool RefreshSequence(Step newStep)
    {
      Debug.Assert(Context != null, "RefreshSteps: Context null");
      Debug.Assert(Context.Steps != null, "RefreshSteps: Context.Steps NULL bei RefreshSequence!");
      Debug.Assert(Context.StepDictionary != null, "RefreshSteps: Context.StepDictionary null!");

      // if clicked a selected object, unselect and roll back to previous to clicking that object
      var newSteps = Steps.Any(x => x.Obj == newStep.Obj) ? EraseAllStepsBeforeAndIncludingObject(newStep.Obj) : Steps.Plus(newStep);

      //Console.WriteLine("\tSteps:" + Steps.Count+", newSteps:"+newSteps.Count());

      if (newSteps.Count() == 0) { Context.Steps = newSteps.ToList(); return false; }

      var revsteps = newSteps.ToArray().Reverse().ToList();
      List<Step> result = new List<Step>();
      cl[] key = null;
      var dict = Context.StepDictionary;
      bool complete = false;

      foreach (var k in dict.Keys)
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

          for (int j = 0; j < result.Count; j++)
          {
            Debug.Assert(result[j].Click == key[j], "RefreshSteps: Clicks do not match in matching step sequence");
            result[j].IsValid = dict[key][j].IsValid;
            result[j].Process = dict[key][j].Process;
            result[j].UndoProcess = dict[key][j].UndoProcess;
            result[j].Message = dict[key][j].Message;
          }

          var currentStep = result.LastOrDefault(); 
          Debug.Assert(currentStep != null, "RefreshSteps: currentStep null!");
          if (currentStep.IsValid != null && !currentStep.IsValid(result)) continue;
          currentStep.Process?.Invoke(result);

          if (i == 0) { complete = true; break; } //got complete action
        }
      }

      Context.Steps = result; // if no match found: get back empty step list in same context
      return complete;
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
        if (fieldPlace == null) fieldPlace = GetPossiblePlacesForType(fieldBuy.Card.Type).FirstOrDefault();
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
      Debug.Assert(card != null, "bought null card!!!");
      Message = MainPlayer.Name + " bought " + card.Name;
      if (card.colony() || card.adv() || card.dyn()) MainPlayer.UpdateStabAndMil();
    }
    public async Task BuyGoldenAgeTask(string resname, int num)
    {
      //can pay with all resources except
      var respay = MainPlayer.Res.List.Where(x => x.CanPayWith).ToList();
      var canaffordvp = respay.Sum(x => x.Num) >= Stats.Age; 
      if (canaffordvp)
      {
        var res = await WaitForPickResourceCompleted(new string[] { resname, "vp" }, num, "golden age");
        if (res.Name == "vp")
        {
          var reslist = await WaitForPickMultiResourceCompleted(respay, Stats.Age, "vp");
          foreach(var rpay in reslist) MainPlayer.Pay(rpay.Num,rpay.Name);
        }
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
      if (card.NumDeployed >= NumArchitects(card))
      {
        await WaitForAnimationQueueCompleted();
        await WonderReadyTask();
      }

      Stats.Architects--;

      Message = MainPlayer.Name + " hired an architect";
    }
    public async Task WonderReadyTask()
    {
      ContextInit(ctx.wready);
      UpdateUI();
      while (Step == null)
      {
        Message = "pick a wonder space";
        await WaitForButtonClick();
      }
      MainPlayer.WonderReady(Step.Obj as Field);
      //enqueue wonderReadyAnimation
      ContextEnd();
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
      if (AnimationQueue.Count == 0) { await Task.Delay(500); }
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
