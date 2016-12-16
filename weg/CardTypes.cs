using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ations
{
  public static class CardTypes
  {
    public static Dictionary<string, Brush> typeColors = new Dictionary<string, Brush>() {
      {"advisor", new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFD7820"))},
      {"building", new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF13A9F0"))},
      {"military", new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF01313"))},
      {"dynasty", new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF7A6565"))},
      {"wonder", new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF703F10"))},
      {"wic", new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF703F10"))}, //can eliminate need to edit xml
      {"colony", new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0E9E3E"))},
      {"golden", Brushes.Yellow },
      {"war", Brushes.Black },
      {"battle", Brushes.Gray },
      {"event", Brushes.Indigo },
      {"selected",new SolidColorBrush((Color)ColorConverter.ConvertFromString("#90000000")) },
      {"normal",new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00000000")) },
      {"bright",new SolidColorBrush((Color)ColorConverter.ConvertFromString("#90FFFF00")) },
    };
    public const int WIC = 6;

  }
  public enum TCard { A, B, M, D, W, C, G, X, T, E } // battle=T, war=X, weiss noch nicht ob noetig
  public enum TBoard { Stats, Progress, Civ }// not needed
}
