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
  public enum ctx { none, start, pickCivField, special, wready, swapprogress, pickProgress, removeWorker, deployWorker, removeMilitaryWorker, countClickMilitaryWorkers }

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
      if (context == ctx.pickCivField) { MarkPreselectedFields(); return; }
      else if (context == ctx.removeMilitaryWorker || context == ctx.countClickMilitaryWorkers) { MarkMilitaryCardsWithWorkers(); return; }
      else if (context == ctx.removeWorker) { MarkBuildMilCardsWithWorkers(); return; }
      else if (context == ctx.deployWorker) { MarkBuildMilCards(); return; }
      else if (context == ctx.wready) { MarkPossiblePlacesForWIC(); return; }
      else if (context == ctx.swapprogress || context == ctx.pickProgress) { MarkProgressCards(); return; }
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
    public void MarkProgressCards() { foreach (var f in Progress.Fields.Where(x => x.Card != null)) f.Card.IsEnabled = true; }
    public void MarkProgressCardsOfType(string type) { foreach (var f in Progress.Fields.Where(x => x.Card != null && x.Card.Type == type)) f.Card.IsEnabled = true; }
    public void MarkPossibleProgressCards() { foreach (var f in Progress.Fields.Where(x => x.Card != null)) f.Card.IsEnabled = CalculateCanBuy(f); }
    public void MarkCiv()
    {
      foreach (var f in MainPlayer.Civ.Fields.Where(x => !x.IsEmpty))
      {
        var card = f.Card;
        if (card.CanDeployOn && MainPlayer.Res.n("coal") >= Stats.Age) card.IsEnabled = true;
        if (Checker.CheckActionPossible(MainPlayer, f.Card)) card.IsEnabled = true;
      }
      foreach(var choice in MainPlayer.SpecialOptions)
      {
        if (Checker.CheckActionPossible(MainPlayer, choice.Tag as Card)) choice.IsSelectable = true;
      }
    }
    public void MarkArchitects() { ArchitectEnabled = ArchitectAvailable && MainPlayer.HasWIC && MainPlayer.WIC.wonder() && CalcCanAffordArchitect(); }
    public void MarkTurmoils() { TurmoilEnabled = true; }
    public void MarkWorkers() { WorkerEnabled = MainPlayer.Res.n("worker") > 0 && CanDeploy; }
    public void MarkPossiblePlacesForProgressCard(string type) { foreach (var f in MainPlayer.GetPossiblePlacesForType(type)) f.Card.IsEnabled = true; }
    public void MarkPossiblePlacesForWIC() { foreach (var f in MainPlayer.Civ.Fields.Where(x => x.Type == "wonder")) f.Card.IsEnabled = true; }
    public void MarkMilitaryCardsWithWorkers() { foreach (var f in MainPlayer.Civ.Fields.Where(x => !x.IsEmpty && x.Card.mil() && x.Card.NumDeployed > 0)) f.Card.IsEnabled = true; }
    public void MarkPreselectedFields() { foreach (var f in PreselectedFields) f.Card.IsEnabled = true; }
    public void MarkBuildMilCardsWithWorkers() { foreach (var f in MainPlayer.Civ.Fields.Where(x => !x.IsEmpty && x.Card.buildmil() && x.Card.NumDeployed > 0)) f.Card.IsEnabled = true; }
    public void MarkBuildMilCards() { foreach (var f in MainPlayer.Civ.Fields.Where(x => !x.IsEmpty && x.Card.buildmil())) f.Card.IsEnabled = true; }

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
    public bool IsCardActivation { get { return isCardActivation; } set { if (isCardActivation != value) { isCardActivation = value; NotifyPropertyChanged(); } } }
    bool isCardActivation;
    public bool IsSpecialOptionActivation { get { return isSpecialOptionActivation; } set { if (isSpecialOptionActivation != value) { isSpecialOptionActivation = value; NotifyPropertyChanged(); } } }
    bool isSpecialOptionActivation;
    public Field DeployTarget { get { var st = Steps.FirstOrDefault(x => x.Obj is Field); return st != null ? st.Obj as Field : null; } }// objects.FirstOrDefault(x => (x as Field) != null) as Field; } }
    public Field DeploySource { get { return Steps.Count > 1 ? Steps[1].Obj as Field : null; } }
    public Field ProgressField { get { return Steps.Count > 0 && Steps[0].Click == cl.prog ? Steps[0].Obj as Field : null; } }
    public Field CivBoardPlace { get { return Steps.Count > 1 ? Steps[1].Obj as Field : null; } }
    public bool ActionComplete { get; set; }
    ContextInfo Context { get; set; }
    Stack<ContextInfo> ContextStack { get; set; }
    List<Step> Steps { get { Debug.Assert(Context != null, "Steps accessed: Context is null"); return Context.Steps; } }
    Step Step { get { Debug.Assert(Context.Steps != null, "Step accessed: Context.Steps is null!"); return Context.Steps.LastOrDefault(); } }

    public void OnClickSpecialOption(Choice choice) { UpdateSteps(new Step(cl.cspecial, choice)); }
    //{
    //  //DisableAndUnselectAll();
    //  //Message = "execute special action?";

    //  //await Checker.ExecuteAction(MainPlayer, choice.Tag as Card);
    //}
    public void OnClickProgressField(Field field) { UpdateSteps(new Step(cl.prog, field)); }
    public void OnClickCivField(Field field) { UpdateSteps(new Step(cl.civ, field)); }
    public void OnClickArchitect() { UpdateSteps(new Step(cl.arch)); }
    public void OnClickTurmoil() { UpdateSteps(new Step(cl.turm)); }
    public void OnClickCivResource(Res res) { UpdateSteps(new Step(cl.worker, res)); }
    public void OnClickWorkerCounter(Field field) { field.Counter++; if (field.Counter > field.Card.NumDeployed) field.Counter = 0; }
    public void ContextInit(ctx newContext, string msg = "", bool clearStepsIfSameContext = true)
    {
      if (Context == null) { Context = new ContextInfo { Steps = new List<Step>(), Id = newContext, BaseMessage = msg }; }
      else if (Context.Id == newContext && clearStepsIfSameContext) { Context.Steps = new List<Step>(); Context.BaseMessage = msg; }
      else { ContextStack.Push(Context); Context = new ContextInfo { Steps = new List<Step>(), Id = newContext, BaseMessage = msg }; }
      SetContextMessage();
      Context.StepDictionary = contextDictionaries[newContext];
    }
    public void SetContextMessage() { Message = Context.BaseMessage.Replace("$Player", MainPlayer.Name); }
    public void ContextEnd() { if (ContextStack.Count > 0) Context = ContextStack.Pop(); SetContextMessage(); DisableAndUnselectAll(); }
    public void ClearSteps() { Context.Steps.Clear(); }
    public void UpdateSteps(Step newStep)
    {

      ActionComplete = RefreshSequence(newStep);
      //Console.WriteLine("complete=" + ActionComplete);
      UpdateUI();
    }


    public bool RefreshSequence(Step newStep)
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
      bool complete = false; IsCardActivation = IsSpecialOptionActivation=false;

      foreach (var k in dict.Keys)
      {
        var rk = k.Reverse().ToArray();
        var len = rk.Length;

        // try all occurrences of laststep as some match in key
        List<int> beginnings = new List<int>();
        // how many times does cl occur in key?
        var lastclick = revsteps[0].Click;
        int ncl = rk.Count(x => x == lastclick);
        if (ncl == 0) continue;  //not a match at all
        int i = 0;
        for (int icl = 0; icl < ncl; icl++)
        {
          while (i < len && rk[i] != revsteps[0].Click) i++;
          Debug.Assert(i < len || icl == ncl - 1, "Refresh: Algorithm ERROR!!!");
          var success = true;
          for (int j = i; j < len; j++) { if (j - i >= revsteps.Count || revsteps[j - i].Click != rk[j]) { success = false; i++; break; } } // not a match!

          if (success)// test validation functions for each step!!!
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
            if (currentStep.IsValid != null && !currentStep.IsValid(result)) { result.Clear(); continue; }
            currentStep.Process?.Invoke(result);

            if (i == 0) { complete = true; break; } //got complete action
          }//end if success
          if (complete) break;
        }//end for each possible beginning
        if (complete) break;
      }//end for each key

      Context.Steps = result; // if no match found: get back empty step list in same context
      return complete;
    }


    public cl[] FindExactKey(Dictionary<cl[], Step[]> dict, cl[] clicks)
    {
      foreach (var k in dict.Keys) if (SameElements(k, clicks)) return k;
      return null;
    }
    public cl[] FindBeginningOfKey(Dictionary<cl[], Step[]> dict, cl[] clicks, int len)
    {

      foreach (var k in dict.Keys) if (SameElements(k, clicks)) return k;
      return null;
    }
    public bool SameElements(cl[] a1, cl[] a2)
    {
      var same = true;
      if (a1.Length != a2.Length) return false;
      for (int i = 0; i < a1.Length; i++) if (a1[i] != a2[i]) return false;
      return true;
    }
    public static bool FirstIsPrefixOfSecond(cl[] first, cl[] second)
    {
      if (first.Length > second.Length) return false;
      for (int i = 0; i < first.Length; i++) if (first[i] != second[i]) return false;
      return true;
    }
    public static cl[] FindLongestPostfixOfFirstThatIsPrefixOfSecond(cl[] first, cl[] second)
    {
      // cut first to at most same length as second
      if (first.Length > second.Length) first = first.Skip(first.Length - second.Length).ToArray();

      while (!FirstIsPrefixOfSecond(first, second)) { first = first.Skip(1).ToArray(); }

      //now first could be empty!
      return first.Length == 0 ? null : first;
    }
    public bool RefreshSequenceNew(Step newStep)
    {
      Debug.Assert(Context != null, "RefreshSteps: Context null");
      Debug.Assert(Context.Steps != null, "RefreshSteps: Context.Steps NULL bei RefreshSequence!");
      Debug.Assert(Context.StepDictionary != null, "RefreshSteps: Context.StepDictionary null!");

      // if clicked a selected object, unselect and roll back to previous to clicking that object
      var newSteps = Steps.Any(x => x.Obj == newStep.Obj) ? EraseAllStepsBeforeAndIncludingObject(newStep.Obj) : Steps.Plus(newStep);

      if (newSteps.Count() == 0) { Context.Steps = newSteps.ToList(); return false; }

      var dict = Context.StepDictionary;

      // does newsteps fit any key?
      var clicks = newSteps.Select(x => x.Click).ToArray();

      bool done = false;

      //cl[] bestmatch=null;
      //cl[] bestkey=null;
      Dictionary<cl[], cl[]> matches = new Dictionary<cl[], cl[]>();

      foreach (var k in dict.Keys)
      {
        var match = FindLongestPostfixOfFirstThatIsPrefixOfSecond(clicks, k);
        if (match != null) matches.Add(k, match);
      }

      // foreach match check if valid: all steps have to fulfill precond!





      //  while (!done)
      //{
      //  var exactKey = FindExactKey(dict, clicks);
      //  if (exactKey == null)
      //  {
      //    var beginningOfKey =
      //    }
      //}






      //Debug.Assert(Context != null, "RefreshSteps: Context null");
      //Debug.Assert(Context.Steps != null, "RefreshSteps: Context.Steps NULL bei RefreshSequence!");
      //Debug.Assert(Context.StepDictionary != null, "RefreshSteps: Context.StepDictionary null!");

      //// if clicked a selected object, unselect and roll back to previous to clicking that object
      //var newSteps = Steps.Any(x => x.Obj == newStep.Obj) ? EraseAllStepsBeforeAndIncludingObject(newStep.Obj) : Steps.Plus(newStep);

      ////Console.WriteLine("\tSteps:" + Steps.Count+", newSteps:"+newSteps.Count());

      //if (newSteps.Count() == 0) { Context.Steps = newSteps.ToList(); return false; }

      var revsteps = newSteps.ToArray().Reverse().ToList();
      List<Step> result = new List<Step>();
      cl[] key = null;
      //var dict = Context.StepDictionary;
      bool complete = false; IsCardActivation = IsSpecialOptionActivation= false;

      foreach (var k in dict.Keys)
      {
        var rk = k.Reverse().ToArray();
        var len = rk.Length;
        int i = 0;
        while (i < len && rk[i] != revsteps[0].Click) i++;
        if (i >= len) continue; //not a match at all
        //while (i < len-1 && rk[i+1] == revsteps[0].Click) i++; //****** nimm last match of a sequence of same clicks

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
          if (currentStep.IsValid != null && !currentStep.IsValid(result)) { result.Clear(); continue; }
          currentStep.Process?.Invoke(result);

          if (i == 0) { complete = true; break; } //got complete action
        }
      }

      Context.Steps = result; // if no match found: get back empty step list in same context
      return complete;
    }

    //public bool RefreshSequence(Step newStep)
    //{
    //  Debug.Assert(Context != null, "RefreshSteps: Context null");
    //  Debug.Assert(Context.Steps != null, "RefreshSteps: Context.Steps NULL bei RefreshSequence!");
    //  Debug.Assert(Context.StepDictionary != null, "RefreshSteps: Context.StepDictionary null!");

    //  // if clicked a selected object, unselect and roll back to previous to clicking that object
    //  var newSteps = Steps.Any(x => x.Obj == newStep.Obj) ? EraseAllStepsBeforeAndIncludingObject(newStep.Obj) : Steps.Plus(newStep);

    //  if (newSteps.Count() == 0) { Context.Steps = newSteps.ToList(); return false; }

    //  var dict = Context.StepDictionary;

    //  // does newsteps fit any key?
    //  var clicks = newSteps.Select(x => x.Click).ToArray();

    //  bool done = false;
    //  while (!done)
    //  {
    //    var exactKey = FindExactKey(dict, clicks);
    //    if (exactKey == null)
    //    {
    //      var beginningOfKey = 
    //    }
    //  }

    //  foreach (var k in dict.Keys)
    //  {
    //    for(int i = 0; i < k.Length; i++)
    //    {
    //      var prefix = k.Take(i).ToArray();
    //      for(int j = 0; j < clicks.Length; j++)
    //      {
    //        var postfix = clicks.Skip(j).ToArray();

    //        if (SameContent(prefix, postfix))
    //        {
    //          // check if valid step

    //        }
    //      }
    //    }
    //  }




    //    var revsteps = newSteps.ToArray().Reverse().ToList();
    //  List<Step> result = new List<Step>();
    //  cl[] key = null;
    //  bool complete = false; IsCardActivation = false;

    //  foreach (var k in dict.Keys)
    //  {
    //    var rk = k.Reverse().ToArray();
    //    var len = rk.Length;
    //    int i = 0;
    //    while (i < len && rk[i] != revsteps[0].Click) i++;
    //    //while (i < len-1 && rk[i+1] == revsteps[0].Click) i++; // nimm last match
    //    if (i >= len) continue; //not a match at all

    //    var success = true;
    //    for (int j = i; j < len; j++) { if (j - i >= revsteps.Count || revsteps[j - i].Click != rk[j]) { success = false; break; } } // not a match!

    //    if (success)
    //    {
    //      result = revsteps.ToArray().Take(len - i).Reverse().ToList();
    //      key = k;

    //      for (int j = 0; j < result.Count; j++)
    //      {
    //        Debug.Assert(result[j].Click == key[j], "RefreshSteps: Clicks do not match in matching step sequence");
    //        result[j].IsValid = dict[key][j].IsValid;
    //        result[j].Process = dict[key][j].Process;
    //        result[j].UndoProcess = dict[key][j].UndoProcess;
    //        result[j].Message = dict[key][j].Message;
    //      }

    //      var currentStep = result.LastOrDefault();
    //      Debug.Assert(currentStep != null, "RefreshSteps: currentStep null!");
    //      if (currentStep.IsValid != null && !currentStep.IsValid(result)) { result.Clear(); continue; }
    //      currentStep.Process?.Invoke(result);

    //      if (i == 0) { complete = true; break; } //got complete action
    //    }
    //  }

    //  Context.Steps = result; // if no match found: get back empty step list in same context
    //  return complete;
    //}
    public bool RefreshSequence_orig(Step newStep)
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
      bool complete = false; IsCardActivation = IsSpecialOptionActivation= false;

      foreach (var k in dict.Keys)
      {
        var rk = k.Reverse().ToArray();
        var len = rk.Length;
        int i = 0;
        while (i < len && rk[i] != revsteps[0].Click) i++;
        if (i >= len) continue; //not a match at all
        //while (i < len-1 && rk[i+1] == revsteps[0].Click) i++; //****** nimm last match of a sequence of same clicks

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
          if (currentStep.IsValid != null && !currentStep.IsValid(result)) { result.Clear(); continue; }
          currentStep.Process?.Invoke(result);

          if (i == 0) { complete = true; break; } //got complete action
        }
      }

      Context.Steps = result; // if no match found: get back empty step list in same context
      return complete;
    }

    #endregion





  }
}
