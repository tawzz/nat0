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
    public void TestScenario1() //wonder ready testen, assume >=2 players
    {
      var pl = Players[0];
      var card = Card.MakeCard("colosseum");
      pl.AddCivCard(card, pl.WICField);
      card.NumDeployed = 1;

      Players[1].HasPassed = true;

      pl.UpdateStabAndMil();
    }

    public void TestScenario2() //event testen, assume 5 players
    {
      Players[0].Military = 5;
      Players[1].Military = 3;
      Players[2].Stability = 5;
      Players[3].Stability = 3;
      Players[4].Stability = 5;
    }

    public void TestScenario3() //event testen: default in wheat, assume 5 players
    {
      Players[0].Military = 5;
      Players[1].Military = 3;
      Players[2].Stability = 5;
      Players[3].Stability = 3;
      Players[4].Stability = 5;
      Stats.EventCard = Card.MakeEventCard("yellow_turban_rebellion"); 

    }

    public void TestScenario4() //event testen: pl default in vp, pl defaults in books, assume 5 players
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
      Stats.UpdateWarPosition(Players[1], Card.MakeCard("hyksos_invasion", 1));
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
    public void TestScenario7() //test HandleForeachEvent, assume 5 players
    {
      Players[0].CardsBoughtThisRound.Add(Card.MakeCard("milvian_bridge", 1));
      Players[0].CardsBoughtThisRound.Add(Card.MakeCard("milvian_bridge", 1));
      Players[0].CardsBoughtThisRound.Add(Card.MakeCard("parthian_wars", 1));
      Players[1].Military = 3;
      Stats.UpdateWarPosition(Players[1], Card.MakeCard("hyksos_invasion", 1));
      Players[2].Stability = 5;//verliert 1vp
      Players[3].Stability = 2;//verliert 1 vp und 2 coal
      Players[4].Stability = 0;//verliert 3vp, 1 coal, 3 auswahl
      Players[4].Res.set("coal", 1);
      Stats.EventCard = Card.MakeEventCard("rigveda");
    }
    public void TestScenario8() //test HandleForeachEvent, assume 5 players
    {
      Players[0].Military = 5;
      Players[0].Res.set("wheat", 0);
      Players[1].Military = 3;
      Players[2].Stability = 5;
      Players[3].Stability = 3;
      Players[4].Stability = 4;
      Stats.EventCard = Card.MakeEventCard("pax_romana");
    }
    public void TestScenario9() //test go_first effect, assume 5 players
    {
      Players[0].Military = 5;
      Players[0].Res.set("wheat", 0);
      Players[1].Military = 3;
      Players[2].Stability = 5;
      Players[3].Stability = 3;
      Players[4].Stability = 4;
      Stats.EventCard = Card.MakeEventCard("qin_unification");
    }
    public async void Tester() { Title = "Testing..."; LongMessage = Message = "TESTING CHOICE PICKER!!!"; await TestChoices(); }
    public async Task TestChoices()
    {
      IsOkStartEnabled = false; IsRunning = true;

      var choice = await WaitForPickChoiceCompleted(new string[] { "pick 3 coal", "go last", "steal a card from opponent" }, "event");

      Message = choice.Text + " = your pick";
      //Choices.Add(new Choice { Text = "pick 3 gold" });
      //Choices.Add(new Choice { Text = "go first" });
      //ShowChoices = true;
      //await Task.Delay(longDelay);
      //await WaitForButtonClick();
      //var choice = SelectedChoice;
      //if (choice != null) Message = "you picked " + SelectedChoice.Text; else Message = "NO Choice picked!";

    }

  }
}
