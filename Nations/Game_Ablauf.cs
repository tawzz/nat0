using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Reflection;
using System.Collections.ObjectModel;

namespace ations
{
  public partial class Game
  {
    /*
      ablauf pattern:
      1. SetupFor...Phase: 
        arbeit:
          - if CurrentPhase non-empty, ordnet diese in history ein (fuer display, undo...)
          - setzt alle spieler auf phase not completed (normalerweise). 
          - creates gamephase mit plan fuer next phase
        prompt:
          - setzt eine message: was der player tun soll
          - setzt start kickoff
        animation, anschiessend freigabe der ui:
          - started ev. empty storyboard, an dessen ende UI activiert wird: dann erst kann player kickoff button druecken
      2. Folge von KickoffAction/Reaction nach above model, bis alle spieler durch,
          - spezielle KickoffAction is 
            - NextPlayer: da wird gecheckt player gewechselt, phase restarted oder auf next phase gestellt (CurrentPhase.SetupForNextPhase invoke)
            - phases bei denen keine take turns ist, rufen nicht next player auf, sondern nach der arbeit wird SetupForNextPhase called

      ...
      */
    public ObservableCollection<GamePhase> GamePhases { get; set; } //to keep a history to display somewhere
    public GamePhase Phase { get { return currentPhase; } set { currentPhase = value; NotifyPropertyChanged(); } }
    GamePhase currentPhase;

    public void Setup() // called in Initialize, on Game creation
    {
      ResChoices.Clear();
      SelectedResource = null;
      RandomPlayerOrder();
      GamePhases.Clear();

      TestSetup2();
      //SetupForRoundMarkerPhase();
    }
    public void Kickoff() { if (Phase != null && Phase.NextKickoffAction != null) Phase.NextKickoffAction(); }
    public void NewPhase() { if (Phase != null) { Phase.IsCompleted = true; GamePhases.Add(Phase); } foreach (var pl in Players) pl.PhaseCompleted = false; }

    public void SetupForRoundMarkerPhase() { NewPhase(); Phase = new GamePhase(Phases.roundmarker, "Start", "Ready for Battle?", OnRoundStart, null, AniRoundMarker, RoundMarkerCompleted, SetupForProgressCardsPhase); }
    public void OnRoundStart()
    {
      Round++;
      AniRoundMarker(RoundMarkerUI);
      foreach (var pl in Players) pl.Defaulted.Clear(); //TODO mach ordentliches player.NewPhase und player.NewRound
    }
    public void RoundMarkerCompleted(object sb, object target)
    {
      SetupForProgressCardsPhase();
      DealProgressCards();
      SetupForGrowthPhase();
      OnGrowth();
    } //direkt anschliessen, as 1 phase

    // skip: roundmarker and deal progresscards as one phase
    public void SetupForProgressCardsPhase() { NewPhase(); Phase = new GamePhase(Phases.progresscards, "Start", "Let's Fight!!!", KOProgressCards, null, AniProgressCards, ProgressCardsCompleted, SetupForGrowthPhase); }
    public void KOProgressCards() { DealProgressCards(); EnableUI=true; Phase.SetupNextGamePhaseAction?.Invoke(); }
    public void ProgressCardsCompleted(object sb, object target) { } //not used

