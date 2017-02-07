using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ations
{
  public partial class Game
  {

    #region testing properties, code
    int testIndex, actionIndex;
    public string[] TestSequence { get; set; }
    public bool IsTesting { get; set; }
    public string TestKey { get; set; }
    public Dictionary<string, int> ResBefore0 { get; set; }
    public Dictionary<string, int> ResAfter0 { get; set; }
    public Dictionary<string, int> ResDiff0 { get; set; }
    public Dictionary<string, int> ResBefore1 { get; set; }
    public Dictionary<string, int> ResAfter1 { get; set; }
    public Dictionary<string, int> ResDiff1 { get; set; }


    public Action CustomizeProgress { get; set; }
    public Action TestPreGrowth { get; set; }
    public Action TestPostGrowth { get; set; }
    public Action CustomizeGrowth { get; set; }
    public Action TestPreNewEvent { get; set; }
    public Action TestPostNewEvent { get; set; }
    public Action CustomizeEvent { get; set; }
    public Action TestPreAction { get; set; }
    public Action TestPostAction { get; set; }
    public Action TestPreProduction { get; set; }
    public Action TestPostProduction { get; set; }
    public Action CustomizeProduction { get; set; }
    public Action TestPreWar { get; set; }
    public Action TestPostWar { get; set; }
    public Action TestPreEventResolution { get; set; }
    public Action TestPostEventResolution { get; set; }
    public Func<bool> TestVerify { get; set; }
    public List<Action> TestInput { get; set; }
    public Action TestCleanup { get; set; }

    public bool SwitchRoundAndAgeOn { get { return switchRoundStartOn; } set { if (switchRoundStartOn != value) { switchRoundStartOn = value; NotifyPropertyChanged(); } } }
    bool switchRoundStartOn;
    public bool SwitchProgressOn { get { return switchProgressOn; } set { if (switchProgressOn != value) { switchProgressOn = value; NotifyPropertyChanged(); } } }
    bool switchProgressOn;
    public bool SwitchGrowthOn { get { return switchGrowthOn; } set { if (switchGrowthOn != value) { switchGrowthOn = value; NotifyPropertyChanged(); } } }
    bool switchGrowthOn;
    public bool SwitchNewEventOn { get { return switchNewEventOn; } set { if (switchNewEventOn != value) { switchNewEventOn = value; NotifyPropertyChanged(); } } }
    bool switchNewEventOn;
    public bool SwitchActionOn { get { return switchActionOn; } set { if (switchActionOn != value) { switchActionOn = value; NotifyPropertyChanged(); } } }
    bool switchActionOn;
    public bool SwitchProductionOn { get { return switchProductionOn; } set { if (switchProductionOn != value) { switchProductionOn = value; NotifyPropertyChanged(); } } }
    bool switchProductionOn;
    public bool SwitchOrderOn { get { return switchOrderOn; } set { if (switchOrderOn != value) { switchOrderOn = value; NotifyPropertyChanged(); } } }
    bool switchOrderOn;
    public bool SwitchWarOn { get { return switchWarOn; } set { if (switchWarOn != value) { switchWarOn = value; NotifyPropertyChanged(); } } }
    bool switchWarOn;
    public bool SwitchEventOn { get { return switchEventOn; } set { if (switchEventOn != value) { switchEventOn = value; NotifyPropertyChanged(); } } }
    bool switchEventOn;
    public bool SwitchFamineOn { get { return switchFamineOn; } set { if (switchFamineOn != value) { switchFamineOn = value; NotifyPropertyChanged(); } } }
    bool switchFamineOn;

    void switchesOff() { SwitchFamineOn= SwitchNewEventOn = SwitchProgressOn = SwitchRoundAndAgeOn = SwitchGrowthOn = SwitchEventOn = SwitchActionOn = SwitchOrderOn = SwitchProductionOn = SwitchWarOn = false; }
    void switchesOn() { SwitchFamineOn= SwitchNewEventOn = SwitchProgressOn = SwitchRoundAndAgeOn = SwitchGrowthOn = SwitchEventOn = SwitchActionOn = SwitchOrderOn = SwitchProductionOn = SwitchWarOn = true; }
    void switchesToDefault() { SwitchNewEventOn = SwitchProgressOn = true; SwitchRoundAndAgeOn = SwitchGrowthOn = SwitchEventOn = SwitchActionOn = SwitchOrderOn = SwitchProductionOn = SwitchWarOn = SwitchFamineOn = false; }
    void testActionsOff()
    {
      CustomizeProgress = TestPreGrowth = TestPostGrowth = CustomizeGrowth = TestPreNewEvent = TestPostNewEvent = CustomizeEvent = TestPreAction = TestPostAction
        = TestPreProduction = TestPostProduction = CustomizeProduction = TestPreWar = TestPostWar
        = TestPreEventResolution = TestPostEventResolution = TestCleanup = null;
      TestVerify = null;
      if (TestInput != null) TestInput.Clear();
    }
    public string ButtonClick { get { return buttonClick; } set { if (buttonClick != value) { buttonClick = value; NotifyPropertyChanged(); } } }
    string buttonClick;

    public void PlayModeRequested() { if (IsTesting) { LongMessage = "switching to playmode after test completion"; ButtonClick = "PlayMode"; } }
    public void NextTestRequested() { LongMessage = "next round: starting next test..."; ButtonClick = "NextTest"; }
    public void RepeatRequested() { LongMessage = "next round: repeating test..."; ButtonClick = "Repeat"; }
    public void TestOrPlayMode()
    {
      if (ButtonClick == "PlayMode")
      {
        if (IsTesting)
        {
          switchesOn(); IsTesting = false; testActionsOff(); Description = "";
          TestKey = "Play Mode";
        }
      }
      else if (ButtonClick == "NextTest")// || string.IsNullOrEmpty(ButtonClick))
      {
        TestSetupNext(true);
      }
      else if (ButtonClick == "Repeat")
      {
        TestSetupNext(false);
      }
      else if (initialMode == "testing")
      {
        TestSetupNext(true);
      }
      else { TestKey = "Nations"; }
    }
    public void TestStartSequence()
    {
      Debug.Assert(TestDictionary.ContainsKey(startWithTest), "startWithTest NOT in TestDictionary!!! " + startWithTest);
      if (TestSequence == null) TestSequence = TestDictionary.Keys.ToArray();
      testIndex = TestDictionary.Keys.ToList().IndexOf(startWithTest) - 1;
      IsTesting = true;
    }
    public void TestSetupNext(bool incrementTestIndex = true)
    {
      if (!IsTesting) TestStartSequence();
      ResetGame();
      foreach (var pl in Players) pl.Reset(); // assume 2 players in all tests for now! no change of civ, just reinit players
      ClearAnimations(); // since player reset mods resources.

      if (incrementTestIndex) testIndex++; if (testIndex >= TestSequence.Length) testIndex = 0;
      actionIndex = 0;
      TestKey = TestSequence[testIndex];
      Debug.Assert(TestDictionary.ContainsKey(TestKey), "testkey " + TestKey + " missing!!!");
      switchesToDefault();
      testActionsOff();
      ShowScore = true;
      TestDictionary[TestKey](); //sets all the above functions but does not change anything in game
      Scoring();
    }
    public void ConsumeTestInput() // called in WaitForThreeButtonClick and WaitForButtonClick
    {
      //CurrentTest.PerformAction();
      if (TestInput != null && actionIndex < TestInput.Count)
      {
        TestInput[actionIndex]?.Invoke();
        actionIndex++;
      }
    }
    public bool TestCheckFailed()
    {
      foreach (var pl in Players) Checker.CalcStabAndMil(pl);
      var failed = !TestVerify?.Invoke();
      Message = failed == true ? "test failed!!!!!" : "test succeeded";
      Console.WriteLine(TestKey + " " + Message);
      return failed == true;
    }

    #endregion

    // different test setups
    public static void TestGrowth(string s, Player plMain, Action setup, List<Action> actions, Func<bool> verification, params object[] args)
    {
      G.SwitchGrowthOn = true;
      var plOther = plMain == P0 ? P1 : P0;
      G.TestPreGrowth = () => { G.MainPlayer = plMain; setup(); Checker.CalcStabAndMil(plMain); ResRecord(); };
      G.TestInput = actions;
      G.TestVerify = () => { ResEval(); G.MainPlayer = plMain; return verification(); };
      G.Description = s;
      Args = args;
    }
    public static void TestNewEvent(string s, Player plMain, Action setup, List<Action> actions, Func<bool> verification, params object[] args)
    {
      G.SwitchNewEventOn = true;
      var plOther = plMain == P0 ? P1 : P0;
      G.TestPreNewEvent = () => { G.MainPlayer = plMain; setup(); ResRecord(); };
      G.TestInput = actions;
      G.TestVerify = () => { ResEval(); G.MainPlayer = plMain; return verification(); };
      G.Description = s;
      Args = args;
    }
    public static void TestAction(string s, Player plMain, Action setup, List<Action> actions, Func<bool> verification, params object[] args)
    {
      G.SwitchActionOn = true;
      var plOther = plMain == P0 ? P1 : P0;
      G.TestPreAction = () => { G.MainPlayer = plMain; plOther.HasPassed = true; setup(); Checker.CalcStabAndMil(P0); Checker.CalcStabAndMil(P1); ResRecord(); };
      G.TestInput = actions;
      G.TestVerify = () => { ResEval(); G.MainPlayer = plMain; return verification(); };
      G.Description = s;
      Args = args;
    }
    public static void TestNewEventAndAction(string s, Player plMain, Action setup, List<Action> actions, Func<bool> verification, params object[] args)
    {
      G.SwitchActionOn = G.SwitchNewEventOn = true;
      var plOther = plMain == P0 ? P1 : P0;
      G.TestPreNewEvent = () => { G.MainPlayer = plMain; plOther.HasPassed = true; setup(); Checker.CalcStabAndMil(P0); Checker.CalcStabAndMil(P1); ResRecord(); };
      G.TestInput = actions;
      G.TestVerify = () => { ResEval(); G.MainPlayer = plMain; return verification(); };
      G.Description = s;
      Args = args;
    }
    public static void TestProduction(string s, Player plMain, Action setup, List<Action> actions, Func<bool> verification, params object[] args)
    {
      G.SwitchProductionOn = true;
      var plOther = plMain == P0 ? P1 : P0;
      G.TestPreProduction = () => { G.MainPlayer = plMain; setup(); Checker.CalcStabAndMil(plMain); ResRecord(); };
      G.TestInput = actions;
      G.TestVerify = () => { ResEval(); G.MainPlayer = plMain; return verification(); };
      G.Description = s;
      Args = args;
    }
    public static void TestOrder(string s, Player plMain, Action setup, List<Action> actions, Func<bool> verification, params object[] args)
    {
      G.SwitchOrderOn = true;
      G.CustomizeProduction = () => { setup(); Checker.CalcStabAndMil(P0); Checker.CalcStabAndMil(P1); ResRecord(); };
      G.TestInput = actions;
      G.TestVerify = () => { ResEval(); return verification(); };
      G.Description = s;
      Args = args;
    }
    public static void TestActionAndProduction(string s, Player plMain, Action setup, List<Action> actions, Func<bool> verification, params object[] args)
    {
      G.SwitchActionOn = G.SwitchProductionOn = true;
      var plOther = plMain == P0 ? P1 : P0;
      G.TestPreAction = () => { G.MainPlayer = plMain; plOther.HasPassed = true; setup(); Checker.CalcStabAndMil(plMain); ResRecord(); };
      G.TestInput = actions;
      G.TestVerify = () => { ResEval(); G.MainPlayer = plMain; return verification(); };
      G.Description = s;
      Args = args;
    }
    public static void TestWar(string s, Player plMain, Action setup, List<Action> actions, Func<bool> verification, params object[] args)
    {
      G.SwitchWarOn = true;
      var plOther = plMain == P0 ? P1 : P0;
      G.TestPreWar = () => { G.MainPlayer = plMain; setup(); ResRecord(); };
      G.TestInput = actions;
      G.TestVerify = () => { ResEval(); G.MainPlayer = plMain; return verification(); };
      G.Description = s;
      Args = args;
    }
    public static void TestEventResolution(string s, Player plMain, Action setup, List<Action> actions, Func<bool> verification, params object[] args)
    {
      G.SwitchEventOn = true;
      G.TestPreEventResolution = () => { P0.HasPassed = P1.HasPassed = true; G.Stats.EventCard = Card.MakeEventCard(G.TestKey.StringBefore(" ")); setup(); Checker.CalcStabAndMil(P0); Checker.CalcStabAndMil(P1); ResRecord(); };
      G.TestInput = actions;
      G.TestVerify = () => { ResEval(); return verification(); };
      G.Description = s;
      Args = args;
    }
    public static void TestEventAndWarResolution(string s, Player plMain, Action setup, List<Action> actions, Func<bool> verification, params object[] args)
    {
      G.SwitchEventOn = G.SwitchWarOn = true;
      G.TestPreWar = () => { G.Stats.EventCard = Card.MakeEventCard(G.TestKey); setup(); Checker.CalcStabAndMil(P0); Checker.CalcStabAndMil(P1); ResRecord(); };
      G.TestInput = actions;
      G.TestVerify = () => { ResEval(); return verification(); };
      G.Description = s;
      Args = args;
    }
    public static void TestFamine(string s, Player plMain, Action setup, List<Action> actions, Func<bool> verification, params object[] args)
    {
      G.SwitchFamineOn = true;
      G.CustomizeProduction = () => { if (args.Length > 0) G.Stats.EventCard = Card.MakeEventCard(args[0] as string); setup(); Checker.CalcStabAndMil(plMain); ResRecord(); };
      G.TestInput = actions;
      G.TestVerify = () => { ResEval(); return verification(); };
      G.Description = s;
      Args = args;
    }
    public static void TestFullGame(string s, List<Action> actions)
    {
      G.switchesOn();
    }
    public static void TestEndOfAge(string s, int[] arrbook, int[] arrvp)
    {
      G.InitPlayers(5);
      GivePlayersRes("book", arrbook);
      GivePlayersRes("vp", 0);
      //foreach (var pl in G.Players) { pl.Books = pl.Index; pl.Res.set("vp", 0); }
      G.Stats.Round = 1; // should be yes!
      G.TestVerify = () => { return PlayersHaveVP(arrvp); };
      G.TestCleanup = () => G.InitPlayers(2);
      G.Description = s;
    }
    public static bool PlayersHaveVP(int[] nums)
    {
      for (int i = 0; i < G.Players.Count; i++) if (G.Players[i].Res.n("vp") != nums[i]) return false;
      return true;
    }
    public static void GivePlayersRes(string resname, int[] nums)
    {
      for (int i = 0; i < G.Players.Count; i++)
      {
        G.Players[i].Res.set(resname, 0);
        G.Players[i].UpdateResBy(resname, nums[i]);
      }
    }
    public static void GivePlayersRes(string resname, int n)
    {
      for (int i = 0; i < G.Players.Count; i++)
      {
        G.Players[i].Res.set(resname, 0);
        G.Players[i].UpdateResBy(resname, n);
      }
    }

    // shortcuts
    public static object[] Args { get; set; }
    public static void ResRecord() { G.ResBefore0 = P0.GetResourceSnapshot(); G.ResBefore1 = P1.GetResourceSnapshot(); }
    public static void ResEval() { G.ResDiff0 = P0.GetResDiff(G.ResBefore0); G.ResDiff1 = P1.GetResDiff(G.ResBefore1); }
    public static int RD0(string res) { return G.ResDiff0.ContainsKey(res) ? G.ResDiff0[res] : 0; }
    public static int RD1(string res) { return G.ResDiff1.ContainsKey(res) ? G.ResDiff1[res] : 0; }
    public static void P0_WIC(string civcardname, int ndeploy = 0) { var card = Card.MakeCard(civcardname); Checker.AddCivCardSync(P0, card, P0.WICField); card.NumDeployed = ndeploy; }
    public static void P1_WIC(string civcardname, int ndeploy = 0) { var card = Card.MakeCard(civcardname); Checker.AddCivCardSync(P1, card, P1.WICField); card.NumDeployed = ndeploy; }
    public static void P0_ADV(string name, int ndeploy = 0) { var card = Card.MakeCard(name); Checker.AddCivCardSync(P0, card, P0.ADVField); card.NumDeployed = ndeploy; }
    public static void P1_ADV(string name, int ndeploy = 0) { var card = Card.MakeCard(name); Checker.AddCivCardSync(P1, card, P1.ADVField); card.NumDeployed = ndeploy; }
    public static void P0_DYN(string name, int ndeploy = 0) { var card = Card.MakeCard(name); Checker.AddCivCardSync(P0, card, P0.DYNField); card.NumDeployed = ndeploy; }
    public static void P1_DYN(string name, int ndeploy = 0) { var card = Card.MakeCard(name); Checker.AddCivCardSync(P1, card, P1.DYNField); card.NumDeployed = ndeploy; }
    public static void P0_BM0(string name, int ndeploy = 0) { var card = Card.MakeCard(name); Checker.AddCivCardSync(P0, card, P0.Civ.Fields[1]); card.NumDeployed = ndeploy; }
    public static void P0_BM1(string name, int ndeploy = 0) { var card = Card.MakeCard(name); Checker.AddCivCardSync(P0, card, P0.Civ.Fields[2]); card.NumDeployed = ndeploy; }
    public static void P0_BM2(string name, int ndeploy = 0) { var card = Card.MakeCard(name); Checker.AddCivCardSync(P0, card, P0.Civ.Fields[3]); card.NumDeployed = ndeploy; }
    public static void P0_BM3(string name, int ndeploy = 0) { var card = Card.MakeCard(name); Checker.AddCivCardSync(P0, card, P0.Civ.Fields[4]); card.NumDeployed = ndeploy; }
    public static void P1_BM0(string name, int ndeploy = 0) { var card = Card.MakeCard(name); Checker.AddCivCardSync(P1, card, P1.Civ.Fields[1]); card.NumDeployed = ndeploy; }
    public static void P1_BM1(string name, int ndeploy = 0) { var card = Card.MakeCard(name); Checker.AddCivCardSync(P1, card, P1.Civ.Fields[2]); card.NumDeployed = ndeploy; }
    public static void P1_BM2(string name, int ndeploy = 0) { var card = Card.MakeCard(name); Checker.AddCivCardSync(P1, card, P1.Civ.Fields[3]); card.NumDeployed = ndeploy; }
    public static void P1_BM3(string name, int ndeploy = 0) { var card = Card.MakeCard(name); Checker.AddCivCardSync(P1, card, P1.Civ.Fields[4]); card.NumDeployed = ndeploy; }
    public static void P0_Deploy(int fieldindex, int ndeploy) { P0.Civ.Fields[fieldindex].Card.NumDeployed = ndeploy; Checker.CalcStabAndMil(P0); }
    public static void P1_Deploy(int fieldindex, int ndeploy) { P1.Civ.Fields[fieldindex].Card.NumDeployed = ndeploy; Checker.CalcStabAndMil(P1); }
    public static void P0_WOND0(string name) { var card = Card.MakeCard(name); Checker.AddCivCardSync(P0, card, P0.Civ.Fields[9]); }
    public static void P0_WOND1(string name) { var card = Card.MakeCard(name); Checker.AddCivCardSync(P0, card, P0.Civ.Fields[10]); }
    public static void P0_WOND2(string name) { var card = Card.MakeCard(name); Checker.AddCivCardSync(P0, card, P0.Civ.Fields[11]); }
    public static void P0_COL0(string name) { var card = Card.MakeCard(name); Checker.AddCivCardSync(P0, card, P0.Civ.Fields[7]); }
    public static void P0_COL1(string name) { var card = Card.MakeCard(name); Checker.AddCivCardSync(P0, card, P0.Civ.Fields[8]); }
    public static void P0_GOLD(int n) { P0.Res.set("gold", n); }
    public static void ProgressCard(string name, int i) { var card = Card.MakeCard(name); G.Progress.Fields[i].Card = card; card.BasicCost = 3 - (i / G.Progress.Cols); }
    public static void Progress0(string name) { var card = Card.MakeCard(name); G.Progress.Fields[0].Card = card; card.BasicCost = 3; }
    public static void P0_WAR(string name, int warlevel) { P0_MIL(warlevel); G.Stats.UpdateWarPosition(P0, Card.MakeCard(name)); }
    public static void P1_WAR(string name= "first_crusade", int warlevel=6) { P1_MIL(warlevel); G.Stats.UpdateWarPosition(P1, Card.MakeCard(name)); }
    public static void P0_STAB(int stab) { P0_BM3("ziggurat", 1 + stab / 2); Checker.CalcStabAndMil(P0); } //stab soll odd und unter 10 sein!
    public static void P0_MIL(int mil) { P0_BM2("hoplite", mil / 3); Checker.CalcStabAndMil(P0); } //mil soll multiple von 3 sein!
    public static void P0_MIL(string name, int ndeploy) { P0_BM2(name, ndeploy); Checker.CalcStabAndMil(P0); } //mil soll multiple von 3 sein!
    public static void P1_STAB(int stab) { P1_BM3("ziggurat", 1 + stab / 2); Checker.CalcStabAndMil(P1); } //stab soll odd und unter 10 sein!
    public static void P1_MIL(int mil) { P1_BM2("hoplite", mil / 3); Checker.CalcStabAndMil(P1); } //mil soll multiple von 3 sein!
    public static Player MP { get { return G.MainPlayer; } }
    public static Player P0 { get { return G.Players.FirstOrDefault(x => x.Name.StartsWith("F")); } } //ACHTUNG!!!!!!!!
    public static Player P1 { get { return G.Players.FirstOrDefault(x => x.Name.StartsWith("A")); } } //ACHTUNG!!!!!!!!
    public static Game G { get { return Game.Inst; } }

    public static void ClickArchitect() { G.OnClickArchitect(); }
    public static void ClickTurmoil() { G.OnClickTurmoil(); }
    public static void ClickOk() { G.OkStartClicked = true; }
    public static void ClickNo() { G.PassClicked = true; }
    public static void ClickWorker() { G.OnClickCivResource(MP.Res.get("worker")); }
    public static void ClickDynasty() { G.OnClickCivField(MP.Civ.Fields[5]); }
    public static void ClickAdvisor() { var card = MP.Civ.Fields[0].Card; if (Checker.CheckActionPossible(MP, card)) G.OnClickCivField(MP.Civ.Fields[0]); else G.Message = "Advisor cannot be activated"; }
    public static void ClickWonder0() { G.OnClickCivField(MP.Civ.Fields[9]); }
    public static void ClickWonder1() { G.OnClickCivField(MP.Civ.Fields[10]); }
    public static void ClickWonder2() { G.OnClickCivField(MP.Civ.Fields[11]); }
    public static void ClickBM0() { G.OnClickCivField(MP.Civ.Fields[1]); }
    public static void ClickBM1() { G.OnClickCivField(MP.Civ.Fields[2]); }
    public static void ClickBM2() { G.OnClickCivField(MP.Civ.Fields[3]); }
    public static void ClickBM3() { G.OnClickCivField(MP.Civ.Fields[4]); }
    public static void ClickColony0() { G.OnClickCivField(MP.Civ.Fields[7]); }
    public static void ClickColony1() { G.OnClickCivField(MP.Civ.Fields[8]); }
    public static void ClickFieldForColony() { G.OnClickCivField(MP.Civ.Fields[7]); }
    public static void ClickProg0() { G.OnClickProgressField(G.Progress.Fields[0]); }
    public static void ClickProg10() { G.OnClickProgressField(G.Progress.Fields[10]); }
    public static void ClickProg15() { G.OnClickProgressField(G.Progress.Fields[15]); }
    public static void ClickProg(int i) { G.OnClickProgressField(G.Progress.Fields[i]); }
    public static void ClickPass() { Game.Inst.PassClicked = true; }
    public static void ClickSpecialOption0() { G.OnClickSpecialOption(G.MainPlayer.SpecialOptions[0]); }
    public static void ClickWorkerCounter0() { G.OnClickWorkerCounter(G.MainPlayer.Civ.Fields[1]); }
    public static void ClickWorkerCounter1() { G.OnClickWorkerCounter(G.MainPlayer.Civ.Fields[2]); }
    public static void ClickWorkerCounter2() { G.OnClickWorkerCounter(G.MainPlayer.Civ.Fields[3]); }
    public static void ClickWorkerCounter3() { G.OnClickWorkerCounter(G.MainPlayer.Civ.Fields[4]); }
    public static void PickResChoice() { Debug.Assert(Args.Length > 0, "PickResChoice: missing Args!!!"); var res = G.ResChoices.FirstOrDefault(x => x.Name == Args[0] as string); if (res != null) res.Num = (int)Args[1]; }
    public static void PickChoice0() { Debug.Assert(Game.Inst.Choices.Count > 0, "PickChoice0: missing Choice[0]!!!"); Game.Inst.SelectedChoice = Game.Inst.Choices[0]; }
    public static void PickChoice1() { Debug.Assert(Game.Inst.Choices.Count > 1, "PickChoice1: missing Choice[1]!!!"); Game.Inst.SelectedChoice = Game.Inst.Choices[1]; }
    public static void PickChoice2() { Debug.Assert(Game.Inst.Choices.Count > 2, "PickChoice1: missing Choice[2]!!!"); Game.Inst.SelectedChoice = Game.Inst.Choices[2]; }
    public static void PickVP() { var game = Game.Inst; game.SelectedResource = game.ResChoices.FirstOrDefault(x => x.Name == "vp"); }
    public static void PickWheat() { var game = Game.Inst; game.SelectedResource = game.ResChoices.FirstOrDefault(x => x.Name == "wheat"); }
    public static void PickWorker() { var game = Game.Inst; game.SelectedResource = game.ResChoices.FirstOrDefault(x => x.Name == "worker"); }
    public static void PickFirstResource() { var game = Game.Inst; game.SelectedResource = game.ResChoices.FirstOrDefault(); }
    public static void PickResource() { var game = Game.Inst; Debug.Assert(Args.Length > 0, "PickResource: missing Arg[0]!!!"); game.SelectedResource = game.ResChoices.FirstOrDefault(x => x.Name == Args[0] as string); }
    public static bool NO_ADVISOR(Player pl) { return pl.Civ.Fields[CardType.iADV].IsEmpty; }
  }
}
