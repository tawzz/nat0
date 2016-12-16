using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using System.Threading.Tasks;
using System.Windows.Resources;

namespace ations
{
  public static class Helpers
  {
    public const string URISTART = "pack://application:,,,/ations;component/assets/";

    public static XElement[] GetCardarrayX(int age) { var xel = GetX("cards/xml/cards0" + age + ".xml"); return xel != null ? xel.Elements().ToArray() : null; }
    public static XElement[] GetEventCardarrayX(int age)
    {
      var xfile = GetX("cards/xml/eventcards.xml");
      var e1 = xfile.Elements().ToArray(); //the 4 ages
      var e2 = e1[age - 1].Elements().ToArray();
      return e2;//.Elements().Elements("age"+age).ToArray();
    }
    public static XElement GetCardX(string cardname, int age)
    {
      var xarr = GetX("cards/xml/cards0" + age + ".xml");
      return xarr != null ? xarr.Elements().FirstOrDefault(x => x.astring("name") == cardname) : null;
    }
    public static XElement GetCommonCardX(string cardname)
    {
      var xarr = GetX("cards/xml/_commoncards.xml");
      return xarr != null ? xarr.Elements().FirstOrDefault(x => x.astring("name") == cardname) : null;
    }
    public static XElement GetCivCardX(string civ, string cardname)
    {
      var xarr = GetX("cards/xml/civcards/" + civ + "cards.xml");
      return xarr != null ? xarr.Elements().FirstOrDefault(x => x.astring("name") == cardname) : null;
    }
    public static XElement GetCivX(string civ) { return GetX("civs/xml/" + civ + ".xml"); }

    public static XElement GetX(string pathAfterAssets)
    {
      //Uri uri = new Uri("/assets/" + pathAfterAssets, UriKind.Relative);
      Uri uri = new Uri(URISTART + pathAfterAssets);
      StreamResourceInfo info = Application.GetResourceStream(uri);
      if (info != null) { XElement x = XElement.Load(info.Stream); return x; }
      return null;
    }

    public static BitmapImage GetProgressboardImage() { return GetImage("boards/progress_board.jpg"); }
    public static BitmapImage GetStatsboardImage() { return GetImage("boards/stats_board.jpg"); }
    public static BitmapImage GetCardImage(string cardname, int age) { return GetImage("cards/age" + age + "/age" + age + "_" + cardname + ".jpg"); }
    public static BitmapImage GetEventCardImage(string cardname, int age) { return GetImage("cards/event/age" + age + "/age" + age + "_" + cardname + ".jpg"); }
    public static BitmapImage GetCivImage(string civ) { return GetImage("civs/" + civ + ".jpg"); }
    public static BitmapImage GetEmptyCardImage(string type) { return GetImage("civs/cards/" + type + ".png"); }
    public static BitmapImage GetCivCardImage(string cardname) { return GetImage("civs/cards/age1_" + cardname + ".jpg"); }
    public static BitmapImage GetDynCardImage(string civ) { return GetImage("civs/cards/" + civ + "_dyn1.jpg"); }

    public static BitmapImage GetImage(string pathAfterAssets)
    {
      BitmapImage bmp = new BitmapImage();
      bmp.BeginInit();
      bmp.UriSource = new Uri(URISTART + pathAfterAssets); //geht!
      bmp.EndInit();
      return bmp;
    }
    public static int aint(this XElement el, string attributeName, int defaultValue = 0)
    {
      var att = el.Attribute(attributeName);
      int result;
      return att != null && Int32.TryParse(att.Value, out result) ? result : defaultValue;
    }
    public static string astring(this XElement el, string attributeName, string defaultValue = "")
    {
      var att = el.Attribute(attributeName);
      return att != null ? att.Value : defaultValue;
    }
    public static bool abool(this XElement el, string attributeName, bool defaultValue = false)
    {
      var att = el.Attribute(attributeName);
      bool result;
      if (att == null || !bool.TryParse(att.Value, out result))
        return defaultValue;
      else
        return result;
    }














    #region obsolete
    //obsolete (nur noch fuer erste version)
    public static Thickness GetMarginOfFieldOnProgressBoard(int row, int col) { return new Thickness(74 + col * 190, 50 + row * 307, 1166 - col * 190, 200); }
    public static int GetRow(int index, int cols) { return index / cols; }
    public static int GetCol(int index, int cols) { return index % cols; }
    #endregion
  }

}
