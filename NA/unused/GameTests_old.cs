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

    #region switches and tests
    public string CurrentKey { get; set; }
    public Test CurrentTest { get; set; }

    public void Reinitialize(bool forTest = false)
    {
      CanInitialize = false;
      if (forTest && CurrentTest != null) { var t = CurrentTest; Initialize(t.nplayers, t.ncols, t.nrounds, t.isdyn, t.isfake); }
      else Initialize(NumPlayers, Progress.Cols, rounds, Progress.IncludeDynasty, Progress.IncludeFake);

      NotifyPropertyChanged("ResChoices");
      NotifyPropertyChanged("ChangesInResources");
      NotifyPropertyChanged("Choices");
      NotifyPropertyChanged("AnimationQueue");
      NotifyPropertyChanged("ContextStack");
      NotifyPropertyChanged("NumPlayers");
      NotifyPropertyChanged("Progress");
      NotifyPropertyChanged("Stats");
    }

    public async Task PlayMode()
    {
      await InterruptGame();
      IsTesting = false;
      CanInitialize = false;
      Reinitialize();
      switchesOn();
      SwitchGrowthOn = false;
    }
    public async Task NextTest()
    {
      await InterruptGame();
      if (TestSequence == null) TestSequence = dTest.Keys.ToArray();

      testIndex++; if (testIndex >= TestSequence.Length) testIndex = 0;
      CurrentKey = TestSequence[testIndex];
      Debug.Assert(dTest.ContainsKey(CurrentKey), "dtest does not contains key " + CurrentKey);
      CurrentTest = dTest[CurrentKey];
      IsTesting = true;
      Reinitialize(true);
      CurrentTest.Setup();
    }
    public void TestEnd()
    {
      Message = CurrentTest.Verify() ? "test succeeded" : "test failed!!!";
      IsTesting = false;
      SwitchRoundAndAgeOn = true;

    }

    #endregion

    Dictionary<string, Test> dTest = new Dictionary<string, Test>()
    {
      {"wonder ready", new Test("wonder ready", TestScenario1,
        new List<Action> { ClickArchitect, ClickOk, ClickFieldForWonder, ClickOk, ClickPass },
        TestVerification1,"p1 has colosseum, p2 passed",2,7,2,true,false,false,false,true,false,false,false,false) },
      {"upgrade dynasty", new Test("upgrade dynasty", TestScenario2,
        new List<Action> { ClickTurmoil, ClickOk, ClickChoice1, ClickOk, ClickPass },
        TestVerification2,"p1 takes a turmoil and upgrades dynasty, p2 passed",2,7,2,true,false,false,false,true,false,false,false,false) },
    };
    public static void TestScenario0() { }
    public static bool TestVerification0() { return true; }
    public static void TestScenario1() //wonder ready testen, assume >=2 players
    {
      var game = Game.Inst;

      var pl = game.Players[0]; var card = Card.MakeCard("colosseum"); pl.AddCivCard(card, pl.WICField); card.NumDeployed = 1;
      pl.CalcStabAndMil();

      game.Players[1].HasPassed = true;
    }
    public static bool TestVerification1() { return RD["military"] == 3 && RD["wheat"] == -2 && RD["coal"] == -2; }
    public static void TestScenario2() { G.Stats.Turmoils = 2; G.Players[1].HasPassed = true; }
    public static bool TestVerification2() { return MP.Civ.Dynasties.Count == 1 && RD["stability"] == -2; }



    //public static void ClickProgressCard() { Game.Inst.OnClickProgressField(.OnClickArchitect(); }
    //public static void ClickArchitect() { Game.Inst.OnClickArchitect(); }
    //public static void ClickArchitect() { Game.Inst.OnClickArchitect(); }
    //public static void ClickArchitect() { Game.Inst.OnClickArchitect(); }
    //public static void ClickArchitect() { Game.Inst.OnClickArchitect(); }
    //public static void ClickArchitect() { Game.Inst.OnClickArchitect(); }
    //public static void ClickArchitect() { Game.Inst.OnClickArchitect(); }
    //public static void ClickArchitect() { Game.Inst.OnClickArchitect(); }
    //public static void ClickArchitect() { Game.Inst.OnClickArchitect(); }






















    #region old code
    Action test_scenario = null;
    void setSwitches(int i) { switchesOff(); if (i < 10) SwitchActionOn = true; }

    public async Task NextTest_old()
    {
      if (IsRunning)
      {
        Interrupt = true;
        Message = "interrupting";
        while (!CanInitialize)
        {
          await Task.Delay(300);
          if (Message.Length > 60) Message = "Interrupting."; else Message += ".";
        }
        Message = "Interrupt Completed! proceeding to next test";
      }

      switchesOff();
      string[] testSequence = testPlan.Keys.ToArray(); // string[] testSequence = { "archimedes", "augustus", "boudica" };
      if (testIndex >= testSequence.Length) testIndex = 0;
      CurrentKey = testSequence[testIndex];
      test_scenario = testPlan[CurrentKey][0];
      testPlan[CurrentKey][1]();
      //setSwitches(testIndex);

      testIndex++;

      CanInitialize = false;
      Initialize(5, 7, 4, true, false);

      NotifyPropertyChanged("ResChoices");
      NotifyPropertyChanged("ChangesInResources");
      NotifyPropertyChanged("Choices");
      NotifyPropertyChanged("AnimationQueue");
      NotifyPropertyChanged("ContextStack");
      NotifyPropertyChanged("NumPlayers");
      //      NotifyPropertyChanged("Players");
      //      NotifyPropertyChanged("MainPlayer");
      NotifyPropertyChanged("Progress");
      NotifyPropertyChanged("Stats");
    }
    #endregion



    Dictionary<string, Action[]> testPlan = new Dictionary<string, Action[]>()
    {//new Test(testname,2,2,true,false,true,true,true,... init params, switch params,
      {"archimedes", new Action[] { ()=>Game.Inst.TestScenario13(), ()=> Game.Inst.SwitchActionOn = true } },
      {"brewery",new Action[] { ()=>Game.Inst.TestScenario6(), ()=> Game.Inst.SwitchProductionOn = true } },
      {"rigveda",new Action[] { ()=>Game.Inst.TestScenario7(), ()=> Game.Inst.SwitchEventOn = true } },
      {"pax_romana",new Action[] { ()=>Game.Inst.TestScenario8(), ()=> Game.Inst.SwitchEventOn = true } },
      {"qin_unification",new Action[] { ()=>Game.Inst.TestScenario9(), ()=> Game.Inst.SwitchEventOn = true } },
      {"sea_peoples",new Action[] { ()=>Game.Inst.TestScenario10(), ()=> Game.Inst.SwitchEventOn = true } },
      {"hellenism",new Action[] { ()=>Game.Inst.TestScenario11(), ()=> Game.Inst.SwitchEventOn = true } },
      {"assyrian_deportations",new Action[] { ()=>Game.Inst.TestScenario12(), ()=> Game.Inst.SwitchEventOn = true } },
      {"taoism",new Action[] { ()=>Game.Inst.TestScenario14(), ()=> Game.Inst.SwitchEventOn = true } },
      {"americadyn0",new Action[] { ()=>Game.Inst.TestScenario15(), ()=> Game.Inst.SwitchEventOn = true } },
    };

    public void TestScenario22() //event testen, assume 5 players
    {
      Players[0].Military = 5;
      Players[1].Military = 3;
      Players[2].Stability = 5;
      Players[3].Stability = 3;
      Players[4].Stability = 5;
    }
    public void TestScenario3() //yellow_turban_rebellion event testen: default in wheat, assume 5 players
    {
      Players[0].Military = 5;
      Players[1].Military = 3;
      Players[2].Stability = 5;
      Players[3].Stability = 3;
      Players[4].Stability = 5;
      Stats.EventCard = Card.MakeEventCard("yellow_turban_rebellion");

    }
    public void TestScenario4() //yellow_turban_rebellion event testen: pl default in vp, pl defaults in books, assume 5 players
    {
      Players[0].Military = 5;
      Players[1].Military = 3;
      Players[2].Stability = 5;
      Players[2].Res.set("vp", 0); // defaults in vp but can pay with books: should have under 10 books at the end
      Players[2].Books = 20;
      Players[3].Stability = 3;
      Players[3].Res.set("vp", 1); // defaults in book: should get termination message
      Players[4].Stability = 5;
      Stats.EventCard = Card.MakeEventCard("yellow_turban_rebellion");

    }
    public void TestScenario5() //war testen, assume 5 players
    {
      Players[0].Military = 5;
      Players[1].Military = 3;
      Stats.UpdateWarPosition(Players[1], Card.MakeCard("hyksos_invasion"));
      Players[2].Stability = 5;//verliert 1vp
      Players[3].Stability = 2;//verliert 1 vp und 2 coal
      Players[4].Stability = 0;//verliert 3vp, 1 coal, 3 auswahl
      Players[4].Res.set("coal", 1);
    }
    public void TestScenario6() //brewery und pyramids, da war ein bug mit production!, assume 5 players
    {
      Players[0].Military = 5;
      Players[1].Military = 3;
      var card = Card.MakeCard("brewery");
      Players[1].AddCivCard(card, Players[1].Civ.Fields[2]);
      card.NumDeployed = 1;

      Players[2].HasPassed = Players[3].HasPassed = Players[4].HasPassed = true;

    }
    public void TestScenario7() //rigveda test HandleForeachEvent, assume 5 players
    {
      Players[0].CardsBoughtThisRound.Add(Card.MakeCard("milvian_bridge"));
      Players[0].CardsBoughtThisRound.Add(Card.MakeCard("milvian_bridge"));
      Players[0].CardsBoughtThisRound.Add(Card.MakeCard("parthian_wars"));
      Players[1].Military = 3;
      Stats.UpdateWarPosition(Players[1], Card.MakeCard("hyksos_invasion"));
      Players[2].Stability = 5;//verliert 1vp
      Players[3].Stability = 2;//verliert 1 vp und 2 coal
      Players[4].Stability = 0;//verliert 3vp, 1 coal, 3 auswahl
      Players[4].Res.set("coal", 1);
      Stats.EventCard = Card.MakeEventCard("rigveda");
    }
    public void TestScenario8() //pax_romana test HandleForeachEvent, assume 5 players
    {
      Players[0].Military = 5;
      Players[0].Res.set("wheat", 0);
      Players[1].Military = 3;
      Players[2].Stability = 5;
      Players[3].Stability = 3;
      Players[4].Stability = 4;
      Stats.EventCard = Card.MakeEventCard("pax_romana");
    }
    public void TestScenario9() //qin_unification test go_first effect, assume 5 players
    {
      Players[0].Military = 5;
      Players[0].Res.set("wheat", 0);
      Players[1].Military = 3;
      Players[2].Stability = 5;
      Players[3].Stability = 3;
      Players[4].Stability = 4;
      Stats.EventCard = Card.MakeEventCard("qin_unification");
    }
    public void TestScenario10() //test or Choice event: sea_peoples[1], need least military and not_most of both, assume 5 players
    {
      Players[0].Military = 5;
      Players[0].Res.set("wheat", 0);
      Players[1].Military = Players[3].Military = 3;
      var pl = Players[2];
      Players[2].Stability = 5;
      var card = Card.MakeCard("colosseum");
      Players[2].AddCivCard(card, Players[2].WICField);
      card.NumDeployed = 1;

      Players[3].Stability = 3;
      Players[4].Stability = 4;
      Stats.EventCard = Card.MakeEventCard("sea_peoples");
    }
    public void TestScenario11() //test or Choice event: hellenism[1]
    {
      Players[0].Military = 4; //least military
      Players[0].Stability = 4;
      var pl = Players[0]; pl.Civ.Fields[3].Card.NumDeployed = 5;
      pl.Res.set("worker", 0);

      Players[1].Military = 5;
      Players[1].Stability = 5; // most stability

      Players[2].Military = 5;
      Players[2].Stability = -1; //least stability, arabia axeman idx=3
      pl = Players[2]; pl.Civ.Fields[3].Card.NumDeployed = 5;
      pl.Res.set("worker", 0);

      Players[3].Military = 5;
      Players[3].Stability = 3;

      Players[4].Military = 6; // most military
      Players[4].Stability = 4;
      Stats.EventCard = Card.MakeEventCard("hellenism");
    }
    public void TestScenario12() //test PickWorkerToUndeployFrom: assyrian_deportations
    {
      Players[0].Military = 4; //least military
      Players[0].Stability = 4;

      Players[1].Military = 5;
      Players[1].Stability = 5; // most stability

      Players[2].Military = 5;
      Players[2].Stability = -1; //least stability, arabia axeman idx=3
      var pl = Players[2]; pl.Civ.Fields[3].Card.NumDeployed = 5;
      pl.Res.set("worker", 0);

      Players[3].Military = 5;
      Players[3].Stability = 3;

      Players[4].Military = 6; // most military
      Players[4].Stability = 4;
      Stats.EventCard = Card.MakeEventCard("assyrian_deportations");
    }
    public void TestScenario13() //archimedes, testing card activation cycle, 2+ players
    {
      var pl = Players[0];
      var card = Card.MakeCard("archimedes");
      pl.AddCivCard(card, pl.Civ.Fields[0]);
      card = Card.MakeCard("colosseum");
      pl.AddCivCard(card, pl.WICField);
      card.NumDeployed = 1;

      foreach (var pl1 in Players.Where(x => x != Players[0])) pl1.HasPassed = true;

    }
    public void TestScenario14() //taoism, pass_first test, assume 5 players
    {
      Players[0].Military = 5;
      Players[0].Res.set("wheat", 0);
      Players[1].Military = 3;
      Players[2].Stability = 5;
      Players[3].Stability = 3;
      Players[4].Stability = 4;
      Stats.EventCard = Card.MakeEventCard("taoism");
      PassOrder = new List<Player> { Players[4], Players[2] };
    }
    public void TestScenario15() //americadyn0: natural wonder ready >+2 wheat, need a natural wonder!
    {
      Players[0].Military = 5;
      Players[0].Res.set("wheat", 0);
      Players[1].Military = 3;
      Players[2].Stability = 5;
      Players[3].Stability = 3;
      Players[4].Stability = 4;
      Stats.EventCard = Card.MakeEventCard("taoism");
      PassOrder = new List<Player> { Players[4], Players[2] };
    }

    //public async void Tester() { Title = "Testing..."; LongMessage = Message = "TESTING CHOICE PICKER!!!"; await TestChoices(); }
    //public async Task TestChoices()
    //{
    //  IsOkStartEnabled = false; IsRunning = true;

    //  var choice = await WaitForPickChoiceCompleted(new string[] { "pick 3 coal", "go last", "steal a card from opponent" }, "event");

    //  Message = choice.Text + " = your pick";
    //  //Choices.Add(new Choice { Text = "pick 3 gold" });
    //  //Choices.Add(new Choice { Text = "go first" });
    //  //ShowChoices = true;
    //  //await Task.Delay(longDelay);
    //  //await WaitForButtonClick();
    //  //var choice = SelectedChoice;
    //  //if (choice != null) Message = "you picked " + SelectedChoice.Text; else Message = "NO Choice picked!";

    //}

  }
}
