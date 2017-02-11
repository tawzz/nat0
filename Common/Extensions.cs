using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml.Linq;
using System.Windows.Media;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Globalization;

namespace ations
{
  public static class Extensions
  {
    public static IEnumerable<T> Plus<T>(this IEnumerable<T> list, T elem) { var result = list.ToList();result.Add(elem); return result;}
    public static T FindFirstVisual<T>(this DependencyObject obj) where T : DependencyObject
    {
      if (obj is T) return (T)obj;
      else
      {
        T result = null;
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
        {
          var child = VisualTreeHelper.GetChild(obj, i);
          var ochild = child.FindFirstVisual<T>();
          if (ochild != null)
          {
            result = ochild;
            break;
          }
        }
        return result;
      }
    }
    public static T FindAncestor<T>(this DependencyObject obj) where T : DependencyObject
    {
      var tmp = VisualTreeHelper.GetParent(obj);
      while (tmp != null && !(tmp is T))
      {
        tmp = VisualTreeHelper.GetParent(tmp);
      }
      return (T)tmp;
    }
    public static IEnumerable<T> FindVisualChildren<T>(this DependencyObject depObj) where T : DependencyObject
    {
      if (depObj != null)
      {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
        {
          DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
          if (child != null && child is T)
          {
            yield return (T)child;
          }

          foreach (T childOfChild in FindVisualChildren<T>(child))
          {
            yield return childOfChild;
          }
        }
      }
    }
    public static List<int> ParseAllIntegersInString(this string s)
    {
      var result = new List<int>();
      var i = 0;
      int max = s.Length;
      while (i < max)
      {
        while (i < max && !Char.IsDigit(s[i]) && s[i] != '-') i++;
        if (i >= max) break;
        bool isNeg = false;
        if (s[i] == '-') { isNeg = true; i++; }
        int one = 0;
        while (i < max && Char.IsDigit(s[i])) { one = one * 10 + int.Parse(s[i++].ToString()); }
        if (isNeg) one *= -1;
        result.Add(one);
      }
      return result;
    }
    public static string IntsToString(this IEnumerable<int> list)
    {
      string s = "";
      foreach (int i in list) s += i.ToString() + " ";
      return s;
    }
    public static bool IsLike(this string s, string A)
    {
      return s != "" && A != "" && (s == A || s.Contains(A) || A.Contains(s));
    }
    public static bool IsValidFilename(this string s)
    {
      foreach (var ch in s)
        if (!char.IsLetterOrDigit(ch) && ch != '_' && ch != ' ') return false;
      return true;
    }
    public static bool IsValidPhone(this string phone)
    {
      phone = phone.Trim();
      if (phone[0] == '+') phone = phone.Substring(1);
      foreach (var ch in phone)
        if (!Char.IsDigit(ch) && ch != ' ')
          return false;
      return true;
    }
    public static string Remove(this string s, string A)
    {
      return s.Replace(A, "");
    }
    public static string ToCapital(this string s)
    {
      return s != "" ? s[0].ToString().ToUpper() + s.Substring(1) : s; // OJ bleibt so! .ToLower();
    }
    public static string StringAfter(this string s, string A, bool trim = true)
    {
      int from = s.IndexOf(A);
      if (from >= 0) s = s.Substring(from + A.Length);

      return trim ? s.Trim() : s;
    }
    public static string StringAfter(this char ch, string A, bool trim = true)
    {
      return ch.ToString().StringAfter(A, trim);
    }
    public static string f2(this string s) { return s.Substring(0, 2); }
    public static string a2(this string s) { return s.Substring(2); }
    public static string f1(this string s) { return s.Substring(0, 1); }
    public static string a1(this string s) { return s.Substring(1); }
    public static string StringAfterLast(this string s, string A, bool trim = true)
    {
      int from = s.LastIndexOf(A);
      if (from >= 0) s = s.Substring(from + A.Length);

      return trim ? s.Trim() : s;
    }
    public static string StringBefore(this string s, string A, bool trim = true)
    {
      int to = s.IndexOf(A);
      if (to >= 0) s = s.Substring(0, to);

      return trim ? s.Trim() : s;
    }
    public static string StringBeforeLast(this string s, string A, bool trim = true)
    {
      int to = s.LastIndexOf(A);
      if (to >= 0) s = s.Substring(0, to);

      return trim ? s.Trim() : s;
    }
    public static string StringBetween(this string s, string A, string B, bool trim = true)
    {
      int from = s.IndexOf(A);
      if (from >= 0) s = s.Substring(from + A.Length);

      int to = s.IndexOf(B);
      if (to >= 0) s = s.Substring(0, to);

      return trim ? s.Trim() : s;
    }

    public static bool TryLoadXElement(this string s, out XElement x)
    {
      if (s == null || !File.Exists(s))
      {
        x = null;
        return false;
      }

      try { x = XElement.Load(s); return true; }
      catch { x = null; return false; }
    }

