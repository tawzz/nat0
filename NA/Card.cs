using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
namespace ations
{
  public class Card : DependencyObject, INotifyPropertyChanged
  {
    #region properties
    public string Type { get; set; }
    public string Name { get; set; }
    public Brush Brush { get; set; }
    public int Age { get; set; }
    public BitmapImage Image { get; set; }
    public BitmapImage ImageDeployObject { get; set; }
    public XElement X { get; set; }
    public int BasicCost { get; set; } //depends only on row on progress board
    public int Price { get; set; } // includes alterations (bonuses...) and ist used for paying 

    // clear every round
    public List<Res> ListOfResources { get { return listOfResources; } set { if (listOfResources != value) { listOfResources = value; NotifyPropertyChanged(); } } }
    List<Res> listOfResources = null;// new List<Res> { new Res("gold"), new Res("wheat") }; // includes architects on wonder in construction
    public object Tag { get; set; } //just to store some associated info, eg., marked progress card for ethiopia_axumite_kingdom

    //public ObservableCollection<Res> StoredResources { get { return storedResources; } }
    //ObservableCollection<Res> storedResources = new ObservableCollection<Res>();
    public int ActionTaken { get; set; } //counts how many times action was executed this round, reset each round start

    public int NumDeployed { get { return numDeployed; } set { if (numDeployed != value) { numDeployed = value; NotifyPropertyChanged(); } } }
    int numDeployed; // includes architects on wonder in construction
    public bool IsEnabled { get { return isEnabled; } set { if (isEnabled != value) { isEnabled = value; NotifyPropertyChanged(); } } }
    bool isEnabled;
    public bool IsSelected { get { return (bool)GetValue(IsSelectedProperty); } set { SetValue(IsSelectedProperty, value); } }
    public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected", typeof(bool), typeof(Card), null);

    public bool ActionAvailable { get { return HasAction && ActionTaken < GetActionMax; } }
    public bool HasAction { get { return GetAction != null; } }
    public bool CanDeployOn { get { return this.buildmil() && NumDeployed < MaxDeploy; } }
    public int MaxDeploy { get { return X.aint("maxdeploy", 100); } }

    #endregion

    public XElement ToXml()
    {
      var result = new XElement("card",
        new XAttribute("name", Name),
        new XAttribute("age", Age),
        new XAttribute("basicCost", BasicCost),
        new XAttribute("price", Price),
        new XAttribute("numdeployed", NumDeployed),
        new XAttribute("actionTaken", ActionTaken)
        );
      result.Add(X);
      return result;
    }

    public static Card MakeEmptyCard(Field field)
    {
      var card = new Card();
      var type = field.Type;
      card.Type = type; if (type == "military") type = "building";
      card.Brush = CardType.typeColors[card.Type];
      card.X = new XElement("card", new XAttribute("type", type));
      card.Age = 1;
      //card.X = field.X;
      card.Image = Helpers.GetCardImage(card.X);
      field.Card = card;
      return card;
    }
    public static Card MakeProgressCard(XElement xcard, Field field) { var card = MakeCard(xcard); field.Card = card; return card; }
    public static Card MakeEventCard(XElement xcard)
    {
      var card = new Card();
      card.Type = xcard.astring("type", "event");
      card.Brush = CardType.typeColors[card.Type];
      card.Name = xcard.astring("name");
      card.Age = xcard.aint("age", 1);
      card.X = xcard;
      card.Image = Helpers.GetEventCardImage(card.Name, card.Age);
      return card;
    }
    public static Card MakeEventCard(string name)
    {
      var xcard = Helpers.GetEventCardX(name);
      if (xcard != null)
      {
        var card = new Card();
        card.Type = xcard.astring("type", "event");
        card.Brush = CardType.typeColors[card.Type];
        card.Name = xcard.astring("name");
        card.Age = xcard.aint("age", 1);
        card.X = xcard;
        card.Image = Helpers.GetEventCardImage(card.Name, card.Age);
        return card;
      }
      return null;
    }