    public void SetupForGrowthPhase() { NewPhase(); Phase = new GamePhase(Phases.growth, "Growth", "Pick a Resource", OnGrowth, OnGrowth, AniResourceUpdate, GrowthCompletedForPlayer, NewEvent); }
    public void OnGrowth()
    {
      var reslist = new List<string> { "wheat", "coal", "gold" };
      if (MainPlayer.ExtraWorkers.Count(x => !x.IsCheckedOut) > 0) reslist.Add("worker");
      var num = LevelGrowth[MainPlayer.Level]; //als param zu resourcepicker?
      PrepareResourcePicker(reslist, num, "growth");
    }
    public void PrepareResourcePicker(IEnumerable<string> resnames, int num, string forWhat)
    {
      //make sure, ResChoices is cleared!
      foreach (var rname in resnames) ResChoices.Add(new Res(rname));
      Message = MainPlayer.Name + ", pick a resource for " + forWhat;
      Phase.MessageTodo = "pick " + forWhat + " resource";
      Number = num;
      WorkerNumber = 1; //ev. weg damit
      ShowChoicePicker = true;
      Phase.ButtonText = "Pick";
      Phase.NextKickoffAction = OnPick;
      EnableUI=true;
    }
    public void OnPick()
    {
      var res = SelectedResource;
      if (res == null) { Phase.MessageTodo = "YOU IDIOT YOU DID NOT PICK ANYTHING!!!!!!!!!!!!!!!!!!!!!!!!!!!!"; EnableUI=true; return; }

      //did pick something:
      ShowChoicePicker = false;
      ResChoices.Clear();
      SelectedResource = null;
      Phase.MessageTodo = Message = MainPlayer.Name + ", you picked " + res.Name.ToCapital();

      if (res.Name == "worker")
      {
        var workers = MainPlayer.ExtraWorkers;
        var wfree1 = workers.FirstOrDefault(x => !x.IsCheckedOut);
        var wfree2 = workers.LastOrDefault(x => !x.IsCheckedOut);
        if (wfree1.CostRes != wfree2.CostRes)
        {
          ResChoices.Clear();
          var resarr = MainPlayer.ExtraWorkers.Where(x => !x.IsCheckedOut).Select(x => x.CostRes).Distinct().ToArray();
          foreach (var rname in resarr) ResChoices.Add(new Res(rname));
          Phase.MessageTodo = Message = "pick worker type";
          ShowChoicePicker = true;
          Phase.ButtonText = "Pick Worker";
          Phase.NextKickoffAction = OnPickWorker;
          EnableUI=true;
          return;
        }
        else { MainPlayer.CheckOutWorker(wfree1); } //TODO brauch ich WorkerNumber ueberhaupt? nein, aber lasse for now!
      }
      else { MainPlayer.resupdate(res.Name, Number); }
    }
    public void OnPickWorker()
    {
      var res = SelectedResource;
      if (res == null) { Phase.MessageTodo = Message = "YOU IDIOT YOU DID NOT PICK ANYTHING!!!!!!!!!!!!!!!!!!!!!!!!!!!!"; return; }

      //did pick: 
      ShowChoicePicker = false;
      ResChoices.Clear();
      SelectedResource = null;
      var worker = MainPlayer.ExtraWorkers.FirstOrDefault(x => !x.IsCheckedOut && x.CostRes == res.Name);
      MainPlayer.CheckOutWorker(worker);
      Phase.MessageTodo = Message = MainPlayer.Name + ", this worker will cost you 3 " + res.Name;//TODO immer 3?
    }
    public void GrowthCompletedForPlayer(object sb, object target) { OnNext(); }//TODO: brauch ich das? lassen for now
    public void OnNext()
    {
      MainPlayer.PhaseCompleted = true;
      var next = Players.FirstOrDefault(x => x.Index > MainPlayer.Index && !x.PhaseCompleted);
      if (next != null) //phase not yet completed!
      {
        MainPlayer = next;
        Phase.MessageTodo = Message = "Hi, " + MainPlayer.Name + "!!!!!";
        Phase.PlayerChangeKickoffAction?.Invoke(); //TODO: direct zu resource picker nicht so gut, ani playerwechsel
      }
      else
      {
        GamePhases.Add(Phase);
        Message = "";
        MainPlayer = Players[0];
        EnableUI=true;
        Phase.SetupNextGamePhaseAction?.Invoke();
      }
    }

    public void NewEvent()
    {
      PickEventCard(); // also does architects and turmoils
      SetupForActionPhase();
      PlayerAction();
    }

