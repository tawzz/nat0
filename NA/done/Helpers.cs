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
using System.Diagnostics;

namespace ations
{
  public static class Helpers
  {
    public const string URISTART = "pack://application:,,,/ations;component/assets/";
    public static string[] allCardPathNames = { "cards0", "allCivCards", "cardsdyn0", "cardsfake0" };
    public static Dictionary<string, Tuple<XElement,string>> CardDictionary { get; set; }
    public static List<XElement> CardXList { get; set; }

    public static XElement[] GetCardarrayX(int age, bool incldyn, bool inclfake)
    {
      var pathnames = new List<string>();
      pathnames.Add(allCardPathNames[0] + age);
      if (incldyn) pathnames.Add(allCardPathNames[2] + age);
      if (inclfake) pathnames.Add(allCardPathNames[3] + age);
      var result = new List<XElement>();
      foreach (var path in pathnames)
      {
        var xel = GetX("cards/xml/" + path + ".xml");
        if (xel == null) throw new Exception("coudn't load cards from " + path);
        result.AddRange(xel.Elements());
      }
      return result.ToArray();
    }
    public static XElement[] GetEventCardarrayX(int age)
    {
      //Debug.Assert(age >= 1, "Helpers.GetEventCardArrayX called with age<1");
      if (age < 1) age = 1; //testing
      var xfile = GetX("cards/xml/eventcards.xml");
      var e1 = xfile.Elements().ToArray(); //the 4 ages
      var e2 = e1[age - 1].Elements().ToArray();
      return e2;//.Elements().Elements("age"+age).ToArray();
    }
    public static XElement GetEventCardX(string name)
    {
      var xfile = GetX("cards/xml/eventcards.xml");
      var evAllAges = xfile.Elements().ToArray(); //the 4 ages
      foreach (var evAge in evAllAges)
      {
        var xcard = evAge.Elements().FirstOrDefault(x => x.astring("name") == name);
        if (xcard != null) return xcard;
      }
      return null;
    }
    public static XElement GetCivX(string civ) { return GetX("civs/xml/" + civ + ".xml"); }
    public static XElement GetCardX(string cardname, int age = 1)
    {
      XElement result = null;
      foreach (var path in allCardPathNames)
      {
          var extpath = path.EndsWith("0") ? path + age : path;
          var xarr = GetX("cards/xml/" + extpath + ".xml"); // first try find in cards0<age>.xml
          if (xarr != null) result = xarr.Descendants("card").FirstOrDefault(x => x.astring("name") == cardname);
          if (result != null) return result;
      }
      throw new Exception("card with name: " + cardname + " NOT found!!!!");
    }

    public static void MakeCardIndex()
    {
      if (CardDictionary == null)
      {
        CardDictionary = new Dictionary<string, Tuple<XElement, string>>();
        CardXList = new List<XElement>();
      }
      CardDictionary.Clear(); CardXList.Clear();

      var pathnames = new List<string> { allCardPathNames[1] };
      foreach (var age in new int[] { 1, 2, 3, 4 })
      {
        pathnames.Add(allCardPathNames[0] + age);
        pathnames.Add(allCardPathNames[2] + age);
        pathnames.Add(allCardPathNames[3] + age);
      }
      bool isCivCard = true; // only for first path in pathnames
      foreach (var path in pathnames)
      {
        var xel = GetX("cards/xml/" + path + ".xml");
        if (xel == null) throw new Exception("coudn't load cards from " + path);
        var cardlist = xel.Descendants("card").ToList();
        CardXList.AddRange(cardlist);
        foreach(var cardx in cardlist)
        {
          var name = cardx.astring("name");
          if (name == "buffalo_horde")
          {
            int n = 5;
          }
          if (string.IsNullOrEmpty(name)) continue;
          var imagePath = Helpers.GetImagePath(cardx, isCivCard);
          if (CardDictionary.ContainsKey(name))
          {
            string msg = "duplicate: " + name;
            //CardDictionary[name] = new Tuple<XElement, string>(cardx, imagePath);
          }
          else CardDictionary.Add(name, new Tuple<XElement, string>(cardx, imagePath));
        }
        isCivCard = false;
      }


    }
    public static XElement GetCardX(string name)
    {
      Debug.Assert(Helpers.CardDictionary.ContainsKey(name), "Helpers:GetCardX! name " + name + " not found in CardDictionary!!!");
      return CardDictionary[name].Item1;
    }
    public static BitmapImage GetCardImage(string name)
    {
      Debug.Assert(Helpers.CardDictionary.ContainsKey(name), "Helpers:GetCardImage! name " + name + " not found in CardDictionary!!!");
      return GetImage(CardDictionary[name].Item2);
    }
    public static string GetImagePath(string name) // for cards or resources
    {
      if (CardDictionary.ContainsKey(name)) return URISTART + CardDictionary[name].Item2;
      else return URISTART + "misc/" + name + ".png";
    }

