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
  public partial class Game
  {
    #region constants

    const int numPlayersSetting = 2; // 1 to 5
    const int progressColsSetting = 7; // 1 to 7
    const int roundSetting = 8;
    const bool inclExpDynSetting = true;
    const bool inclExpFakeSetting = false;

    int rounds = 8;
    int longDelay = 400, shortDelay = 100, minAnimationDuration = 1000;
    int iplayer, iround, iphase, iturn, iaction;


    public static string[] PlayerNames = { "Felix", "Amanda", "Taka", "Franzl", "Bertl" };
    public static bool[] PlayerType = { false, true, true, true, true };
    public static string[] PlayerCivs = { "america", "egypt", "arabia", "china", "ethiopia" }; //TODO: choose civs,random civs,daten eingeben!
    public static Brush[] PlayerBrushes = {
        new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0050EF")), //blue
        new SolidColorBrush(Color.FromArgb(255, 96, 169, 23)),//Green
        new SolidColorBrush(Color.FromArgb(255, 250, 104, 0)),//orange
        new SolidColorBrush(Color.FromArgb(255,170, 0, 255)),//violet
        Brushes.Sienna
      };

    public const int MAX_PLAYERS = 5;
    public const int MAX_AGE = 4;
    public const int MAX_ROUNDS = 8;
    public static int[] LevelGrowth = { 4, 3, 2, 1 };

    #endregion
  }
}
