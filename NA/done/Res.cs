using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ations
{
  public class Res : INotifyPropertyChanged
  {
    public static List<string> CardAttributes = new List<string> { "name", "type", "age", "exp", "score", "arch", "milmin", "raid", "deploy", "maxdeploy", "allows", "effect" };
    public static List<string> DynamicResources = new List<string> { "raid", "military", "stability" };
    public static List<string> Bonuses = new List<string> { "private_arch" };
    public static List<string> CardTypes = new List<string> { "building", "military", "colony", "wonder", "natural", "advisor", "battle", "golden", "war" };
    public static List<string> RuleKeywords = new List<string> { "effect", "first_turn", "pred", "param", "foreach" };
    public static List<string> CumulativeResources = new List<string> { "vp", "wheat", "gold", "coal", "book" };
    public static List<string> ProductionResources = new List<string> { "wheat", "gold", "coal", "book" };
    public static List<string> WarResources = new List<string> { "wheat", "gold", "coal", "book" }; // same as production
    public static List<string> SpecialResources = new List<string> { "vp", "cross", "worker", "raid" };
    public static List<string> GrowthResources = new List<string> { "wheat", "coal", "gold", "worker" };
    public static List<string> StandardResources = new List<string> { "wheat", "coal", "gold", "vp", "worker" };
    public static List<string> StatsResources = new List<string> { "book", "military", "stability" };
    public static List<string> PayForGoldenAgeVPResources = new List<string> { "wheat", "coal", "gold" };
    public static List<string> PayForDefaultingResources = new List<string> { "wheat", "coal", "gold" }; // same as golden age VP
    public static List<string> PlayerStatResources = new List<string> { "wheat", "coal", "gold", "vp", "worker", "military", "stability", "book" };

    public string Name { get; set; }
    public int Num { get { return num; } set { num = value; NotifyPropertyChanged(); } }
    int num;
    public string Path { get; set; }
    public bool IsSelectable { get { return isSelectable; } set { if (isSelectable != value) { isSelectable = value; NotifyPropertyChanged(); } } }
    bool isSelectable;
    public bool IsSelected { get { return isSelected; } set { if (isSelected != value) { isSelected = value; NotifyPropertyChanged(); } } }
    bool isSelected;

    public bool CanPayWith { get { return PayForGoldenAgeVPResources.Contains(Name); } }
    public bool CanPayDefaultWith { get { return PayForDefaultingResources.Contains(Name); } }
    public bool VP { get { return Name == "vp"; } }

    public Res() { }
    public Res(string name) { Name = name; Path = Helpers.URISTART + "misc/" + name + ".png"; }
    public Res(string name, int num) { Name = name; Num = num; Path = Helpers.URISTART + "misc/" + name + ".png"; }



    public override string ToString() { return Name + " (" + Num + ")"; }
    public event PropertyChangedEventHandler PropertyChanged; public virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null) { this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
  }
}
