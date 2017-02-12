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
using System.Xml.Serialization;
using ations;
namespace ations
{
  //#region design time
  //public class AGameDesignTime { public Game Game { get; set; } public AGameDesignTime() { Game = new Game(); } }// GameInst; } }
  //#endregion
  public partial class Game : DependencyObject, INotifyPropertyChanged, IXmlSerializable
  {
    //#region  constructor: singleton
    //private static readonly Game instance = new Game(); public static Game Inst { get { return instance; } }
    //private Game()
    public Game()
    {
      Checker = new Checker(this);
      Tests = new Tests(this);

      PreselectedFields = new ObservableCollection<ations.Field>();
      ResChoices = new ObservableCollection<Res>();
      ChangesInResources = new ObservableCollection<Res>();
      Choices = new ObservableCollection<Choice>();
      AnimationQueue = new List<Storyboard>();
      ContextStack = new Stack<ContextInfo>();

      Players = new ObservableCollection<Player>();
      Helpers.MakeCardIndex();
      //State = new State();

      Initialize();
    }

    public void ResetGame()
    {
      PreselectedFields.Clear();
      ResChoices.Clear();
      Choices.Clear();
      ChangesInResources.Clear();
      PassOrder = new List<Player>();
      Description = Message = LongMessage = ""; Caption = "Start"; LeftCaption = "Cancel"; RightCaption = "Pass";
      ShowScore = ShowChoices = ShowResChoices = ShowChangesInResources = ShowMultiResChoices = false;
      Number = 0;
      ClearAnimations();
      Stats.Clear();
      ArchitectEnabled = TurmoilEnabled = WorkerEnabled = IsOkStartEnabled = IsPassEnabled = IsCancelEnabled = false;
      OkStartClicked = CancelClicked = PassClicked = ArchitectSelected = TurmoilSelected = WorkerSelected = IsCardActivation = IsSpecialOptionActivation = false;
      ActionComplete = false;
      Context = null;
      ContextStack.Clear();
    }
    public void ClearAnimations()
    {
      if (AnimationQueue != null) AnimationQueue.Clear();
      if (ResourcesUpdatedThisAnimationCycle != null) ResourcesUpdatedThisAnimationCycle.Clear();
    }
    public void InitPlayers(int num)
    {
      Debug.Assert(Players != null, "InitPlayers with Players==null");
      NumPlayers = num;
      Players.Clear();
      for (int i = 0; i < NumPlayers; i++)
        Players.Add(PlayerType[i] ? new AI(PlayerNames[i], PlayerCivs[i], PlayerBrushes[i], Levels.Chieftain, i) : new Player(PlayerNames[i], PlayerCivs[i], PlayerBrushes[i], Levels.Chieftain, i));
      MainPlayer = Players[0];
      NotifyPropertyChanged("Players");
      NotifyPropertyChanged("NumPlayers");
      NotifyPropertyChanged("MainPlayer");

    }
    void Initialize(int nPlayers = numPlayersSetting, int nProgressCols = progressColsSetting, int nRounds = roundSetting, bool inclDyn = inclExpDynSetting, bool inclFake = inclExpFakeSetting)
    {
      // alle default values
      // do not touch UIRoundMarker!      //SelectedChoice = null;      //SelectedResource = null;
      PreselectedFields.Clear();
      ResChoices.Clear();
      Choices.Clear();
      ChangesInResources.Clear();
      AnimationQueue.Clear();
      PassOrder = new List<Player>();
      Description = Message = LongMessage = ""; Caption = "Start"; LeftCaption = "Cancel"; RightCaption = "Pass"; Title = "";
      Interrupt = IsRunning = CanInitialize = false;
      Kickoff = null;
      ShowScore = ShowChoices = ShowResChoices = ShowChangesInResources = ShowMultiResChoices = false;
      Number = 0;
      //if (NumEach != null) NumEach.Clear();
      if (ResourcesUpdatedThisAnimationCycle != null) ResourcesUpdatedThisAnimationCycle.Clear();
      iplayer = iround = iphase = iturn = iaction = 0;
      rounds = nRounds;

      InitPlayers(nPlayers);
      Progress = new Progress(this,nProgressCols, inclDyn, inclFake);// NumPlayers + 2, true, false); //testing
      Stats = new Stats(this);

      ArchitectEnabled = TurmoilEnabled = WorkerEnabled = IsOkStartEnabled = IsPassEnabled = IsCancelEnabled = false;
      OkStartClicked = CancelClicked = PassClicked = ArchitectSelected = TurmoilSelected = WorkerSelected = IsCardActivation = IsSpecialOptionActivation = false;
      ActionComplete = false;
      Context = null;
      ContextStack.Clear();

      //      Title = "Nations Start!"; Message = "press Start!"; Caption = "Start"; LongMessage = "game initialization complete";
      IsOkStartEnabled = true; Kickoff = GameLoop;//Tester;// testing 

      // TEST SETUP FUER RUNTIME DEFAULT HERE ***************************************
      //switchesOff();
      //SwitchRoundAndAgeOn = SwitchProgressOn = SwitchNewEventOn = SwitchActionOn = SwitchProductionOn = SwitchOrderOn = SwitchWarOn = true;
      Tests.switchesOn();

      // TEST SETUP FUER DESIGNER HERE **********************************************
      //design test
      //var card = P0.GetCard("yurt");
      //card.StoredResources.Add(new Res("gold"));



      //ShowCardChoices = true;
      //var dyn = MainPlayer.Civ.Dynasties.ToList();
      //List<Choice> list = new List<Choice>();
      //foreach (var c in dyn) { var ch = new Choice(); ch.Tag = c; ch.Text = c.Name; list.Add(ch); }
      //list.Add(new Choice { Text = "gold", Path = Helpers.GetImagePath("gold") });
      //foreach (var ch in list) Choices.Add(ch);

    }

