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
using ations;
namespace ations
{
  public partial class Game : DependencyObject, INotifyPropertyChanged
  {
    #region players

    // can init players (from scratch), reset all players, reset a player, change player number, remove specific player, add a player, change player civ (will reset to beginning of that civ),
    // change civcards incl numdeployed, change resources, change extraworkers, reset dynamic data
    public void ResetPlayer(Player pl) // just reset the player data to 0, prepare for loading a player from xml
    {

    }
    public void ChangePlayerNumber(int num)
    {

      var npl = Players.Count;
      if (num < npl)
      {
        // just remove a few players
      }
      else if (num > npl)
      {
        // add players, be careful not to add same player !
      }
      // reset player data for all players, not ?!?!?!?!?



    }


    #endregion

    #region initialize

    //public void SetupForStart() //called after initialization
    //{

    //  //Stats.WarCard = ACard.MakeCard("hyksos_invasion", 1); //testing design
    //  //Stats.EventCard = ACard.MakeEventCard("attila"); //testing design

    //  ////testing
    //  //Choices.Add(new Choice { Text = "Upgrade Dynasty erst einmal als ganz langer choices der sicher 3 zeilen braucht" });
    //  //Choices.Add(new Choice { Text = "Upgrade Dynasty" });
    //  //Choices.Add(new Choice { Text = "Take 2 Gold" });
    //  //ShowChoices = true;
    //}


    #endregion
    //public ObservableCollection<Res> SelectedResources { get { return (ObservableCollection<Res>)GetValue(SelectedResourcesProperty); } set { SetValue(SelectedResourcesProperty, value); } }
    //public static readonly DependencyProperty SelectedResourcesProperty = DependencyProperty.Register("SelectedResources", typeof(ObservableCollection<Res>), typeof(Game), null);
    //public Dictionary<string, int> NumEach { get; set; }

    //public void AddSimpleCardResEffects(Card card)
    //{ //simple = only affect self,no preconditions,no special effects: first line of card.X
    //  var reslist = card.GetDynamicResources();
    //  foreach (var res in reslist) MainPlayer.CardEffects.Add(new Tuple<Card, Res>(card, res));
    //}
    //public void RemoveCardEffects(Card card)
    //{
    //  foreach (var pl in Players)
    //  {
    //    var ceffRemove = pl.CardEffects.Where(x => x.Item1.Name == card.Name).ToList();
    //    foreach (var ceff in ceffRemove) pl.CardEffects.Remove(ceff);
    //  }
    //}

    #region serialization
    // when saving game, it is saved exactly as it was at the beginning of the round
    // undo for now loads the game state at the beginning of the current round
    // equally, could save game or undo game to the beginning of the current players turn
    // then need also pass order, round effects, war card, CardsBoughtThisRound
    public XElement StateRoundStart { get; set; }
    public XElement StateTurnBegin { get; set; }
    public bool IsLoading { get; set; }
    public XElement LoadedGame { get; set; }
    public void SaveGame()
    {
      if (StateRoundStart == null) StateRoundStart = this.ToXml();
      StateRoundStart.Save("savedGame.xml");
      //File.WriteAllLines("report.txt", Checker.Report.ToArray());

    }
    public async Task LoadGame()
    {
      await InterruptGame();

      var xgame = XElement.Load("savedGame.xml");
      NumPlayers = xgame.aint("numplayers");
      Progress.Cols = xgame.aint("cols");
      rounds = xgame.aint("rounds");
      Progress.IncludeDynasty = xgame.abool("incldyn");
      Progress.IncludeFake = xgame.abool("inclfake");
      IsTesting = false;
      ResetGame();
      //load players as in xgame
      switchesOn();
      SwitchRoundAndAgeOn = false;
      IsLoading = true;
      LoadedGame = xgame;
      GameLoop();
    }
    public void LoadGameII()
    {
      var xgame = LoadedGame;
      Stats.Round = xgame.aint("round");
      Stats.Age = xgame.aint("age");

      Progress.LoadXml(xgame.Element("progress"));// load progress cards as in xgame.Element("progress")

      //load players, in dem order in dem die players sind ist der richtige player order!
      int i = 0;
      foreach (var xpl in xgame.Elements("player")) Players[i++].LoadXml(xpl);
      if (Players[0].Civ.Fields[10].Card.Name != "colosseum") { Message = "WIE BITTE?"; }
      IsLoading = false;
      LoadedGame = null;
      SwitchRoundAndAgeOn = true;
    }