    public void SetupForActionPhase() { NewPhase(); Phase = new GamePhase(Phases.action, "Action", "Your Turn, " + MainPlayer.Name, PlayerAction, PlayerAction, AniPlayerAction, ActionCompletedForPlayer, SetupForProductionPhase); }
    public void PlayerAction() { CanBuyProgressCard = true; MarkAllSelectableObjects(); }
    public void MarkAllSelectableObjects()
    {
      foreach (var card in ProgressCards) { card.CanBuy = CalcCanBuy(card); }
      CanTakeArchitect = CalcCanTakeArchitect();
      MainPlayer.CanDeploy = CalcCanDeploy();
      // now things are marked
    }
    bool CalcCanBuy(Card card)
    {
      var cost = card.Cost;  //can buy card?
      var canbuy = cost <= MainPlayer.resnum("gold");
      var type = card.Type;
      if (type == "battle") canbuy = canbuy && MainPlayer.Cards.Where(x => x.Type == "military").Any(x => x.NumDeployed > 0);
      if (type == "war") canbuy = canbuy && !IsWar;
      if (type == "colony") canbuy = canbuy && MainPlayer.resnum("military") >= card.X.aint("milmin");
      return canbuy;
    }
    bool CalcCanTakeArchitect()
    {
      var canarch = Architects > 0; //can buy architect?
      var wic = MainPlayer.Cards[CardTypes.WIC];
      if (!wic.IsEmpty && wic.Type == "wonder")
      {
        var archNeeded = wic.X.astring("arch").Split('_'); // returns eg{"2","2"}
        var indexNeeded = archNeeded.Length - wic.NumDeployed - 1;
        var coalNeeded = int.Parse(archNeeded[indexNeeded]);
        canarch = canarch && MainPlayer.resnum("coal") >= coalNeeded;
      }
      return canarch;
    }
    bool CalcCanDeploy() { return MainPlayer.resnum("worker") > MainPlayer.Cards.Where(x => x.Type == "building" || x.Type == "military").Sum(x => x.NumDeployed); }
    public void PrepareForPlaceSelection(string type)
    {
      CanBuyProgressCard = false; // just for now!
      CanPlaceCard = true;
      var civcards = MainPlayer.Cards;
      foreach (var c in civcards) c.CanPlace = false;
      if (type == "colony") foreach (var c in civcards.Where(x => x.Type == "colony")) c.CanPlace = true;
      if (type == "building" || type == "military") foreach (var c in civcards.Where(x => x.Type == "building" || x.Type == "military")) c.CanPlace = true;
      //TODO: differentiate beteen natural wonders that are permanent vs not permanent! 
      // right now, consider all natural wonders as permanent
      if (type == "wonder" || type == "natural") foreach (var c in civcards.Where(x => x.Type == "wonder")) c.CanPlace = true;
      EnableUI=true;

    }
    public void BuyCard(Card card) //TODO: Commands, kommt von UI bei click card uebrigens card sollte faden nicht einfach verschwinden!!!
    {
      if (!card.CanBuy || !CanBuyProgressCard) { return; }//Phase.MessageTodo = "buying not possible!"; 

      //add card to mainplayer's civ board or process
      SelectedCard = card;
      var cost = card.Cost; //TODO: special rules!
      var pl = MainPlayer;
      //ProgressCards.Remove(card); NOT JUST YET for roll back
      //pl.Pay(cost);
      //for undo, need to remember replaced card!
      //Message = MainPlayer.Name + " bought card " + card.Name;
      switch (card.Type)
      {
        case "war":
          {
            UpdateWarPosition(pl, card);
            //OnNext();
          } // gleich weiter zu next player
          break;
        case "battle":
          {
            PrepareResourcePicker(new string[] { "wheat", "coal", "book" }, MainPlayer.resnum("raid"), "battle");
          }
          break;
        case "golden":
          //what is the resource of this golden age?
          var rname = card.X.astring("res");
          var rnum = card.X.aint("n"); // ist das immer = age? egal jetzt TODO implement golden age bonuses
          var vpcost = Age;
          var canafford = MainPlayer.resnum("gold") >= vpcost;
          if (!canafford)
          {
            Phase.MessageTodo = Message = MainPlayer.Name + ", you had to pick " + rname.ToCapital();
            MainPlayer.resupdate(rname, rnum);
            //OnNext(); //hier wird automatisch AniResourceUpdate gestarted
          }
          else
          {
            PrepareResourcePicker(new string[] { rname, "vp" }, rnum, "golden age");
          }
          break;
        case "wonder":
        case "natural":
          // remove architects! ignore extra architects for now! TODO: fix
          var numarch = MainPlayer.resnum("architects");
          // extra architects are also removed!
          //easy: just look at NumDeployed of WIC card
          MainPlayer.resupdate("architects", -numarch);
          MainPlayer.Place(card, MainPlayer.Cards[CardTypes.WIC]);
          //OnNext(); // place card on wic place, 
          break;
        case "advisor":
          MainPlayer.Place(card, MainPlayer.Cards[0]);
          //OnNext();
          break;
        case "colony":
          var numColonySpaces = MainPlayer.Cards.Count(x => x.Type == "colony" && !string.IsNullOrEmpty(x.Name));
          if (numColonySpaces == 1)
          {
            MainPlayer.Place(card, MainPlayer.Cards[CardTypes.WIC + 1]);
            //OnNext();
          }//TODO: HAck!
          else
          {
            PrepareForPlaceSelection(card.Type);
          }
          break;
        default:
          PrepareForPlaceSelection("building");
          break;
      }
    }
    public void PlaceCard(Card cardspace)
    {
      if (!cardspace.CanPlace || !CanPlaceCard) { return; }//Phase.MessageTodo = "buying not possible!"; 
      MainPlayer.Place(SelectedCard, cardspace);
      CanPlaceCard = false;
      //foreach (var card in MainPlayer.Cards) { card.CanPlace = true; card.CanBuy = true; }
      EnableUI=true;
      //OnNext();
    }

