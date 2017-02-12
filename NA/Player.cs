using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;
namespace ations
{
  public class Player : INotifyPropertyChanged
  {
    #region properties
    public Brush Brush { get; set; }
    public String Name { get; set; }
    public int Level { get; set; }
    public bool IsAI { get { return false; } }// not used

    public bool ShowScore { get { return showScore; } set { if (showScore != value) { showScore = value; NotifyPropertyChanged(); } } }
    bool showScore;
    public double Score { get { return score; } set { if (score != value) { score = value; NotifyPropertyChanged(); } } }
    double score = 0;
    public string InfoSecondary { get { return infoSecondary; } set { infoSecondary = value; NotifyPropertyChanged(); } } //not used
    string infoSecondary;

    public Civ Civ { get { return civ; } set { civ = value; NotifyPropertyChanged(); } }
    Civ civ;
    public bool HasExtraAdvisor { get { return ExtraAdvisor != null; } }
    public Card ExtraAdvisor
    {
      get { return extraAdvisor; }
      set
      {
        if (extraAdvisor == null && value != null || extraAdvisor != null && extraAdvisor.Name != value.Name)
        {
          if (extraAdvisor != null) Cards.Remove(extraAdvisor);
          extraAdvisor = value;
          Debug.Assert(Cards.Contains(value), "setting ExtraAdvisor to card not contained in Cards!!!");
          NotifyPropertyChanged();
          NotifyPropertyChanged("HasExtraAdvisor");
        }
      }
    }
    Card extraAdvisor = null;

    //public List<Card> Cards { get { return Civ.Fields.Where(x=>x!=WICField).Select(x => x.Card).ToList(); } } //foreach field there is a card, either real or empty!
    public ObservableCollection<Card> Cards { get; set; }//nur die ausser WIC { return Civ.Fields.Where(x => x != WICField).Select(x => x.Card).ToList(); } } //foreach field there is a card, either real or empty!
    public Card WIC { get { return HasWIC ? WICField.Card : null; } }
    public IEnumerable<Card> BMCards { get { return Cards.Where(x => x.buildmil()); } }
    public IEnumerable<Field> NonEmptyBMFields { get { return Civ.Fields.Where(x => !x.IsEmpty && x.Card.buildmil()).ToList(); } }

    public ResDict Res { get; set; }
    public ObservableCollection<Choice> SpecialOptions { get; set; }
    public ObservableCollection<Worker> ExtraWorkers { get; set; }//cost res, cost count, margin, is checked out
    public int GratisExtraWorkers { get; set; }

    // clear every round (or less)
    public Dictionary<string, int> Defaulted { get; set; } //to record by how much defaulted per resource in current round
    public bool HasPassed { get { return hasPassed; } set { if (hasPassed != value) { hasPassed = value; NotifyPropertyChanged(); } } }
    bool hasPassed;
    public List<Res> RoundResEffects = new List<Res>();// { get; set; }
    public List<Card> RoundCardEffects = new List<Card>();// { get; set; }
    public List<Card> CardsBoughtThisRound = new List<Card>();
    public List<Res> WarLoss { get; set; }
    public int TurmoilsTaken { get; set; }
    public int RoundsToWaitForNaturalWonder { get; set; }
    public Res GrowthResourcePicked { get; set; }

    public int Index { get; set; }
    public bool IsMainPlayer { get { return isMainPlayer; } set { isMainPlayer = value; NotifyPropertyChanged(); } }
    bool isMainPlayer;
    public int NumActions { get { return numActions; } set { if (numActions != value) { numActions = value; NotifyPropertyChanged(); } } }
    int numActions;
    public bool PassedFirst(Game game) { return this == game.PassOrder.First(); } 
    public bool PassedLast(Game game) { return game.PassOrder.Count == game.NumPlayers && this == game.PassOrder.Last();  }
    public bool IsBroke { get; set; }

