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
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml.Linq;

namespace ations
{
  #region design time
  public class AGameDesignTime { public AGame Game { get; set; } public AGameDesignTime() { Game = AGame.GameInstance; } }
  #endregion
  public class AGame : DependencyObject, INotifyPropertyChanged
  {
    #region constants
    public const int MAX_PLAYERS = 5;
    public const int MAX_AGE = 2; //TODO: make decks for ages 3 and 4
    public const int MAX_ROUNDS = 8;
    public static int[] LevelGrowth = { 4, 3, 2, 1 };
    #endregion
    #region properties
    public int NumPlayers { get; set; }
    public ObservableCollection<APlayer> Players { get; set; }
    public APlayer MainPlayer
    {
      get { return mainPlayer; }
      set
      {
        if (mainPlayer != value)
        {
          var oldmain = mainPlayer;
          mainPlayer = value;
          if (oldmain != null) oldmain.IsMainPlayer = false;
          mainPlayer.IsMainPlayer = true;
          NotifyPropertyChanged();
        }
      }
    }
    APlayer mainPlayer;
    public AProgress Progress { get; set; }
    public AStats Stats { get; set; }
    public bool CanTakeArchitect { get { return canTakeArchitect; } set { if (canTakeArchitect != value) { canTakeArchitect = value; NotifyPropertyChanged(); } } }
    bool canTakeArchitect;
    public bool CanTakeTurmoil { get { return canTakeTurmoil; } set { if (canTakeTurmoil != value) { canTakeTurmoil = value; NotifyPropertyChanged(); } } }
    bool canTakeTurmoil;
    public bool CanBuyProgressCard { get { return canBuyProgressCard; } set { if (canBuyProgressCard != value) { canBuyProgressCard = value; NotifyPropertyChanged(); } } }
    bool canBuyProgressCard; //TODO mit phases steuern, nicht so!
    public bool CanPlaceCard { get { return canPlaceCard; } set { if (canPlaceCard != value) { canPlaceCard = value; NotifyPropertyChanged(); } } }
    bool canPlaceCard; //TODO mit phases steuern, nicht so!

    public bool ShowStartButton { get { return showStartButton; } set { if (showStartButton != value) { showStartButton = value; NotifyPropertyChanged(); } } }
    bool showStartButton;
    public bool ShowBuyButton { get { return showBuyButton; } set { if (showBuyButton != value) { showBuyButton = value; NotifyPropertyChanged(); } } }
    bool showBuyButton;
    public bool ShowResChoices { get { return showResChoices; } set { if (showResChoices != value) { showResChoices = value; NotifyPropertyChanged(); } } }
    bool showResChoices;
    public bool ShowChoicePicker { get { return showChoicePicker; } set { if (showChoicePicker != value) { showChoicePicker = value; NotifyPropertyChanged(); } } }
    bool showChoicePicker;
    public bool ShowWorkerPicker { get { return showWorkerPicker; } set { if (showWorkerPicker != value) { showWorkerPicker = value; NotifyPropertyChanged(); } } }
    bool showWorkerPicker;

    public string Message { get { return message; } set { message = value; NotifyPropertyChanged(); } }
    string message;
    public string Caption { get { return caption; } set { caption = value; NotifyPropertyChanged(); } }
    string caption;
    public string Title { get { return title; } set { title = value; NotifyPropertyChanged(); } }
    string title;

    public ObservableCollection<ARes> ResChoices { get; set; }
    public ARes SelectedResource { get { return (ARes)GetValue(SelectedResourceProperty); } set { SetValue(SelectedResourceProperty, value); } }
    public static readonly DependencyProperty SelectedResourceProperty = DependencyProperty.Register("SelectedResource", typeof(ARes), typeof(AGame), null);
    public ACard SelectedCard { get; set; }
    public int Number { get; set; }