    public void ActionCompletedForPlayer(object sb, object target) { OnNext(); } //same as GrowthCompletedForPlayer





















    public void SetupForProductionPhase() { }
    public void Production() { }

    //public void StartRound()
    //{
    //  Round++;
    //  DealProgressCards();

    //  Phase.SetupNextGamePhaseAction?.Invoke(); //SetupForGrowthPhase();
    //  OnGrowth();
    //  // koennte game timer starten!...
    //}
    //public void AddCardToCivboard(string name)
    //{
    //  // simple: search first empty field on board
    //  // load card
    //  // place it there
    //  Message = MainPlayer.Name + " added card " + name + " to his board";
    //}

    //public void NextPlayer()
    //{
    //  //switch (CurrentPhase.Type)
    //  //{
    //  //  case PhaseType.growth:
    //  //    break;
    //  ////  case PhaseType.workerpick_growth:
    //  //    MainPlayer.CheckOutWorker(MainPlayer.ExtraWorkers.First(x => x.CostRes == res.Name));
    //  //    break;
    //  //}
    //}
    //public void StartPhase(Action PhaseStartAction)
    //{
    //  foreach (var pl in Players) pl.PhaseCompleted = false;
    //  PhaseStartAction?.Invoke();
    //}
    //public void OnPickBattle()
    //{
    //  var res = SelectedResource;
    //  if (res == null) { Phase.MessageTodo = "YOU IDIOT YOU DID NOT PICK ANYTHING!!!!!!!!!!!!!!!!!!!!!!!!!!!!"; EnableUI=true; return; }