    public List<Field> Colonies { get { return Civ.Fields.Where(x => !x.IsEmpty && x.Card.colony()).ToList(); } }
    public List<Field> Buildings { get { return Civ.Fields.Where(x => !x.IsEmpty && x.Card.build()).ToList(); } }
    public List<Field> MilitaryFields { get { return Civ.Fields.Where(x => !x.IsEmpty && x.Card.mil()).ToList(); } }
    public List<Field> Advisors { get { return Civ.Fields.Where(x => !x.IsEmpty && x.Card.adv()).ToList(); } }
    public List<Field> Wonders { get { return Civ.Fields.Where(x => !x.IsEmpty && x.Card.wonder()).ToList(); } }
    public List<Field> Naturals { get { return Civ.Fields.Where(x => !x.IsEmpty && x.Card.natural()).ToList(); } }
    public int NumColonies { get { return Cards.Count(x => x.colony()); } }
    public int NumAdvisors { get { return Cards.Count(x => x.adv()); } }
    public int NumBuildings { get { return Cards.Count(x => x.build()); } }
    public int NumMilitaryFields { get { return Cards.Count(x => x.mil()); } }
    public int NumWonders { get { return Cards.Count(x => x.wonder()); } }
    public int NumNaturals { get { return Cards.Count(x => x.natural()); } }
    public int NumWorkers { get { return Res.n("worker") + Cards.Where(x => x.buildmil()).Sum(x=>x.NumDeployed); } }
    public int NumMilitaryDeployed { get { return Cards.Where(x => x.mil()).Sum(x => x.NumDeployed); } }
    public bool HasColony { get { return Cards.Any(x => x.colony()); } }
    public bool HasAdvisor { get { return Cards.Any(x => x.adv()); } }
    public bool HasRemovableAdvisor { get { return Cards.Any(x => x.adv() && x.Name!="emperor"); } }
    public bool HasExtraworkers { get { return ExtraWorkers.Any(x => !x.IsCheckedOut); } }
    public bool HasWIC { get { return !WICField.IsEmpty; } }
    public bool HasNaturalWIC { get { return RoundsToWaitForNaturalWonder > 0; } }
    public bool HasMilitaryDeployed { get { return Cards.Any(x => x.mil() && x.NumDeployed > 0); } }
    public bool HasBuildingDeployed { get { return Cards.Any(x => x.build() && x.NumDeployed > 0); } }
    public bool HasBMDeployed { get { return Cards.Any(x => x.buildmil() && x.NumDeployed > 0); } }
    public bool Defeated(Game game) { return Military < game.Stats.WarLevel; } 
    public Field WICField { get { return Civ.Fields[CardType.iWIC]; } }
    public Field ADVField { get { return Civ.Fields[0]; } }
    public Field DYNField { get { return Civ.Fields[CardType.iDYN]; } }
    // Res.n("raid"); } set { if (Res.n("raid") != value) { Res.set("raid", value); NotifyPropertyChanged(); } } }
    ////    public int MilminBonus { get; set; }
    //  public int MilitaryDeployBonus { get; set; }
    //  public int BuildingDeployBonus { get; set; }
    //public List<Tuple<Card, Res>> CardEffects { get; set; }

    #region Military, Stability, and Books (resource + positioning on stats board), LevelPosition
    public Point LevelPosition { get; set; }
    static int[] LevelOffsetX = { 20, 50, 20, 50 };
    static int[] LevelOffsetY = { 20, 80, 140, 200 };
    static int LevelIncrementX = 15;

    public int Stability
    {
      get { return stability; }
      set
      {
        if (stability != -1 && stability != Res.n("stability"))
        {
          int n = 3;
        }
        //Debug.Assert(stability == -1 || stability == Res.n("stability"), "Stability inconsistency!!!!!");
        if (stability != value)// || Res.n("stability") != value)
        {
          stability = value;
          Res.set("stability", value);
          var x = StabilityOffsetX + StabilityIncrementX * Index;
          var staby = stability > 15 ? 15 : stability < -4 ? -4 : stability;
          var y = staby >= 0 ? StabilityOffsetY - staby * StabilityIncrementY
            : StabilityOffsetYNegative - (staby * StabilityIncrementY);

          StabilityPosition = new Point(x, y);
          NotifyPropertyChanged("StabilityPosition");
          NotifyPropertyChanged();
        }
      }
    }
    int stability = -1; //trigger update at start if value 0
    public Point StabilityPosition { get; set; }
    static int StabilityOffsetX = 20;
    static int StabilityOffsetY = 855;
    static int StabilityOffsetYNegative = 935;
    static int StabilityIncrementY = 56;
    static int StabilityIncrementX = 7;