    public static Color Hex2Color(this string hex)
    {
      if (hex == "") return Colors.LightGray;

      //remove the # at the front 
      hex = hex.Replace("#", "");
      byte a = 255;
      byte r = 255;
      byte g = 255;
      byte b = 255;

      int start = 0;
      //handle ARGB strings (8 characters long) 
      if (hex.Length == 8)
      {
        a = Byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
        start = 2;
      }
      //convert RGB characters to bytes 
      r = Byte.Parse(hex.Substring(start, 2), NumberStyles.HexNumber);
      g = Byte.Parse(hex.Substring(start + 2, 2), NumberStyles.HexNumber);
      b = Byte.Parse(hex.Substring(start + 4, 2), NumberStyles.HexNumber);
      return Color.FromArgb(a, r, g, b);
    }
    public static BitmapImage LoadImage(this string file)
    {
      BitmapImage bmp = new BitmapImage();
      bmp.BeginInit();
      //bmp.UriSource = new Uri("pack://application:,,,/WpfKillerApp;component/Resources/images/sound2.png"); // DOES NOT WORK

      bmp.UriSource = new Uri("pack://application:,,,/images/" + file); // WPF
      bmp.EndInit();
      //imageSoundOn.Source = bmp;

      return bmp;

    }
    public static BitmapSource LoadTransparentImage(this string file, Color trans)
    {
      var bmp = file.LoadImage();
      var bmp1 = MakePixelsTransparent(trans, bmp);
      return bmp1;



    }


    public static WriteableBitmap MakePixelsTransparent(Color colorKey, BitmapImage img)
    {
      int pixelWidth = (int)img.Width;
      int pixelHeight = (int)img.Height;
      int Stride = pixelWidth * 4;
      BitmapSource imgSource = img; // (BitmapSource)img.Source;
      byte[] pixels = new byte[pixelHeight * Stride];
      imgSource.CopyPixels(pixels, Stride, 0);
      byte TransparentByte = byte.Parse("0");
      byte Byte255 = byte.Parse("255");
      int N = pixelWidth * pixelHeight;
      //Operate the pixels directly
      for (int i = 0; i < N; i++)
      {
        byte a = pixels[i * 4];
        byte b = pixels[i * 4 + 1];
        byte c = pixels[i * 4 + 2];
        byte d = pixels[i * 4 + 3];
        if (a == Byte255 && b == Byte255 && c == Byte255 && d == Byte255)
        {
          pixels[i * 4] = TransparentByte;
          pixels[i * 4 + 1] = TransparentByte;
          pixels[i * 4 + 2] = TransparentByte;
          pixels[i * 4 + 3] = TransparentByte;
        }
      }
      WriteableBitmap writeableBitmap = new WriteableBitmap(pixelWidth, pixelHeight, 96, 96,
          PixelFormats.Pbgra32, BitmapPalettes.Halftone256Transparent);
      writeableBitmap.WritePixels(new Int32Rect(0, 0, pixelWidth, pixelHeight), pixels, Stride, 0);
      return writeableBitmap;
    }

    public static void SetAttribute(this XElement el, string attributeName, object value)
    {
      if (el.Attribute(attributeName) == null)
        el.Add(new XAttribute(attributeName, value.ToString()));
      else
        el.Attribute(attributeName).Value = value.ToString();

    }

    public static void MakePixelsTransparent(Color colorKey, Image img)
    {
      int pixelWidth = (int)img.Width;
      int pixelHeight = (int)img.Height;
      int Stride = pixelWidth * 4;
      BitmapSource imgSource = (BitmapSource)img.Source;
      byte[] pixels = new byte[pixelHeight * Stride];
      imgSource.CopyPixels(pixels, Stride, 0);
      byte TransparentByte = byte.Parse("0");
      byte Byte255 = byte.Parse("255");
      int N = pixelWidth * pixelHeight;
      //Operate the pixels directly
      for (int i = 0; i < N; i++)
      {
        byte a = pixels[i * 4];
        byte b = pixels[i * 4 + 1];
        byte c = pixels[i * 4 + 2];
        byte d = pixels[i * 4 + 3];
        if (a == Byte255 && b == Byte255 && c == Byte255 && d == Byte255)
        {
          pixels[i * 4] = TransparentByte;
          pixels[i * 4 + 1] = TransparentByte;
          pixels[i * 4 + 2] = TransparentByte;
          pixels[i * 4 + 3] = TransparentByte;
        }
      }
      WriteableBitmap writeableBitmap = new WriteableBitmap(pixelWidth, pixelHeight, 96, 96,
          PixelFormats.Pbgra32, BitmapPalettes.Halftone256Transparent);
      writeableBitmap.WritePixels(new Int32Rect(0, 0, pixelWidth, pixelHeight), pixels, Stride, 0);
      img.Source = writeableBitmap;
    }

    public static Point ActualPosition(this UIElement ui, Visual visual)
    {
      GeneralTransform gt = ui.TransformToVisual(visual);
      return gt.Transform(new Point(0, 0));
    }

    public static double ActualLeft(this UIElement ui, Visual visual)
    {
      return ui.ActualPosition(visual).X;
    }
    public static double ActualTop(this UIElement ui, Visual visual)
    {
      return ui.ActualPosition(visual).Y;
    }
    public static double GetWindowLeft(this Window window)
    {
      if (window.WindowState == WindowState.Maximized)
      {
        var leftField = typeof(Window).GetField("_actualLeft", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (double)leftField.GetValue(window);
      }
      else
        return window.Left;
    }
    public static double GetWindowTop(this Window window)
    {
      if (window.WindowState == WindowState.Maximized)
      {
        var topField = typeof(Window).GetField("_actualTop", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (double)topField.GetValue(window);
      }
      else
        return window.Top;
    }

    public static void CopyFileFromResourceTo(this string filename, string directory) // wpf version
    {
      using (var src = Application.GetResourceStream(new Uri(@"/NewEC;component/data/" + filename, UriKind.Relative)).Stream)
      using (var dest = new FileStream(Path.Combine(directory, filename), FileMode.Create, FileAccess.Write))
      {
        src.CopyTo(dest, 32768);
      }
    }
  }
}
