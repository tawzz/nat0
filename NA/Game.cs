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

namespace ations
{
  #region design time
  public class AGameDesignTime { public Game Game { get; set; } public AGameDesignTime() { Game = Game.GameInstance; } }
  #endregion
  public partial class Game : DependencyObject, INotifyPropertyChanged
  {
    #region constants
    public const int MAX_PLAYERS = 5;
    public const int MAX_AGE = 2;
    public const int MAX_ROUNDS = 8;
    public static int[] LevelGrowth = { 4, 3, 2, 1 };

    #endregion

    #region properties and constructor/singleton

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
          Message = "Hi, " + mainPlayer.Name + "!"; //testing!
          if (oldmain != null) oldmain.IsMainPlayer = false;
          if (mainPlayer != null) mainPlayer.IsMainPlayer = true;
          NotifyPropertyChanged();
        }
      }
    }
    Player mainPlayer;
    public Progress Progress { get; set; }
    public Stats Stats { get; set; }

    public string Message { get { return message; } set { message = value; NotifyPropertyChanged(); } }
    string message;
    public string LongMessage { get { return longMessage; } set { longMessage = value; NotifyPropertyChanged(); } }
    string longMessage;
    public string Caption { get { return caption; } set { caption = value; NotifyPropertyChanged(); } }
    string caption = "Start";
    public string Title { get { return title; } set { title = value; NotifyPropertyChanged(); } }
    string title;
    public bool Interrupt { get; set; }
    public bool IsRunning { get; private set; }
    public Action Kickoff { get; private set; }
    public List<Storyboard> AnimationQueue { get; set; }
    public FrameworkElement UIRoundmarker { get; set; }


    //constructor: singleton
    private static readonly Game instance = new Game(); public static Game GameInstance { get { return instance; } }
    private Game() { Initialize(); }

    //initialization helpers
    void InitPlayers(int num)
    {
      Players = new ObservableCollection<Player>();
      string[] names = { "Felix", "Amanda", "Taka", "Franzl", "Bertl" };
      bool[] isai = { false, true, true, true, true };
      string[] civs = { "america", "egypt", "arabia", "china", "ethiopia" }; //TODO: choose civs,random civs,daten eingeben!
      Brush[] brushes = {
        new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0050EF")), //blue
        new SolidColorBrush(Color.FromArgb(255, 96, 169, 23)),//Green
        new SolidColorBrush(Color.FromArgb(255, 250, 104, 0)),//orange
        new SolidColorBrush(Color.FromArgb(255,170, 0, 255)),//violet
        Brushes.Sienna
      };
      for (int i = 0; i < NumPlayers; i++)
        Players.Add(isai[i] ? new AI(names[i], civs[i], brushes[i], Levels.Chieftain, i) : new Player(names[i], civs[i], brushes[i], Levels.Chieftain, i));
      MainPlayer = Players[0];
    }

    #endregion

    #region initialize

    void Initialize()
    {
      //SelectedResources = new ObservableCollection<Res>();
      ResChoices = new ObservableCollection<Res>();
      NetProduction = new ObservableCollection<Res>();
      Choices = new ObservableCollection<ations.Choice>();
      AnimationQueue = new List<Storyboard>();
      ContextStack = new Stack<ContextInfo>();

      NumPlayers = 2;
      InitPlayers(NumPlayers);
      Progress = new Progress(7);// NumPlayers + 2); //testing
      Stats = new Stats(this);

      SetupForStart();
      //Stats.WarCard = ACard.MakeCard("hyksos_invasion", 1); //testing design
      //Stats.EventCard = ACard.MakeEventCard("attila"); //testing design



    }
    public void SetupForStart() //called after initialization
    {
      Title = "Nations Start!"; Message = "press Start!"; Caption = "Start"; LongMessage = "game initialization complete";
      IsOkStartEnabled = true; Kickoff = GameLoop;//Tester;// testing

      ////testing
      //Choices.Add(new Choice { Text = "Upgrade Dynasty erst einmal als ganz langer choices der sicher 3 zeilen braucht" });
      //Choices.Add(new Choice { Text = "Upgrade Dynasty" });
      //Choices.Add(new Choice { Text = "Take 2 Gold" });
      //ShowChoices = true;
    }

    #endregion

    #region choice selection

    public ObservableCollection<Choice> Choices { get; set; }
    public bool ShowChoices { get { return showChoices; } set { showChoices = value; NotifyPropertyChanged(); } }
    bool showChoices;
    public Choice SelectedChoice { get { return (Choice)GetValue(SelectedChoiceProperty); } set { SetValue(SelectedChoiceProperty, value); } }
    public static readonly DependencyProperty SelectedChoiceProperty = DependencyProperty.Register("SelectedChoice", typeof(Choice), typeof(Game), null);

    async Task<Choice> WaitForPickChoiceCompleted(IEnumerable<string> choiceTexts, string forWhat)
    {
      Debug.Assert(Choices != null && Choices.Count == 0, "Choices null or not cleared for choice picker");
      Debug.Assert(SelectedChoice == null, "start choice picker with choice already selected!");

      foreach (var text in choiceTexts) Choices.Add(new Choice { Text = text });
      ShowChoices = true; Caption = "Pick"; Message = MainPlayer.Name + ", pick " + forWhat + " choice";

      var choice = await MakeSureUserPicksAChoice();

      ShowChoices = false; Choices.Clear(); SelectedChoice = null;
      Message = MainPlayer.Name + " picked " + choice.Text.ToCapital();

      return choice;
    }
    async Task<Choice> MakeSureUserPicksAChoice()
    {
      while (SelectedChoice == null)
      {
        if (Interrupt) throw (new Exception("STOPPED BY PLAYER!"));
        await Task.Delay(shortDelay);
        await WaitForButtonClick();

        var choice = SelectedChoice;
        if (choice == null) { Message = "YOU DID NOT PICK ANYTHING!"; }
      }
      return SelectedChoice;
    }

    #endregion

    #region  resource selection/update/production

    public ObservableCollection<Res> ResChoices { get; set; }
    public ObservableCollection<Res> NetProduction { get; set; }
    public bool ShowResChoices { get { return showResChoices; } set { if (showResChoices != value) { showResChoices = value; NotifyPropertyChanged(); } } }
    bool showResChoices;
    public bool ShowProduction { get { return showProduction; } set { if (showProduction != value) { showProduction = value; NotifyPropertyChanged(); } } }
    bool showProduction;
    public bool ShowMultiResChoices { get { return showMultiResChoices; } set { if (showMultiResChoices != value) { showMultiResChoices = value; NotifyPropertyChanged(); } } }
    bool showMultiResChoices;
    public Res SelectedResource { get { return (Res)GetValue(SelectedResourceProperty); } set { SetValue(SelectedResourceProperty, value); } }
    public static readonly DependencyProperty SelectedResourceProperty = DependencyProperty.Register("SelectedResource", typeof(Res), typeof(Game), null);
    //public ObservableCollection<Res> SelectedResources { get { return (ObservableCollection<Res>)GetValue(SelectedResourcesProperty); } set { SetValue(SelectedResourcesProperty, value); } }
    //public static readonly DependencyProperty SelectedResourcesProperty = DependencyProperty.Register("SelectedResources", typeof(ObservableCollection<Res>), typeof(Game), null);
    public int Number { get; set; }
    public Dictionary<string, int> NumEach { get; set; }

    public void ResourceUpdated(FrameworkElement ui)
    {
      var sb = Storyboards.Scale(ui, TimeSpan.FromSeconds(.3), new Point(1, 1), new Point(5, 2), null, true);
      AnimationQueue.Add(sb);
    }
    public void NumDeployedUpdated(FrameworkElement ui)
    {
      var card = (ui.DataContext as Field).Card;
      if (card.NumDeployed <= 0) return;
      var sb = Storyboards.Scale(ui, TimeSpan.FromSeconds(.3), new Point(1, 1), new Point(5, 2), null, true);
      AnimationQueue.Add(sb);
    }
    async Task<Res> MakeSureUserPicksAResource()
    {
      while (SelectedResource == null)
      {
        await Task.Delay(shortDelay);
        await WaitForButtonClick();

        var res = SelectedResource;
        if (res == null) { Message = "YOU DID NOT PICK ANYTHING!"; }
      }
      return SelectedResource;
    }
    async Task<Res> WaitForPickResourceCompleted(IEnumerable<string> resnames, int num, string forWhat)
    {
      Debug.Assert(ResChoices != null && ResChoices.Count == 0, "ResChoices not cleared for growth resource pick");
      Debug.Assert(SelectedResource == null, "start resource pick with resource already selected!");

      foreach (var rname in resnames) ResChoices.Add(new Res(rname));
      Number = num;
      ShowResChoices = true; Caption = "Pick"; Message = MainPlayer.Name + ", pick " + forWhat + " resource";

      var res = await MakeSureUserPicksAResource();

      ShowResChoices = false; ResChoices.Clear(); SelectedResource = null;
      Message = MainPlayer.Name + " picked " + res.Name.ToCapital();

      if (res.Name == "vp") Number = 1;

      if (res.Name == "worker")
      {
        var workers = MainPlayer.ExtraWorkers;
        var wfree1 = workers.FirstOrDefault(x => !x.IsCheckedOut);
        var wfree2 = workers.LastOrDefault(x => !x.IsCheckedOut);
        if (wfree1.CostRes != wfree2.CostRes)
        {
          Debug.Assert(ResChoices != null && ResChoices.Count == 0, "ResChoices not cleared for worker pick");
          Debug.Assert(SelectedResource == null, "start worker pick with resource already selected!");

          var resarr = MainPlayer.ExtraWorkers.Where(x => !x.IsCheckedOut).Select(x => x.CostRes).Distinct().ToArray();
          foreach (var rname in resarr) ResChoices.Add(new Res(rname));
          ShowResChoices = true; Message = "pick worker type";

          res = await MakeSureUserPicksAResource();
          ShowResChoices = false; ResChoices.Clear(); SelectedResource = null;

          if (res.Name == wfree2.CostRes) wfree1 = wfree2;
        }
        Message = MainPlayer.Name + " picked " + wfree1.CostRes.ToCapital() + " worker";
        MainPlayer.CheckOutWorker(wfree1);
      }
      else
      {
        MainPlayer.UpdateResBy(res.Name, Number);
      }
      return res;
    }

    public void OnClickResourceInMultiPicker(Res res) { res.Num = (res.Num + 1) % (MainPlayer.Res.n(res.Name) + 1); }
    async Task MakeSureUserPicksExactlyTotalOfResources(int total)
    {
      int picked = 0;
      while (picked != total)
      {
        await Task.Delay(shortDelay);
        await WaitForButtonClick();

        // how do I bind to a list of resources two way?!?!?!?!?!?!?!?!?!!!!!!!!
        var reslist = ResChoices;
        picked = reslist.Sum(x => x.Num);
        if (picked != total) { Message = "PICK THE AMOUNT SPECIFIED: " + total + "!"; foreach (var r in ResChoices) r.Num = 0; }
      }
    }
    async Task<IEnumerable<Res>> WaitForPickMultiResourceCompleted(IEnumerable<Res> reslist, int total, string forWhat)
    {//returns list of resources with number selected by player
      //reslist is copied so can pass player resource list as param
      Debug.Assert(ResChoices != null && ResChoices.Count == 0, "ResChoices not cleared for growth resource pick");
      //Debug.Assert(SelectedResource == null, "start resource pick with resource already selected!");

      // can pay at most MainPlayer.Res.n(resname) of each resource
      ResChoices.Clear();
      foreach (var res in reslist) ResChoices.Add(new Res(res.Name, 0));
      ShowMultiResChoices = true; Caption = "Pick"; Message = MainPlayer.Name + ", pick " + total + " resources for " + forWhat;

      await MakeSureUserPicksExactlyTotalOfResources(total);

      ShowMultiResChoices = false;
      var result = ResChoices.ToList(); ResChoices.Clear();
      Message = MainPlayer.Name + " picked " + total + " resource";

      return result;
    }


    #endregion

    #region helpers

    public void RandomPlayerOrder()
    {
      var plarr = Players.OrderBy(x => Rand.N()).ToArray();
      Players.Clear();
      foreach (var pl in plarr) Players.Add(pl);
      for (int i = 0; i < Players.Count; i++) Players[i].Index = i;
      MainPlayer = Players[0];
    }

    public IEnumerable<Field> GetPossiblePlacesForType(string type)
    {
      Console.WriteLine(MainPlayer.Name);
      return MainPlayer.Civ.Fields.Where(x => x.TypesAllowed.Contains(type)).ToArray();
    }
    bool CalculateCanBuy(Field field)
    {
      var card = field.Card;
      var canbuy = false;
      if (card != null)
      {
        card.Cost = 3 - field.Row; //ignores special rules
        canbuy = card.Cost <= MainPlayer.Res.n("gold");
        var type = card.Type;
        if (type == "battle")
        {
          var milcards = MainPlayer.Cards.Where(x => x.Type == "military").ToArray();
          var havemil = milcards.Any(x => x.NumDeployed > 0);
          canbuy = canbuy && MainPlayer.Cards.Where(x => x.Type == "military").Any(x => x.NumDeployed > 0);
        }
        if (type == "war") canbuy = canbuy && !Stats.IsWar;
        if (type == "colony") canbuy = canbuy && MainPlayer.Res.n("military") >= card.X.aint("milmin");
      }
      return canbuy;
    }
    public int CalcArchitectCost(Card card)
    {
      Debug.Assert(card != null, "CalcArchitectCost for null card called!");
      var costs = card.GetArchCostArray;
      var idx = card.NumDeployed;
      Debug.Assert(idx < costs.Length, "CalcArchitectCost for wonder that was already ready!!!");
      return costs[idx]; // idx<costs.Length?costs[idx]:100;
    }
    public bool ArchitectAvailable { get { return (Stats.Architects > 0 || MainPlayer.HasPrivateArchitect); } }
    public int NumArchitects(Card card) { return card.GetArchCostArray.Length; }
    public bool CalcCanAffordArchitect() { return MainPlayer.Res.n("coal") >= CalcArchitectCost(MainPlayer.WICField.Card); }
    public bool CanDeploy { get { return MainPlayer.Res.n("coal") > Stats.Age; } } //simplified

    void UpdateUI() { DisableAndUnselectAll(); EnableAndSelectForContext(Context.Id); Message = Step != null ? EditMessage(Step.Message) : Context.BaseMessage; }
    void EnableAndSelectForContext(ctx context)
    {
      MarkAllPlayerChoicesAccordingToContext(context);
      foreach (var st in Steps) Select(st.Click, st.Obj);
    }
    void Select(cl click, object obj = null)
    {
      if (obj is Field) { (obj as Field).Card.IsSelected = true; }
      else if (click == cl.worker) { (obj as Res).IsSelected = WorkerSelected = true; }
      else if (click == cl.arch) { ArchitectSelected = true; }
      else if (click == cl.turm) { TurmoilSelected = true; }
    }
    string EditMessage(string text)
    {
      var obj0 = Steps.First().Obj;
      var obj1 = Steps.Count > 1 ? Steps[1].Obj : null;
      if (obj0 != null) text = text.Replace("$0", obj0.ToString().StringBefore("("));
      if (obj1 != null) text = text.Replace("$1", obj1.ToString().StringBefore("("));
      text = text.Replace("_", " ").Replace("$Player", MainPlayer.Name);
      return text;
    }
    IEnumerable<Step> EraseAllStepsBeforeAndIncludingObject(object obj)
    {
      var result = Steps.TakeWhile(x => x.Obj != obj).ToList();
      var erased = Steps.SkipWhile(x => x.Obj != obj).ToList();
      foreach (var st in erased) st.UndoProcess?.Invoke();
      return result;
    }
    bool IsOneStepBuy(Field field)
    {
      var card = field.Card;
      var possible = GetPossiblePlacesForType(card.Type).ToArray();
      var isciv = card.civ();
      return !isciv || possible.Length == 1;
    }

    public void DisableAndUnselectAll()
    {
      TurmoilSelected = ArchitectSelected = WorkerSelected = false;
      ArchitectEnabled = TurmoilEnabled = WorkerEnabled = false;
      foreach (var f in Progress.Fields) if (f.Card != null) { f.Card.IsEnabled = f.Card.IsSelected = false; }
      foreach (var f in MainPlayer.Civ.Fields) if (f.Card != null) { f.Card.IsEnabled = f.Card.IsSelected = false; }
    }

    public event PropertyChangedEventHandler PropertyChanged; public void NotifyPropertyChanged([CallerMemberName] string propertyName = null) { this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }

    #endregion



  }
}