    public int Military
    {
      get { return military; }
      set
      {
        Debug.Assert(military == -1 || military == Res.n("military"), "Military inconsistency!!!!!");
        if (military != value)// || Res.n("military") != value)
        {
          military = value;
          Res.set("military", value);
          var xoffset = military <= 20 ? MilitaryOffsetXUpTo20 : MilitaryOffsetX20To40;
          var x = xoffset + MilitaryIncrementX * Index;
          var mily = military > 40 ? 40 : military;
          var y = mily <= 20 ? MilitaryOffsetY - mily * MilitaryIncrementY
            : MilitaryOffsetY - (41 - mily) * MilitaryIncrementY;

          MilitaryPosition = new Point(x, y);
          //MilitaryPosition = new Point(20 + MilitaryIncrementX * Index, MilitaryOffsetY);
          NotifyPropertyChanged("MilitaryPosition");
          NotifyPropertyChanged();
        }
      }
    }
    int military = -1; //trigger update at start if value 0
    public Point MilitaryPosition { get; set; }
    static int MilitaryOffsetXUpTo20 = 14;
    static int MilitaryOffsetX20To40 = 80;
    static int MilitaryOffsetY = 1120;
    static int MilitaryIncrementY = 54;
    static int MilitaryIncrementX = 4;

    public int Books
    {
      get { return books; }
      set
      {
        Debug.Assert(books == -1 || books == Res.n("book"), "Books inconsistency!!!!!");
        if (books != value)// || Res.n("book") != value)
        {
          books = value;
          Res.set("book", value);
          var b = value % 50;
          var x = BooksOffsetX0;
          x = b < 10 ? x + b * BooksX + 4 * Index //ok
            : b >= 10 && b <= 25 ? x + 10 * BooksX//ok
            : b > 25 && b < 35 ? x + (35 - b) * BooksX + 4 * Index // ok
            : x;
          var y = BooksOffsetY0;
          y = b <= 10 ? y //x + b * BooksX + 4 * Index //ok
            : b >= 11 && b <= 25 ? y + (b - 10) * BooksY + 4 * Index//ok
            : b > 25 && b <= 35 ? y + 15 * BooksY // ok
            : y + (50 - b) * BooksY + 4 * Index;

          BooksPosition = new Point(x, y);
          NotifyPropertyChanged("BooksPosition");

          NotifyPropertyChanged();
        }
      }
    }
    int books = -1; //trigger update at start if value 0
    public Point BooksPosition { get; set; }
    static int BooksOffsetX0 = 27;
    static int BooksOffsetY0 = 25;
    static int BooksX = 90;
    static int BooksY = 90;
    #endregion
    #endregion

    #region code
    public void Reset(bool initCiv = true, string civ = null) //TODO:  consolidate mit Gameloop round start actions!
    {//if initciv=false, player will NOT have a civ!
      //IsMainPlayer = false;
      HasPassed = IsBroke = false;
      RoundsToWaitForNaturalWonder = NumActions = GratisExtraWorkers = TurmoilsTaken = 0;
      Military = Books = Stability = 0;
      Res.Clear();
      Cards.Clear();
      WarLoss.Clear();
      CardsBoughtThisRound.Clear();
      RoundResEffects.Clear();
      RoundCardEffects.Clear();
      Defaulted.Clear();
      SpecialOptions.Clear();
      if (initCiv) InitCiv(civ ?? this.Civ.Name);
      Score = 0;
    }
    public void InitCiv(string civ)
    {
      ExtraWorkers.Clear();
      Civ = new Civ(civ);
      Civ.InitCards();
      foreach (var f in Civ.Fields) if (!f.IsEmpty && f != WICField) Cards.Add(f.Card);
      foreach (var r in Civ.GetResources().List) Res.set(r.Name, r.Num);
      foreach (var w in Civ.GetExtraWorkers()) ExtraWorkers.Add(w);
    }
    public Player() { }
    public Player(string name, string civ, Brush brush, int level, int index)
    {
      WarLoss = new List<ations.Res>();
      CardsBoughtThisRound = new List<ations.Card>();
      Cards = new ObservableCollection<Card>();
      RoundResEffects = new List<ations.Res>();
      RoundCardEffects = new List<Card>();
      SpecialOptions = new ObservableCollection<Choice>();
      Defaulted = new Dictionary<string, int>();
      Res = new ations.ResDict();
      ExtraWorkers = new ObservableCollection<ations.Worker>();

      Index = index; //index in player order
      Level = level % LevelOffsetX.Length;
      LevelPosition = new Point(LevelOffsetX[Level] + LevelIncrementX * Index, LevelOffsetY[Level]);
      Name = name;
      Brush = brush;

      InitCiv(civ);
      Books = Stability = Military = 0;
    }