    //  //did pick something:
    //  ShowChoicePicker = false;
    //  ResChoices.Clear();
    //  SelectedResource = null;
    //  Phase.MessageTodo = Message = MainPlayer.Name + ", you picked " + res.Name.ToCapital();
    //  MainPlayer.resupdate(res.Name, MainPlayer.resnum("raid")); //hier wird automatisch AniResourceUpdate gestarted
    //}
    //public void OnPickGoldenAge()
    //{
    //  var res = SelectedResource;
    //  if (res == null) { Phase.MessageTodo = "YOU IDIOT YOU DID NOT PICK ANYTHING!!!!!!!!!!!!!!!!!!!!!!!!!!!!"; EnableUI=true; return; }

    //  //did pick something:
    //  ShowChoicePicker = false;
    //  ResChoices.Clear();
    //  SelectedResource = null;
    //  Phase.MessageTodo = Message = MainPlayer.Name + ", you picked " + res.Name.ToCapital();
    //  MainPlayer.resupdate(SelectedCard.Name, MainPlayer.resnum("raid")); //hier wird automatisch AniResourceUpdate gestarted
    //}
    //public void MarkAllSelectableObjects()
    //{
    //  //hier brauche alle available actions
    //  //buy card action: mark cards that can be bought
    //  foreach (var card in ProgressCards)
    //  {
    //    // can buy card only if enough gold (ignore for now special rules: cost reduction? bonus? or japan  dyn2 cannot buy natural wonders...
    //    //var row = Helpers.GetRow(card.Field.Index, Cols); // Index = r * Cols + c
    //    //var cost = row + 1;
    //    var cost = card.Cost; //need Cost anyway in case he buys it, so Cost is set when cards are distributed
    //    var canbuy = cost <= MainPlayer.resnum("gold");
    //    // can buy battle only if deployed on military
    //    var type = card.Type;

    //    var cmil = MainPlayer.Cards.Where(x => x.Type == "military").ToArray();
    //    var depl = cmil.Sum(x => x.NumDeployed);

    //    if (type == "battle") canbuy = canbuy && MainPlayer.Cards.Where(x => x.Type == "military").Any(x => x.NumDeployed > 0);
    //    // can buy war only if no war has been bought
    //    if (type == "war") canbuy = canbuy && !IsWar;
    //    // colony only if mil > min, ignore spcial rules for now
    //    if (type == "colony") canbuy = canbuy && MainPlayer.resnum("military") >= card.X.aint("milmin");
    //    // special civ specific rules? some players might not have colony space, etc.. ignore that for now!
    //    card.CanBuy = canbuy;
    //  }

    //  // buy architect action:
    //  var canarch = Architects > 0;
    //  var wic = MainPlayer.Cards[CardTypes.WIC];
    //  if (!wic.IsEmpty && wic.Type == "wonder") //wonder or natural in construction is present //!string.IsNullOrEmpty(wic.Name)
    //  {
    //    var archNeeded = wic.X.astring("arch").Split('_'); // returns eg{"2","2"}
    //    var indexNeeded = archNeeded.Length - wic.NumDeployed - 1;
    //    var coalNeeded = int.Parse(archNeeded[indexNeeded]);
    //    canarch = canarch && MainPlayer.resnum("coal") >= coalNeeded;
    //  }
    //  CanTakeArchitect = canarch;

    //  // deploy worker
    //  MainPlayer.CanDeploy = MainPlayer.resnum("worker") > MainPlayer.Cards.Where(x => x.Type == "building" || x.Type == "military").Sum(x => x.NumDeployed);

    //  //for now ignore other actions such as check out extra worker...

    //}
    //public void PrepareForPlaceSelection(string type)
    //{
    //  CanBuyProgressCard = false; // just for now!
    //  CanPlaceCard = true;
    //  var civcards = MainPlayer.Cards;
    //  foreach (var c in civcards) c.CanPlace = false;
    //  if (type == "colony") foreach (var c in civcards.Where(x => x.Type == "colony")) c.CanPlace = true;
    //  if (type == "building" || type == "military") foreach (var c in civcards.Where(x => x.Type == "building" || x.Type == "military")) c.CanPlace = true;
    //  //TODO: differentiate beteen natural wonders that are permanent vs not permanent! 
    //  // right now, consider all natural wonders as permanent
    //  if (type == "wonder" || type == "natural") foreach (var c in civcards.Where(x => x.Type == "wonder")) c.CanPlace = true;
    //  EnableUI=true;