    #endregion
    #region initialization
    private static readonly AGame instance = new AGame(); public static AGame GameInstance { get { return instance; } }
    private AGame() { Initialize(); }
    void InitPlayers(int num)
    {
      Players = new ObservableCollection<APlayer>();
      string[] names = { "Felix", "Amanda", "Taka", "Franzl", "Bertl" };
      string[] civs = { "america", "egypt", "arabia", "china", "ethiopia" }; //TODO: choose civs,random civs,daten eingeben!
      Brush[] brushes = {
        new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0050EF")), //blue
        new SolidColorBrush(Color.FromArgb(255, 96, 169, 23)),//Green
        new SolidColorBrush(Color.FromArgb(255, 250, 104, 0)),//orange
        new SolidColorBrush(Color.FromArgb(255,170, 0, 255)),//violet
        Brushes.Sienna
      };
      for (int i = 0; i < NumPlayers; i++) Players.Add(new APlayer(names[i], civs[i], brushes[i], Levels.Chieftain, i));
      MainPlayer = Players[0];
    }
    void Initialize()
    {
      ResChoices = new ObservableCollection<ations.ARes>();

      NumPlayers = 5;
      InitPlayers(NumPlayers);
      Progress = new AProgress(7);// NumPlayers + 2);
      Stats = new AStats(this);

      Title = "Nations Start!";Message = "Hi, " + MainPlayer.Name+", press Start!" ; ShowStartButton = true; Kickoff = StartRound;
    }
    #endregion

    public Action Kickoff { get; set; }
    Action AllPlayers;
    Action AfterAllPlayers;
    int playerIndex;

    public void StartRound()
    {
      ShowStartButton = false;
      Stats.UpdateRound();
      Stats.UpdateAge();
      Progress.Deal();
      AllPlayers = Growth;
      AfterAllPlayers = EventAndAction;
      playerIndex = 0;
      Debug.Assert(MainPlayer == Players[0], "StartRound with wrong player!!!!!");

      AAnimations.AniRoundMarker(Stats.RoundMarkerPosition, (x) => Growth());
    }
    //******************** version 0 **************************
    public void Growth()
    {
      Title = "Growth"; AAnimations.AfterAnimation = (x) => NextPlayer(); // ani in ui ausgeloest onResourceUpdate

      var reslist = new List<string> { "wheat", "coal", "gold" };
      if (MainPlayer.ExtraWorkers.Count(x => !x.IsCheckedOut) > 0) reslist.Add("worker");
      var num = LevelGrowth[MainPlayer.Level];
      PrepareResourcePicker(reslist, num, "growth");
    }
    public void PrepareResourcePicker(IEnumerable<string> resnames, int num, string forWhat)
    {
      Debug.Assert(ResChoices != null && ResChoices.Count == 0, "PrepareResourcePicker ResChoices init error!");
      foreach (var rname in resnames) ResChoices.Add(new ARes(rname));
      Message = "pick " + forWhat + " resource";
      Number = num;
      ShowChoicePicker = true;
      ShowResChoices = true;
    }
    public void PrepareWorkerPicker()
    {
      ResChoices.Clear();
      var resarr = MainPlayer.ExtraWorkers.Where(x => !x.IsCheckedOut).Select(x => x.CostRes).Distinct().ToArray();
      foreach (var rname in resarr) ResChoices.Add(new ARes(rname));
      Message = "pick worker type";
      ShowResChoices = true;
      ShowWorkerPicker = true;
    }
    //hier wartet auf user pick, der geht zu OnPick oder zu OnPickWorker
    public void OnPick()
    {
      var res = SelectedResource;
      if (res == null) { Message = "YOU DID NOT PICK ANYTHING!"; return; }

      //did pick something:
      ShowResChoices = false;
      ShowChoicePicker = false;
      ResChoices.Clear();
      SelectedResource = null;
      Message = MainPlayer.Name + ", you picked " + res.Name.ToCapital();

      if (res.Name == "worker")
      {
        var workers = MainPlayer.ExtraWorkers;
        var wfree1 = workers.FirstOrDefault(x => !x.IsCheckedOut);
        var wfree2 = workers.LastOrDefault(x => !x.IsCheckedOut);
        if (wfree1.CostRes != wfree2.CostRes) PrepareWorkerPicker();
        else { MainPlayer.CheckOutWorker(wfree1); }
      }
      else { MainPlayer.Res.inc(res.Name, Number); }
    }
    public void OnPickWorker()
    {
      var res = SelectedResource;
      if (res == null) { Message = "YOU DID NOT PICK ANYTHING!"; }
      else
      {
        //did pick: 
        ShowResChoices = false;
        ShowWorkerPicker = false;
        ResChoices.Clear();
        SelectedResource = null;
        var worker = MainPlayer.ExtraWorkers.FirstOrDefault(x => !x.IsCheckedOut && x.CostRes == res.Name);
        MainPlayer.CheckOutWorker(worker);
        Message = MainPlayer.Name + ", this worker will cost you 3 " + res.Name;
      }
    }
    //hier kommt eine animation, danach geht es zu NextPlayer
    void NextPlayer()
    {
      playerIndex++;
      if (playerIndex >= NumPlayers)
      {
        MainPlayer = Players[0];
        AfterAllPlayers?.Invoke();
      }
      else
      {
        MainPlayer = Players[playerIndex];
        AllPlayers?.Invoke();
      }
    }