    //public void LoadXml(XElement xpl)
    //{
    //  Name = xpl.astring("name");
    //  Index = xpl.aint("index");
    //  Level = xpl.aint("level");
    //  HasPassed = xpl.abool("hasPassed");
    //  var xreslist = xpl.Element("resources").Attributes();
    //  foreach (var xattr in xreslist)
    //  {
    //    var resname = xattr.Name.ToString();
    //    var resnum = xpl.Element("resources").aint(resname);
    //    Res.set(resname, resnum);
    //  }

    //  var xciv = xpl.Element("civboard");
    //  var civname = xciv.astring("civ");
    //  Civ = new Civ(civname);
    //  Cards.Clear();
    //  int i = 0;
    //  foreach (var xattr in xciv.Attributes())
    //  { // attributes stored in order - important!!!!!!
    //    var aname = xattr.Name.ToString();
    //    if (aname == "civ") continue;
    //    var ndeployed = xciv.aint(aname);
    //    // name is card name or empty+index, val is numdeployed
    //    var f = Civ.Fields[i];
    //    var card = aname.StartsWith("empty") ? Card.MakeEmptyCard(f) : Card.MakeCard(aname);
    //    card.NumDeployed = ndeployed;
    //    f.Card = card;
    //    if (!f.IsEmpty) Cards.Add(card);
    //  }

    //  var workerx = xpl.Element("extraworkers");
    //  GratisExtraWorkers = workerx.aint("gratis");
    //  for (int k = 0; k < workerx.Elements().Count(); k++) { ExtraWorkers[k].IsCheckedOut = workerx.Elements().ElementAt(k).abool("checkedOut"); }

    //  CalcStabAndMil(this);
    //}
    public XElement ToXml()
    {
      var plx = new XElement("player",
        new XAttribute("name", Name),
        new XAttribute("index", Index),
        new XAttribute("level", Level),
        new XAttribute("hasPassed", HasPassed)
        );
      var resx = new XElement("resources");
      foreach (var res in Res.List) { resx.Add(new XAttribute(res.Name, res.Num)); }
      plx.Add(resx);

      var civx = new XElement("civboard", new XAttribute("civ", Civ.Name));
      foreach (var f in Civ.Fields) civx.Add(new XAttribute(f.IsEmpty ? "empty" + f.Index : f.Card.Name, f.Card.NumDeployed));
      plx.Add(civx);

      var workerx = new XElement("extraworkers", new XAttribute("gratis", GratisExtraWorkers));
      foreach (var w in ExtraWorkers)
      {
        workerx.Add(new XElement("worker",
         new XAttribute("res", w.CostRes),
         new XAttribute("n", w.Cost),
         new XAttribute("checkedOut", w.IsCheckedOut)));
      }
      plx.Add(workerx);

      return plx;
    }

