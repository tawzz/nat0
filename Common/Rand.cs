using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ations
{
  public static class Rand
  {
    static Random rand = new Random(Environment.TickCount);

    public static bool Flip(double percentYes = 50) { return rand.Next(0, 101) <= percentYes; }

    public static DateTime Time() { return DateTime.Now.AddMinutes(rand.Next(0, 1440)); }

    public static int N(int min = 0, int max = 100) { return rand.Next(min, max + 1); }

    public static int Even(int min = 0, int max = 100) { var num = N(min, max); return num % 2 == 1 ? num == max ? num - 1 : num + 1 : num; }

    public static int Odd(int min = 0, int max = 100) { var num = N(min, max); return num % 2 == 0 ? num == max ? num - 1 : num + 1 : num; }

    public static double Double(double min, double max) { return min + (float)rand.NextDouble() * (max - min); }

    public static float Float(float min, float max) { return min + (float)rand.NextDouble() * (max - min); }

    public static SolidColorBrush Brush() { return new SolidColorBrush(Color()); }

    public static Color Color() { return System.Windows.Media.Color.FromArgb(255, (byte)rand.Next(0, 255), (byte)rand.Next(0, 255), (byte)rand.Next(0, 255)); }

    public static void SetRandomBackground(this UIElement ui)
    {
      if (ui is Panel)
        (ui as Panel).Background = Rand.Brush();
      else if (ui is Control)
        (ui as Control).Background = Rand.Brush();
      else if (ui is TextBox)
        (ui as TextBox).Background = Rand.Brush();
    }

    public static int[] Numbers(int n, int min, int max, bool duplicateAllowed = false)
    {
      List<int> intlist = new List<int>();
      while (intlist.Count < n)
      {
        var inew = N(min, max);
        if (intlist.Contains(inew)) continue; else intlist.Add(inew);
      }
      return intlist.ToArray();
    }
  }
}