    //    #region  constructor
    //    //private static readonly Game instance = new Game(); : singleton

    //    public static Game Inst { get { if (inst == null) inst = new Game(); return inst; } private set { inst = value; } }// { {if ; private set; }// { return instance; } }
    //    static Game inst = null;

    //    public Game()
    //    {
    //      PreselectedFields = new ObservableCollection<Field>();
    //      ResChoices = new ObservableCollection<Res>();
    //      ChangesInResources = new ObservableCollection<Res>();
    //      Choices = new ObservableCollection<Choice>();
    //      AnimationQueue = new List<Storyboard>();
    //      ContextStack = new Stack<ContextInfo>();
    //      Players = new ObservableCollection<Player>();
    //      Helpers.MakeCardIndex();

    //      Initialize();
    ////      NotifyPropertyChanged("Inst");
    //    }

//#endregion

    #region resource selection/update/production, choice selection

    public ObservableCollection<Field> PreselectedFields { get; set; }
    public ObservableCollection<Res> ResChoices { get; set; }
    public ObservableCollection<Choice> Choices { get; set; }
    public ObservableCollection<Res> ChangesInResources { get; set; }
    public List<string> ResourcesUpdatedThisAnimationCycle = new List<string>();
    public Res SelectedResource { get { return (Res)GetValue(SelectedResourceProperty); } set { SetValue(SelectedResourceProperty, value); } }
    public static readonly DependencyProperty SelectedResourceProperty = DependencyProperty.Register("SelectedResource", typeof(Res), typeof(Game), null);
    public Choice SelectedChoice { get { return (Choice)GetValue(SelectedChoiceProperty); } set { SetValue(SelectedChoiceProperty, value); } }
    public static readonly DependencyProperty SelectedChoiceProperty = DependencyProperty.Register("SelectedChoice", typeof(Choice), typeof(Game), null);

    public void ResourceUpdated(FrameworkElement ui)
    {
      if (AnimationQueue.Count == 0) ResourcesUpdatedThisAnimationCycle.Clear();
      var res = ui.DataContext as Res;
      if (ResourcesUpdatedThisAnimationCycle.Contains((ui.DataContext as Res).Name)) return;
      ResourcesUpdatedThisAnimationCycle.Add(res.Name);
      var sb = Storyboards.Scale(ui, TimeSpan.FromSeconds(.3), new Point(1, 1), new Point(5, 2), null, true);
      AnimationQueue.Add(sb);
    }
    public void NumDeployedUpdated(FrameworkElement ui)
    {
      var card = (ui.DataContext as Field).Card;
      if (card == null || card.NumDeployed <= 0) return;
      var sb = Storyboards.Scale(ui, TimeSpan.FromSeconds(.3), new Point(1, 1), new Point(5, 2), null, true);
      AnimationQueue.Add(sb);
    }
    public void OnClickResourceInMultiPicker(Res res) { res.Num = (res.Num + 1) % (MainPlayer.Res.n(res.Name) + 1); }

    #endregion

    #region helpers