    public static Card MakeCard(XElement xcard)
    {
      var name = xcard.astring("name");
      var type = xcard.astring("type");
      int age = xcard.aint("age", 1);
      Card card = null;
      Debug.Assert(CardType.typeColors.ContainsKey(type), "MakeCard: typeColors does not contains key " + type);
      card = new Card
      {
        Type = type,
        Brush = CardType.typeColors[type],
        Name = name,
        Age = age,
        X = xcard,
        Image = Helpers.GetCardImage(name),
      };
      card.ImageDeployObject = card.buildmil() ? Helpers.GetMiscImage("worker") :
          card.wonder() ? Helpers.GetMiscImage("architect") : Helpers.GetMiscImage("cross");

      return card;
    }
    public static Card MakeCard(string name = "archimedes")
    {
      return MakeCard(Helpers.GetCardX(name));
    }

    public int[] GetArchCostArray { get { var s = X.astring("arch"); return string.IsNullOrEmpty(s) ? new int[0] : s.Split('_').Select(x => int.Parse(x)).ToArray(); } }
    public int[] GetScoringArray { get { var s = X.astring("score"); return string.IsNullOrEmpty(s) ? new int[0] : s.Split('_').Select(x => int.Parse(x)).ToArray(); } }
    public string GetEffect { get { return X.astring("effect"); } }
    public int GetMilitary { get { return X.aint("military", 0); } }
    public int GetMilmin { get { return X.aint("milmin", 0); } }
    public int GetVP { get { return X.aint("vp", 0); } }
    public int GetStability { get { return X.aint("stability", 0); } }
    public int GetRaid { get { return X.aint("raid"); } }
    public int GetDeployCost { get { return X.aint("deploy", -1); } }
    public int GetTurns { get { return X.aint("turns"); } }
    public XElement GetAction { get { return X.Elements("a").FirstOrDefault(); } }
    public List<XElement> GetProdClauses { get { return X.Elements("prod").ToList(); } }
    public int GetActionMax { get { return HasAction ? GetAction.aint("max", 1) : 0; } }

    public IEnumerable<Res> GetResources(IEnumerable<string> names = null)
    {
      List<Res> result = new List<Res>();
      var reslist = names != null ? X.Attributes().Where(x => names.Contains(x.Name.ToString())).ToList()
        : X.Attributes().Where(x => !Res.CardAttributes.Contains(x.Name.ToString())).ToList();
      foreach (var attr in reslist)
      {
        var name = attr.Name.ToString();
        int n = 0; var ok = int.TryParse(attr.Value, out n);
        Debug.Assert(ok, "GetResourceTuples: Unparsable attribute value for resource " + name + " for card " + this.Name);
        result.Add(new Res(name, n));
      }
      return result;
    }
    public IEnumerable<Res> GetDynamicResources()
    {
      List<Res> result = new List<Res>();
      var reslist = X.Attributes().Where(x => x.Name.ToString().EndsWith("bonus") || x.Name.ToString().StartsWith("cost") || Res.DynamicResources.Contains(x.Name.ToString())).ToList();
      foreach (var attr in reslist)
      {
        var name = attr.Name.ToString();
        int n = 0;
        var ok = int.TryParse(attr.Value, out n);
        Debug.Assert(ok, "GetResourceTuples: Unparsable attribute value for resource " + name + " for card " + this.Name);
        result.Add(new Res(name, n));
      }
      return result;
    }
    //public IEnumerable<Res> GetResources()
    //{

    //  //List<string> exceptions = new List<string> { "first_turn", "vp", "raid", "military", "stability", "score", "name", "type", "age",
    //  //  "private_arch", "maxdeploy", "deploy", "effect", "n", "milmin", "arch", "exp", "cross" };
    //  List<Res> result = new List<Res>();
    //  var reslist = X.Attributes().Where(x => !Res.DynamicResources.Contains(x.Name.ToString()) && !Res.CardAttributes.Contains(x.Name.ToString())).ToList();
    //  foreach (var attr in reslist)
    //  {
    //    var name = attr.Name.ToString();
    //    if (name.EndsWith("bonus") || name.StartsWith("cost")) continue;
    //    int n = 0;
    //    var ok = int.TryParse(attr.Value, out n);
    //    Debug.Assert(ok, "GetResourceTuples: Unparsable attribute value for resource " + name + " for card " + this.Name);
    //    result.Add(new Res(name, n));
    //  }
    //  return result;
    //}
    public IEnumerable<string> GetResourceNames() { return GetResources().Select(x => x.Name).ToList(); }

    public event PropertyChangedEventHandler PropertyChanged; public void NotifyPropertyChanged([CallerMemberName] string propertyName = null) { this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
    public override string ToString() { return Name + "(" + Type + ")"; }


  }
}
