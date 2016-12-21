using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ations
{
  public static class CardType
  {
    public static bool adv(this string s) { return s == "advisor"; }
    public static bool adv(this Card c) { return c.Type == "advisor"; }
    public static bool build(this string s) { return s == "building"; }
    public static bool build(this Card c) { return c.Type == "building"; }
    public static bool mil(this string s) { return s == "military"; }
    public static bool mil(this Card c) { return c.Type == "military"; }
    public static bool buildmil(this string s) { return s == "military"||s=="building"; }
    public static bool buildmil(this Card c) { return c.Type == "military"||c.Type =="building"; }
    public static bool wonder(this string s) { return s == "wonder"; }
    public static bool wonder(this Card c) { return c.Type == "wonder"; }
    public static bool colony(this string s) { return s == "colony"; }
    public static bool colony(this Card c) { return c.Type == "colony"; }
    public static bool war(this string s) { return s == "war"; }
    public static bool war(this Card c) { return c.Type == "war"; }
    public static bool battle(this string s) { return s == "battle"; }
    public static bool battle(this Card c) { return c.Type == "battle"; }
    public static bool golden(this string s) { return s == "golden"; }
    public static bool golden(this Card c) { return c.Type == "golden"; }
    public static bool dyn(this string s) { return s == "dynasty"; }
    public static bool dyn(this Card c) { return c.Type == "dynasty"; }
    public static bool civ(this string s) { return !(s.war() || s.golden() || s.battle()); }
    public static bool civ(this Card c) { return !(c.war() ||c.golden()||c.battle()); }
    public static Dictionary<string, Brush> typeColors = new Dictionary<string, Brush>() {
      {"advisor", new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFD7820"))},
      {"building", new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF13A9F0"))},
      {"military", new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF01313"))},
      {"dynasty", new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF7A6565"))},
      {"wonder", new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF703F10"))},
      {"wic", new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF703F10"))}, //can eliminate need to edit xml
      {"colony", new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0E9E3E"))},
      {"golden", Brushes.Gold },
      {"war", Brushes.Black },
      {"battle", Brushes.Gray },
      {"event", Brushes.Indigo },
      {"selected",new SolidColorBrush((Color)ColorConverter.ConvertFromString("#90000000")) },
      {"normal",new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00000000")) },
      {"bright",new SolidColorBrush((Color)ColorConverter.ConvertFromString("#90FFFF00")) },
    };
    public const int WIC = 6;
  }
}