    public async Task InterruptGame()
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
        Message = "Interrupt Completed!";
        await Task.Delay(longDelay);
      }
    }
    public void RandomPlayerOrder()
    {
      var plarr = Players.OrderBy(x => Rand.N()).ToArray();
      Players.Clear();
      foreach (var pl in plarr) Players.Add(pl);
      for (int i = 0; i < Players.Count; i++) Players[i].Index = i;
      MainPlayer = Players[0];
    }
    public bool CalculateCanBuy(Field field)
    {
      var card = field.Card;
      if (card == null) return false;

      card.Price = Checker.CalcPrice(card);
      var canbuy = card.Price <= MainPlayer.Res.n(Checker.BuyResources(card));

      var type = card.Type;
      if (type == "battle") canbuy = canbuy && MainPlayer.Cards.Where(x => x.mil()).Any(x => x.NumDeployed > 0);
      else if (type == "war") canbuy = canbuy && !Stats.IsWar;
      else if (type == "colony") canbuy = canbuy && Checker.CheckMilminCondition(MainPlayer, card);
      return canbuy;
    }
    public int CalcArchitectCost(Card card)
    {
      Debug.Assert(card != null, "CalcArchitectCost for null card called!");
      var costs = card.GetArchCostArray;
      var idx = card.NumDeployed;
      Debug.Assert(idx < costs.Length, "CalcArchitectCost for wonder that was already ready!!!");
      var baseCost = costs[idx];
      var realCost = Checker.CheckArchitectCost(MainPlayer, card, baseCost);
      return realCost;
    }
    public bool ArchitectAvailable { get { return (Stats.Architects > 0); } }// || MainPlayer.HasPrivateArchitect); } } //))) WRONG!!!
    public int NumArchitects(Card card) { return card.GetArchCostArray.Length; }
    public bool CalcCanAffordArchitect() { return MainPlayer.Res.n("coal") >= CalcArchitectCost(MainPlayer.WICField.Card); }
    public bool CalcCanAffordArchitect(Player pl) { return pl.Res.n("coal") >= CalcArchitectCost(pl.WICField.Card); }
    public bool CanDeploy { get { return MainPlayer.Cards.Any(x => x.buildmil()) && MainPlayer.Res.n("coal") >= CalcMinDeployCost(MainPlayer); } } //simplified
    public int CalcMinDeployCost(Player pl) { return pl.Cards.Where(x => x.buildmil()).Min(x => Checker.CalcDeployCost(pl, x)); }
    public void UpdateUI() { DisableAndUnselectAll(); EnableAndSelectForContext(Context.Id); Message = Step != null ? EditMessage(Step.Message) : Context.BaseMessage; }
    public void EnableAndSelectForContext(ctx context)
    {
      MarkAllPlayerChoicesAccordingToContext(context);
      foreach (var st in Steps) Select(st.Click, st.Obj);
    }
    public void Select(cl click, object obj = null)
    {
      if (obj is Field) { (obj as Field).Card.IsSelected = true; }
      else if (click == cl.worker) { (obj as Res).IsSelected = WorkerSelected = true; }
      else if (click == cl.arch) { ArchitectSelected = true; }
      else if (click == cl.turm) { TurmoilSelected = true; }
    }
    public string EditMessage(string text)
    {
      var obj0 = Steps.First().Obj;
      var obj1 = Steps.Count > 1 ? Steps[1].Obj : null;
      if (obj0 != null) text = text.Replace("$0", obj0.ToString().StringBefore("("));
      if (obj1 != null) text = text.Replace("$1", obj1.ToString().StringBefore("("));
      text = text.Replace("_", " ").Replace("$Player", MainPlayer.Name);
      return text;
    }
    public IEnumerable<Step> EraseAllStepsBeforeAndIncludingObject(object obj)
    {
      var result = Steps.TakeWhile(x => x.Obj != obj).ToList();
      var erased = Steps.SkipWhile(x => x.Obj != obj).ToList();
      foreach (var st in erased) st.UndoProcess?.Invoke();
      return result;
    }
    public bool IsOneStepBuy(Field field)
    {
      var card = field.Card;
      var possible = MainPlayer.GetPossiblePlacesForType(card.Type).ToArray();
      var isciv = card.civ();
      return !isciv || possible.Length == 1;
    }
    public void DisableAndUnselectAll()
    {
      TurmoilSelected = ArchitectSelected = WorkerSelected = false;
      ArchitectEnabled = TurmoilEnabled = WorkerEnabled = false;
      foreach (var f in Progress.Fields) if (f.Card != null) { f.Card.IsEnabled = f.Card.IsSelected = false; }
      foreach (var f in MainPlayer.Civ.Fields) if (f.Card != null) { f.Card.IsEnabled = f.Card.IsSelected = false; }
      foreach (var ch in MainPlayer.SpecialOptions) { ch.IsSelectable = false; }
    }
    public event PropertyChangedEventHandler PropertyChanged; public void NotifyPropertyChanged([CallerMemberName] string propertyName = null) { this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }

    #endregion

  }
}