    public XElement ToXml()
    {
      var result = new XElement("game",
        new XAttribute("round", Stats.Round),
        new XAttribute("cols", Progress.Cols),
        new XAttribute("age", Stats.Age),
        new XAttribute("rounds", rounds),
        new XAttribute("incldyn", Progress.IncludeDynasty),
        new XAttribute("inclfake", Progress.IncludeFake),
        new XAttribute("numplayers", NumPlayers));

      result.Add(Progress.ToXml()); // nur die card names der reihe nach oder "" wenn field empty

      foreach (var pl in Players) result.Add(pl.ToXml());

      return result;
    }

    #endregion


    void Reset_old(int nPlayers = numPlayersSetting, int nProgressCols = progressColsSetting, int nRounds = roundSetting, bool inclDyn = inclExpDynSetting, bool inclFake = inclExpFakeSetting)
    {
      // clear all properties
      // do not touch UIRoundMarker!      //SelectedChoice = null;      //SelectedResource = null;
      if (nPlayers > NumPlayers)
      {
        // create additional players

      }
      if (Players.Count != NumPlayers)
      {

      }

      ResChoices.Clear();
      Choices.Clear();
      ChangesInResources.Clear();
      PassOrder = new List<Player>();
      Message = LongMessage = "";
      Caption = "Start"; LeftCaption = "Cancel"; RightCaption = "Pass"; Title = "";
      Interrupt = IsRunning = CanInitialize = false;
      Kickoff = null;
      AnimationQueue.Clear();
      ShowChoices = ShowResChoices = ShowChangesInResources = ShowMultiResChoices = false;
      Number = 0;
      //if (NumEach != null) NumEach.Clear();
      if (ResourcesUpdatedThisAnimationCycle != null) ResourcesUpdatedThisAnimationCycle.Clear();
      iplayer = iround = iphase = iturn = iaction = 0;
      rounds = nRounds;

      InitPlayers(nPlayers);
      Progress = new Progress(nProgressCols, inclDyn, inclFake);// NumPlayers + 2); //testing
      Stats = new Stats(this);

      ArchitectEnabled = TurmoilEnabled = WorkerEnabled = IsOkStartEnabled = IsPassEnabled = IsCancelEnabled = false;
      OkStartClicked = CancelClicked = PassClicked = ArchitectSelected = TurmoilSelected = WorkerSelected = IsCardActivation = IsSpecialOptionActivation= false;
      ActionComplete = false;
      Context = null;
      ContextStack.Clear();

      //SetupForStart();
    }

    public async Task General_Check(Player pl, string checkName, params object[] oarr)
    {
      var plMain = MainPlayer;
      var plOthers = Players.Where(x => x != pl).ToList();

      // first_turn (buddha)
      foreach (var card in plMain.Cards)
      {
        var eff = card.X.Element(checkName);
        if (eff != null)
        {
          var plAffected = await TestPrecondition(eff, pl, card, oarr);
          //          if (condTrue) await PerformEffects(eff, pl, card, oarr);
        }
      }
      // for each plMain.Cards
      // does this card affect MY first turn?
      // 


      // pre_buy (I have hannibal, about to buy a battle)
      // pre_buy (other player has hannibal, about to buy a battle)
      // wonder_ready, I have sphinx
      // production check (augustus: get 2 coal if most military)
      // production Hepatya: +1cross, 1 book foreach cross
      // americadyn1: natural wonder ready +2wheat
      // event reolution (hellenism: undeploy military for 2 books each

    }
    async Task<List<Player>> TestPrecondition(XElement xel, Player pl, Card card, object[] oarr)
    {
      await Task.Delay(100);
      return null;
      // get pred attribute and param attribute, apply pred to param, return outcome
      //var pred = xel.astring("pred");
      //if (pred == "and")
      //{
      //  var predlist = ev.Elements("p").ToList();
      //  var result = basePlayerSet;
      //  foreach (var predev in predlist) result = CalcPlayersAffectedByPredOnObjects(predev, result, oarr);

      //  return CalcPredAnderOnObjects(ev, basePlayerSet, oarr);
      //}
      //else if (pred == "or") return CalcPredOrerOnObjects(ev, basePlayerSet, oarr);

    }


  }
}
