using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
  public sealed partial class Game : DependencyObject, INotifyPropertyChanged
  {
    #region properties
    #region players
    const int MAX_PLAYERS = 5;
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
          if (oldmain != null) oldmain.IsMainPlayer = false;
          // wait for ui to be updated?!?!?
          mainPlayer.IsMainPlayer = true;
          NotifyPropertyChanged();
        }
      }
    }
    Player mainPlayer;
    #endregion
    #region progress board
    const int MAX_AGE = 2; //TODO: make decks for ages 3 and 4
    const int MAX_ROUNDS = 8;
    public XElement[] ProgressCardDeck { get; set; }
    public ObservableCollection<Card> ProgressCards { get; set; }
    public BitmapImage ProgressboardImage { get; set; }
    public Field[,] Progressboard { get; set; }
    public Thickness ProgressboardMargin { get; set; }
    public int Rows { get; set; }
    public int Cols { get; set; }
    #endregion
    public BitmapImage StatsboardImage { get; set; }
    //_____________________bis hier copiert
    public string Message { get { return message; } set { message = value; NotifyPropertyChanged(); } }
    string message;
    #region stats board
    public int Round
    {
      get { return round; }
      set
      {
        if (round != value)
        {
          round = value % MAX_ROUNDS;
          RoundMarkerPosition = new Point(pRoundMarker[round, 0]-221, pRoundMarker[round, 1]-6);
          Age = (round / 2) + 1;
        }
      }
    }
    int round = -1;
    public int Age
    {
      get { return age; }
      set
      {
        if (age != value)
        {
          age = value % (MAX_AGE + 1);
          UpdateCardDecks();
        }
      }
    }
    int age = -1;
    static int[,] pRoundMarker = { { 0, 116 }, { 70, 116 }, { 200, 116 }, { 280, 116 }, { 0, 216 }, { 70, 216 }, { 200, 216 }, { 280, 216 } };
    public Point RoundMarkerPosition { get { return roundMarkerPosition; } set { roundMarkerPosition = value; NotifyPropertyChanged(); } }
    Point roundMarkerPosition;
    public XElement[] EventCardDeck { get; set; }
    public Card EventCard { get; set; }
    public int Architects { get { return architects; } set { architects = value; NotifyPropertyChanged(); } }
    int architects;

    public bool CanTakeArchitect { get { return canTakeArchitect; } set { if (canTakeArchitect != value) { canTakeArchitect = value; NotifyPropertyChanged(); } } }
    bool canTakeArchitect;
    public bool CanTakeTurmoil { get { return canTakeTurmoil; } set { if (canTakeTurmoil != value) { canTakeTurmoil = value; NotifyPropertyChanged(); } } }
    bool canTakeTurmoil;
    public int Turmoils { get { return turmoils; } set { turmoils = value; NotifyPropertyChanged(); } }
    int turmoils;
    public bool IsWar { get; set; }
    public Point WarPosition { get { return warPosition; } set { warPosition = value; NotifyPropertyChanged(); } }
    Point warPosition;
    public Card WarCard { get { return warCard; } set { if (warCard != value) { warCard = value; NotifyPropertyChanged(); } } }
    Card warCard;
    #endregion
    public bool ShowChoicePicker { get { return showChoicePicker; } set { if (showChoicePicker != value) { showChoicePicker = value; NotifyPropertyChanged(); } } }
    bool showChoicePicker;
    public bool CanBuyProgressCard { get { return canBuyProgressCard; } set { if (canBuyProgressCard != value) { canBuyProgressCard = value; NotifyPropertyChanged(); } } }
    bool canBuyProgressCard; //TODO mit phases steuern, nicht so!
    public bool CanPlaceCard { get { return canPlaceCard; } set { if (canPlaceCard != value) { canPlaceCard = value; NotifyPropertyChanged(); } } }
    bool canPlaceCard; //TODO mit phases steuern, nicht so!
    public ObservableCollection<Res> ResChoices { get; set; }
    public Card SelectedCard { get; set; }
    public int Number { get; set; }
    public int WorkerNumber { get; set; }
    public Res SelectedResource { get { return (Res)GetValue(SelectedResourceProperty); } set { SetValue(SelectedResourceProperty, value); } }
    public static readonly DependencyProperty SelectedResourceProperty = DependencyProperty.Register("SelectedResource", typeof(Res), typeof(Game), null);
    public int[] LevelGrowth = { 4, 3, 2, 1 };

    //TODO: weg mit:
    //public Action ResetAction { get; set; }
    //public Action KickoffAction { get; set; }
    public bool EnableUI { get { return enableUI; } set { if (enableUI != value) { enableUI = value; NotifyPropertyChanged(); } } }
    bool enableUI;
    public bool UIFreeze { get { return uiFreeze; } set { if (uiFreeze != value) { uiFreeze = value; NotifyPropertyChanged(); } } }
    bool uiFreeze;//TODO: eliminate! gibt schon bindings, achtung!!!

    //public FrameworkElement UIEvent { get; set; } // wird animated am ende von Event Phase
    public Button NextButton { get; set; } // allow game to freeze ui immediately by disabling next button, simplest solution really even if not MVVM
    public FrameworkElement RoundMarkerUI { get; set; } // allow game to freeze ui immediately by disabling next button, simplest solution really even if not MVVM
    #endregion

    #region constructor
    private static readonly Game instance = new Game();
    public static Game GameInstance { get { return instance; } }    private Game() { Initialize(); }
    public void Reset() { Initialize(); }//TODO: CANNOT BE DONE IN FREEZE!!! todo check ob wirklich alles damit gecleared ist!
    public void Initialize()
    {
      ShowChoicePicker = false;
      Message = "Let's Play Nations!!!";
      GamePhases = new ObservableCollection<GamePhase>();
      ResChoices = new ObservableCollection<Res>();
      StatsboardImage = Helpers.GetStatsboardImage();

      NumPlayers = 2;
      Rows = 3;
      Cols = NumPlayers + 2;

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

      ProgressboardImage = Helpers.GetProgressboardImage();
      Progressboard = new Field[Rows, Cols];
      ProgressboardMargin = new Thickness(72, 50, 576 - 190 * (Cols - 4), -6); //fuer uniformgrid
      for (int r = 0; r < Rows; r++)
        for (int c = 0; c < Cols; c++)
          Progressboard[r, c] = new Field { Index = r * Cols + c, Margin = Helpers.GetMarginOfFieldOnProgressBoard(r, c) };
      ProgressCards = new ObservableCollection<Card>();

      var env = Environment.CurrentDirectory; // find out if this is runtime or design time
      if (!env.ToLower().StartsWith("d")) // this is design time instance
      {
        TestSetup1();
      }
      else Setup();
    }

    void TestSetup1()
    {
      ResChoices.Clear();
      SelectedResource = null;
      RandomPlayerOrder();
      GamePhases.Clear();
      ResChoices.Add(new Res("wheat"));
      ResChoices.Add(new Res("coal"));
      ResChoices.Add(new Res("gold"));
      ResChoices.Add(new Res("worker"));
      ShowChoicePicker = true;
      Round = 0;
      Age = 1; //normally auto Round setter, but this time will not trigger because default round at 0!
      UpdateCardDecks(); //auto Age setter
      DealProgressCards();
      PickEventCard();
      MainPlayer = Players[0];
      SetupForRoundMarkerPhase();
      Round++;
    }
    void TestSetup2()
    {
      SetupForRoundMarkerPhase();
      Round++;
      foreach (var pl in Players) pl.Defaulted.Clear(); //TODO mach ordentliches player.NewPhase und player.NewRound
      SetupForProgressCardsPhase();
      DealProgressCards();
      SetupForGrowthPhase();
      PickEventCard();
      SetupForActionPhase();
      PlayerAction();
    }
    #endregion

    public void RandomPlayerOrder()
    {
      var plarr = Players.OrderBy(x => Rand.N()).ToArray();
      Players.Clear();
      foreach (var pl in plarr) Players.Add(pl);
      for (int i = 0; i < Players.Count; i++) Players[i].Index = i;
      MainPlayer = Players[0];
    }
    public void UpdateWarPosition(Player p=null, Card card=null)
    {
      if (p == null) { WarPosition = new Point(0, 0); WarCard = null; }
      else
      {
        Point milMargin = new Point(150, 1100);
        int diff = 10;
        WarPosition = new Point(p.MilitaryPosition.X + milMargin.X - diff, p.MilitaryPosition.Y - milMargin.Y - diff);
        IsWar = true;
        if (card != null) WarCard = card;
      }
    }
    void EnsureAgeLimits() { if (Round < 0) Round = 0; else if (Round > MAX_ROUNDS) Round = MAX_ROUNDS - 1; if (Age < 1) Age = 1; else if (Age > MAX_AGE) Age = MAX_AGE; }
    public void UpdateCardDecks()
    {
      EnsureAgeLimits();
      ProgressCardDeck = Helpers.GetCardarrayX(Age).OrderBy(x => Rand.N()).ToArray();
      EventCardDeck = Helpers.GetEventCardarrayX(Age).OrderBy(x => Rand.N()).ToArray();

    }
    public void DealProgressCards()
    {
      if (ProgressCardDeck == null) UpdateCardDecks();
      var cards = ProgressCardDeck.Take(Rows * Cols).ToArray();
      ProgressCardDeck = ProgressCardDeck.Skip(Rows * Cols).ToArray();
      if (ProgressCardDeck.Length < Rows * Cols) UpdateCardDecks(); //just in case of triggering redeal very often!
      ProgressCards.Clear();
      for (int r = 0; r < Rows; r++)
        for (int c = 0; c < Cols; c++)
        {
          var card = Card.MakeProgressCard(cards[r * Cols + c], Progressboard[r, c]);
          card.Cost = 3*(3 - r); //cards of row 3 not affordable unless picked gold! nur zum testen! 3-r;
          ProgressCards.Add(card);
        }
    }
    public void PickEventCard()
    {
      if (EventCardDeck.Count() == 0) UpdateCardDecks(); // just in case run out of event cards (which doesnt normally happen)
      EventCard = Card.MakeEventCard(EventCardDeck.First());
      Turmoils = Architects = EventCard.X.aint("architects") + NumPlayers - (NumPlayers <= 3 ? 1 : 2);
      NotifyPropertyChanged("EventCard");
    }

    #region helpers

    public event PropertyChangedEventHandler PropertyChanged; public void NotifyPropertyChanged([CallerMemberName] string propertyName = null) { this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
    #endregion
    //public void Reset() // just for testing! weil das soll nur mit new Game gehen!!!
    //{
    //  Round = -1;
    //  RandomPlayerOrder();
    //}
    //public Boolean State
    //{
    //  get { return (Boolean)this.GetValue(StateProperty); }
    //  set { this.SetValue(StateProperty, value); }
    //}
    //public static readonly DependencyProperty StateProperty = DependencyProperty.Register(
    //  "State", typeof(Boolean), typeof(Game), new PropertyMetadata(false));
    #region ablauf old
    //public PhaseType phase; //default value: PhaseType.not_started phase this out!!!
    //int playerOrderIndex = 0;
    //public void NextPlayer_old()
    //{
    //  playerOrderIndex++;
    //  if (playerOrderIndex >= Players.Count) playerOrderIndex = 0;
    //  MainPlayer = Players[playerOrderIndex];
    //}
    //public void NextPhase()
    //{
    //  switch (phase)
    //  {
    //    case PhaseType.growth: PickEventCard(); phase = PhaseType.action; break;
    //    case PhaseType.action: phase = PhaseType.production; break;
    //    default: phase = PhaseType.growth; break;
    //  }
    //}
    //public void WorkerChoice() // prompt MainPlayer to choose between available workers to check out
    //{
    //  ResChoices.Clear();
    //  var resarr = MainPlayer.ExtraWorkers.Where(x => !x.IsCheckedOut).Select(x => x.CostRes).Distinct().ToArray();
    //  foreach (var rname in resarr) ResChoices.Add(new Res(rname));
    //  Message = "pick a worker type";
    //  Application.Current.Dispatcher.BeginInvoke(new Action(() => ShowChoicePicker = true), DispatcherPriority.Loaded);
    //}
    //public void ResourceSelected(Res res)
    //{
    //  ShowChoicePicker = false;
    //  Application.Current.Dispatcher.BeginInvoke(new Action(() => ProcessResourceSelected(res)), DispatcherPriority.Loaded);
    //}
    //public void ProcessResourceSelected(Res res)
    //{
    //  switch (phase)
    //  {
    //    case PhaseType.growth:
    //      if (res.Name == "worker")
    //      {
    //        var workers = MainPlayer.ExtraWorkers;
    //        var wfree1 = workers.FirstOrDefault(x => !x.IsCheckedOut);
    //        var wfree2 = workers.LastOrDefault(x => !x.IsCheckedOut);
    //        if (wfree1.CostRes != wfree2.CostRes) { WorkerChoice(); return; }
    //        else MainPlayer.CheckOutWorker(wfree1);
    //      }
    //      else { MainPlayer.resupdate(res.Name, LevelGrowth[MainPlayer.Level]); }
    //      break;
    //      //case PhaseType.workerpick_growth:
    //      //  MainPlayer.CheckOutWorker(MainPlayer.ExtraWorkers.First(x => x.CostRes == res.Name));
    //      //  break;
    //  }
    //}
    //public void NextPlayerGrowth() //nach einer animation!
    //{
    //  NextPlayer_old();
    //  if (playerOrderIndex == 0) NextPhase(); else Growth();
    //}
    #endregion




  }
}