    //}
    //public void BuyCard(Card card) //TODO: Commands, kommt von UI bei click card uebrigens card sollte faden nicht einfach verschwinden!!!
    //{
    //  if (!card.CanBuy || !CanBuyProgressCard) { return; }//Phase.MessageTodo = "buying not possible!"; 

    //  //add card to mainplayer's civ board or process
    //  SelectedCard = card;
    //  var cost = card.Cost; //TODO: special rules!
    //  var pl = MainPlayer;
    //  //ProgressCards.Remove(card); NOT JUST YET for roll back
    //  //pl.Pay(cost);
    //  //for undo, need to remember replaced card!
    //  //Message = MainPlayer.Name + " bought card " + card.Name;
    //  switch (card.Type)
    //  {
    //    case "war":
    //      {
    //        UpdateWarPosition(pl, card);
    //        //OnNext();
    //      } // gleich weiter zu next player
    //      break;
    //    case "battle":
    //      {
    //        PrepareResourcePicker(new string[] { "wheat", "coal", "book" }, MainPlayer.resnum("raid"), "battle");
    //      }
    //      break;
    //    case "golden age":
    //      //what is the resource of this golden age?
    //      var rname = card.X.astring("res");
    //      var rnum = card.X.aint("n"); // ist das immer = age? egal jetzt TODO implement golden age bonuses
    //      var vpcost = Age;
    //      var canafford = MainPlayer.resnum("gold") >= vpcost;
    //      if (!canafford)
    //      {
    //        Phase.MessageTodo = Message = MainPlayer.Name + ", you had to pick " + rname.ToCapital();
    //        MainPlayer.resupdate(rname, rnum);
    //        //OnNext(); //hier wird automatisch AniResourceUpdate gestarted
    //      }
    //      else
    //      {
    //        PrepareResourcePicker(new string[] { rname, "vp" }, rnum, "golden age");
    //      }
    //      break;
    //    case "wonder":
    //    case "natural":
    //      // remove architects! ignore extra architects for now! TODO: fix
    //      var numarch = MainPlayer.resnum("architects");
    //      // extra architects are also removed!
    //      //easy: just look at NumDeployed of WIC card
    //      MainPlayer.resupdate("architects", -numarch);
    //      MainPlayer.Place(card, MainPlayer.Cards[CardTypes.WIC]);
    //      //OnNext(); // place card on wic place, 
    //      break;
    //    case "advisor":
    //      MainPlayer.Place(card, MainPlayer.Cards[0]);
    //      //OnNext();
    //      break;
    //    case "colony":
    //      var numColonySpaces = MainPlayer.Cards.Count(x => x.Type == "colony" && !string.IsNullOrEmpty(x.Name));
    //      if (numColonySpaces == 1)
    //      {
    //        MainPlayer.Place(card, MainPlayer.Cards[CardTypes.WIC + 1]);
    //        //OnNext();
    //      }//TODO: HAck!
    //      else
    //      {
    //        PrepareForPlaceSelection(card.Type);
    //      }
    //      break;
    //    default:
    //      PrepareForPlaceSelection("building");
    //      break;
    //  }
    //}
    //public void PlaceCard(Card cardspace)
    //{
    //  if (!cardspace.CanPlace || !CanPlaceCard) { return; }//Phase.MessageTodo = "buying not possible!"; 
    //  MainPlayer.Place(SelectedCard, cardspace);
    //  CanPlaceCard = false;
    //  //foreach (var card in MainPlayer.Cards) { card.CanPlace = true; card.CanBuy = true; }
    //  EnableUI=true;
    //  //OnNext();
    //}


  }
}