    public IEnumerable<Field> GetPossiblePlacesForType(string type)
    {
      //Console.WriteLine(MainPlayer.Name);
      var result = Civ.Fields.Where(x => x.TypesAllowed.Contains(type)).ToArray();
      return result;
    }
    public Field GetFieldOf(string cardname) { return Civ.Fields.FirstOrDefault(x => !x.IsEmpty && x.Card.Name == cardname); }
    public Card GetCard(string cardname) { return Cards.FirstOrDefault(x => x.Name == cardname); }
    public bool HasCard(string cardname) { return Cards.Any(x => x.Name == cardname); }
    public bool HasCardOfType(string type) { return Cards.Any(x => x.Type == type); }
    public int UpdateResBy(string resname, int inc) // to pay with default, use Pay instead!
    {
      if (inc == 0) return Res.n(resname);
      Debug.Assert(resname != "worker" || Res.n("worker") + inc >= 0, "UpdateResBy: neg #workers!!! ");

      if (resname == "stability") { Stability += inc; return Stability; }

      var newcount = Math.Max(0, Res.n(resname) + inc);
      if (resname == "book") Books = newcount;
      else if (resname == "military") Military = newcount;
      else Res.set(resname, newcount);
      return newcount;
    }
    public int Pay(int cost, string resname = "gold")
    { //debt in books

      var rescount = Res.n(resname);

      UpdateResBy(resname, -cost);

      if (rescount >= cost) return 0;

      else
      {
        var diff = cost - rescount;

        if (resname == "vp") return 10 * cost; // player defaults in vp: RULE MOD! can pay vp in 10 books!

        // resname is NOT vp
        if (!Defaulted.ContainsKey(resname))
        {
          Defaulted.Add(resname, diff);
          diff += Pay(1, "vp");
        }
        else Defaulted[resname] += diff;

        if (resname == "book") return diff; // defaults in books: bad!!!

        var result = Pay(diff, "book");
        return result;
      }

    }
    public bool CanPay(int cost, string resname = "gold") { var rescount = Res.n(resname); return rescount >= cost || (resname != "book") && Res.n("vp") >= 1 && rescount + Books >= cost; }
    public void DeployWorker(int num = 1) { UpdateResBy("worker", -num); }
    public void UndeployFrom(Field field, int num = 1) { if (num > 0) { field.Card.NumDeployed -= num; UpdateResBy("worker", num); } }
    public void UndeployWorker(int num = 1) { if (num != 0) UpdateResBy("worker", num); }
    public void CheckOutWorker(Worker w) { w.IsCheckedOut = true; Res.inc("worker", 1); }
    public void ReturnWorker(string resname = "")
    {
      Debug.Assert(Res.n("worker") > 0, "ReturnWorker with no worker undeployed!");
      UpdateResBy("worker", -1);
      if (string.IsNullOrEmpty(resname)) GratisExtraWorkers++;
      else
      {
        var worker = ExtraWorkers.FirstOrDefault(x => x.CostRes == resname && x.IsCheckedOut);
        Debug.Assert(worker != null, "returning worker with non-existing cost resource");
        worker.IsCheckedOut = false;
      }
    }

    //public void UpgradeDynasty(Card card, Field f = null)
    //{
    //  Debug.Assert(Civ.Dynasties.Contains(card), "UpgradeDynasty: card not in Dynasties!");
    //  Civ.Dynasties.Remove(card);
    //  if (f == null) { f = Civ.LargeSizeDynField; }
    //  Checker.AddCivCardSync(this, card, f); //*********** Player.UpgradeDynasty: vielleicht kann man CheckUpgradeDynasty auch hier machen, wen add dynasty card!
    //}
    //public void RemoveAdvisor() { Checker.RemoveCivCard(this, Civ.Fields[CardType.iADV]); }
    //public void WonderReady(Field targetField)
    //{
    //  var card = WIC;
    //  card.NumDeployed = 0;
    //  Checker.AddCivCardSync(this, card, targetField);
    //  Checker.RemoveWIC(this);
    //}

    public Dictionary<string, int> GetResDiff(Dictionary<string, int> resBefore, IEnumerable<string> relevantRes = null)
    {
      var resAfter = GetResourceSnapshot();
      return GetResDiff(resBefore, resAfter, relevantRes);
    }
    public static Dictionary<string, int> GetResDiff(Dictionary<string, int> resBefore, Dictionary<string, int> resAfter, IEnumerable<string> relevantRes = null)
    { // if last param is missing, returns ALL resource changes from before to after
      var result = new Dictionary<string, int>();
      if (relevantRes == null) relevantRes = resAfter.Keys.ToList();
      Debug.Assert(relevantRes.All(x => resAfter.ContainsKey(x)), "GetResDiff: not all relevant resources in resAfter!!!");
      foreach (var resname in relevantRes)
      {
        if (resBefore.ContainsKey(resname)) result.Add(resname, resAfter[resname] - resBefore[resname]);
        else result.Add(resname, resAfter[resname]);
      }
      return result;
    }
    public Dictionary<string, int> GetResourceSnapshot()
    {
      var result = new Dictionary<string, int>();
      foreach (var res in Res.List) result.Add(res.Name, res.Num);
      return result;
    }

    public event PropertyChangedEventHandler PropertyChanged; public void NotifyPropertyChanged([CallerMemberName] string propertyName = null) { this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
    public override string ToString() { return Name; }
    #endregion
  }
}