    public static XElement GetX(string pathAfterAssets)
    {
      Uri uri = new Uri(URISTART + pathAfterAssets);
      StreamResourceInfo info = Application.GetResourceStream(uri);
      try
      {
        if (info != null) { XElement x = XElement.Load(info.Stream); return x; } else return null;
      }
      catch(Exception ex) { string msg = ex.Message; return null; }
    }
    public static XElement LoadFromFile(string path)
    {
      //var xml = XElement.Load(path);
      //var txt = File.ReadAllText(path);
      //XElement result = XElement.Parse(txt);
      XElement result;
      if (!path.Contains("\\")) path = Path.Combine(Environment.CurrentDirectory, path);
      if (path.TryLoadXElement(out result)) return result;
      else { Console.WriteLine("LoadFromFile: couldn't load file: " + path); return null; }
    }
    public static void SaveToFile(XElement x, string path)
    {
      try
      {
        if (!path.Contains("\\")) path = Path.Combine(Environment.CurrentDirectory, path);
        File.WriteAllText(path, x.ToString());
      }
      catch { Console.WriteLine("SaveToFile: couldn't write file: " + path);  }
    }

    public static BitmapImage GetProgressboardImage() { return GetImage("boards/progress_board.jpg"); }
    public static BitmapImage GetStatsboardImage() { return GetImage("boards/stats_board.jpg"); }
    public static BitmapImage GetCivImage(string civ) { return GetImage("civs/" + civ + ".jpg"); }
    public static BitmapImage GetEventCardImage(string cardname, int age) { return GetImage("cards/event/age" + age + "/age" + age + "_" + cardname + ".jpg"); }

    public static string GetImagePath(XElement xcard, bool isCivCard = false)
    {
      var name = xcard.astring("name");
      var age = xcard.astring("age");
      var type = xcard.astring("type");
      if (name == "extra_siberia") return "dyn/extra_siberia.jpg";
      else if (string.IsNullOrEmpty(xcard.astring("name"))) return "cards/civcards/" + type + ".png";
      else if (type == "dynasty") return "cards/civcards/" + name + ".jpg";
      else if (isCivCard) return "cards/civcards/age" + age + "_" + name + ".jpg";
      else if (xcard.astring("exp") == "dyn") return "dyn/age" + age + "/age" + age + "_" + name + ".jpg";
      else return "cards/age" + age + "/age" + age + "_" + name + ".jpg";
    }
    public static BitmapImage GetCardImage(XElement xcard, bool isCivCard = false)
    {
      var name = xcard.astring("name");
      var age = xcard.astring("age");
      var type = xcard.astring("type");
      if (name == "extra_siberia") return GetImage("dyn/extra_siberia.jpg");
      else if (string.IsNullOrEmpty(xcard.astring("name"))) return GetImage("cards/civcards/" + type + ".png");
      else if (isCivCard)
      {
        if (type == "dynasty") return GetImage("cards/civcards/" + name + ".jpg");
        return GetImage("cards/civcards/age"+ age + "_" + name + ".jpg");
      }
      else if (xcard.astring("exp") == "dyn")
      {
        if (type == "dynasty") return GetImage("dyn/dynasties/" + name + ".jpg");
        else return GetImage("dyn/age" + age + "/age" + age + "_" + name + ".jpg");
      }
      else return GetImage("cards/age" + age + "/age" + age + "_" + name + ".jpg");
    }

    public static BitmapImage GetMiscImage(string filename) { return GetImage("misc/" + filename + ".png"); }

    public static BitmapImage GetImage(string pathAfterAssets)
    {
      try
      {
        BitmapImage bmp = new BitmapImage();
        bmp.BeginInit();
        bmp.UriSource = new Uri(URISTART + pathAfterAssets); //geht!
        bmp.EndInit();
        return bmp;
      }
      catch
      {
        return null;
      }
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