    public void EventAndAction()
    {
      Stats.PickEventCard();
      AllPlayers = PlayerAction;
      AfterAllPlayers = Production;
      Title = "Action Phase"; Message = "Hi, " + MainPlayer.Name + ", press Start!"; ShowStartButton = true; Kickoff = PlayerAction;


    }
    public void PlayerAction() { ShowStartButton = false; MarkPossibleProgressCards(); ShowBuyButton = true; }
    public void MarkPossibleProgressCards()
    {
      UnmarkProgresscards();
      foreach (var f in Progress.Fields) f.Card.CanBuy = CalculateCanBuy(f);
    }
    public bool IsUnambiguousBuy(AField field)
    {
      var card = field.Card;
      var possible = GetPossiblePlacesForCard(field).ToArray();
      var isciv = card.civ();
      return !isciv || possible.Length == 1;
    }
    public void Buy(AField field) //assumes unambiguous buy
    {
      var card = field.Card;
      if (card.civ()) Buy(field, GetPossiblePlacesForCard(field).First());
      else
      {
        Progress.Remove(field);
        MainPlayer.Pay(card.Cost);

        if (card.war()) Stats.UpdateWarPosition(MainPlayer, card);
        //else if (card.golden()) 
      }
      // fuer andere brauch ich erstmal die players und das stats board
    }
    public void Buy(AField fieldBuy, AField fieldPlace)
    {
      var card = fieldBuy.Card;
      card.CanBuy = false;
      MainPlayer.Civ.Add(card, fieldPlace);
      Progress.Remove(fieldBuy);
      MainPlayer.Pay(card.Cost);
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

    public void Production() { }



    #region other safe helpers
    public IEnumerable<AField> GetPossiblePlacesForCard(AField field)
    {
      var card = field.Card;
      return MainPlayer.Civ.Fields.Where(x => x.TypesAllowed.Contains(card.Type)).ToArray();
    }
    public void MarkPossiblePlaces(AField field)
    {
      UnmarkPlaces();
      foreach (var f in GetPossiblePlacesForCard(field)) f.Card.CanPlace = true;
    }
    public void UnmarkPlaces()
    {
      foreach (var f in MainPlayer.Civ.Fields) f.Card.CanPlace = false;
    }
    public void UnmarkProgresscards()
    {
      foreach (var f in Progress.Fields) f.Card.CanBuy = false;
    }
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



























    //**********************************************************mist
    //public void Growth()
    //{
    //  Title = "Growth";
    //  var reslist = new List<string> { "wheat", "coal", "gold" };
    //  var actionlist = new List<Action<string, int>> { AddRes, AddRes, AddRes };
    //  var num = LevelGrowth[MainPlayer.Level];
    //  var numlist = new List<int> { num, num, num };

    //  TryAddWorkerPick(reslist, numlist, actionlist);

    //  var picklist = new List<Pick>();
    //  for(int i = 0; i < reslist.Count; i++)
    //  {
    //    picklist.Add(new Pick(reslist[i], numlist[i], actionlist[i]));
    //  }

    //  StartPickProcess(picklist, NextPlayer);
    //}
    //public void TryAddWorkerPick(List<string>reslist,List<int>numlist,List<Action<string,int>> actionlist)
    //{
    //  if (MainPlayer.ExtraWorkers.Count(x => !x.IsCheckedOut) > 0)
    //  {
    //    reslist.Add("worker");
    //    actionlist.Add(WorkerType);
    //    numlist.Add(1);
    //  }
    //}
    //public Stack<List<Pick>> pickplan;
    //public Action afterPickplanCompleted;
    //public class Pick
    //{
    //  public string resname;
    //  public int num;
    //  public Action<string, int> action;
    //  public Pick(string s, int n, Action<string, int> act) { resname = s; num = n; action = act; }
    //  public override string ToString()
    //  {
    //    return resname + ", " + num + ", " + action;
    //  }
    //}
    //public void StartPickProcess(List<Pick> picklist,Action onCompleted)
    //{
    //  pickplan = new Stack<List<Pick>>();
    //  pickplan.Push(picklist);
    //  afterPickplanCompleted = onCompleted;

    //  ShowChoicePicker = true;
    //  StartPicker();
    //}
    //public void StartPicker()
    //{
    //  var picklist1 = pickplan.Peek();
    //  Debug.Assert(picklist1.Count > 0, "Empty picklist in StartPicker!!!");

    //  // eliminate items from picklist that are unavailable
    //  var newpicklist = new List<Pick>();
    //  foreach(var pick in picklist1)
    //  {
    //    if (pick.resname == "worker" && )
    //  }

    //  // check dass picklist mehr als 1 pick hat
    //  if (picklist1.Count == 1)
    //  {
    //    var pick = picklist1[0];
    //    pick.action?.Invoke(pick.resname, pick.num);
    //  }
    //  // check ob ueberhaupt available! (workers or architects are limited!)


    //  ResChoices.Clear();
    //  foreach (var p in picklist1) { ResChoices.Add(new ARes(p.resname)); }
    //}
    //// hier wartet auf user to pick resource
    //public void OnPick()
    //{
    //  var res = SelectedResource;
    //  var picklist1 = pickplan.Peek();
    //  var pick = picklist1.FirstOrDefault(x => x.resname == res.Name);
    //  if (pick == null)
    //  {
    //    Message = "Programmierfehler!!!!! OnPick";
    //  }
    //  else
    //  {
    //    pick.action?.Invoke(pick.resname, pick.num);
    //  }

    //}
    //public void AddRes(string resname, int n) { MainPlayer.Res.inc(resname, n); NextPick(); }
    //public void WorkerType(string resname, int n)
    //{
    //  Message = "picked WORKER";
    //  if (!MainPlayer.MoreThanOneWorkerAvailable())
    //  {
    //    var worker = MainPlayer.ExtraWorkers.FirstOrDefault(x => !x.IsCheckedOut);
    //    Debug.Assert(worker != null, "no worker available (WorkerType)");
    //    MainPlayer.CheckOutWorker(worker);
    //    NextPick();
    //  }
    //  else
    //  {
    //    pickplan.Pop();
    //    for (int i = 1; i < n; i++)
    //    {
    //      var picklist = new List<Pick>();

    //    }
    //    // muss n picklists in den plan geben, alle worker choices,1, danach AddWorkerOfType(resname,n);
    //  }
    //} // change stack and go on picking
    //public void AddWorkerOfType(string resname,int n)
    //{
    //  var worker = MainPlayer.ExtraWorkers.FirstOrDefault(x => !x.IsCheckedOut && x.CostRes == resname);
    //  Debug.Assert(worker != null, "no worker with picked resource as cost available (AddWorkerOfType)");
    //  MainPlayer.CheckOutWorker(worker);
    //  NextPick();
    //}
    //public void NextPick()
    //{
    //  pickplan.Pop();
    //  if (pickplan.Count > 0) StartPicker(); else EndPickProcess();
    //}
    //public void EndPickProcess()
    //{
    //  ShowChoicePicker = false;
    //  ResChoices.Clear();
    //  SelectedResource = null;

    //  afterPickplanCompleted?.Invoke();
    //}
    //void NextPlayer()
    //{
    //  playerIndex++; 
    //  if (playerIndex >= NumPlayers)
    //  {
    //    MainPlayer = Players[0];
    //    AfterAllPlayers?.Invoke();
    //  }
    //  else
    //  {
    //    MainPlayer = Players[playerIndex];
    //    AllPlayers?.Invoke();
    //  }
    //}
  }
}
