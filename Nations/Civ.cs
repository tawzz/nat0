using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace ations
{
  public class Civ
  {
    public String Name { get; set; }
    public BitmapImage Image { get; set; }
    public XElement X { get; set; }
    public List<Field> Fields { get; set; }
    #region Civ.Margins margins of cards and workers on civboard
    public static Thickness[] CardMargins = {
        new Thickness(5, 40, 1594, 565),//advisor
        new Thickness(290, 45, 1324, 570),//buildings and military 1 to 4
        new Thickness(545, 45, 1069, 570),
        new Thickness(800, 45, 814, 570),
        new Thickness(1055, 45, 559, 570),
        new Thickness(1330, 105, 284, 610),//special ability
        new Thickness(1585, 42, 17, 565),//wonder in construction
        //row 2
        new Thickness(5, 425, 1594, 186),//colonies 1 and 2
        new Thickness(265, 425, 1344, 186),
        new Thickness(565, 425, 1039, 186),//wonders 1 to 5
        new Thickness(820, 425, 784, 186),
        new Thickness(1075, 425, 529, 186),
        new Thickness(1332, 425, 277, 186),
        new Thickness(1587, 425, 21, 186),
        // wonder instead of colony 2
        new Thickness(315, 425, 1289, 186),//wonder 0 (extra wonder) instead of colony
        new Thickness(1330, 45, 284, 570),//special ability when upgraded dynasty or if it is a 5th building (japan)

      };
    const int EXTRA_WONDER_INDEX = 14;
    public static Dictionary<string, int[]> WorkerMargins = new Dictionary<string, int[]>{
        {"china", new int[] {72,168,260,390,480,576,670,0} },
        {"mongolia", new int[] {22,120,210,340,436,530,620,716} },
        {"default", new int[] {22,120,210,300,436,530,620,716} },
       };
    #endregion

    public Civ(string civ)
    {
      Name = civ;
      Image = Helpers.GetCivImage(civ);
      X = Helpers.GetCivX(civ);
      Fields = new List<Field>();
      var xfields = X.Element("cards").Elements().ToArray();
      for (int i = 0; i < xfields.Length; i++)
      {
        var xfield = xfields[i];
        var types = new ObservableCollection<string>();
        var type = xfield.astring("type");
        if (type == "building" || type == "military") { types.Add("building"); types.Add("military"); }
        else if (type == "wonder" || type == "natural" || type == "wic") { types.Add("wonder"); types.Add("natural"); }
        else types.Add(type);
        var margin = i == 8 && type == "wonder" ? CardMargins[EXTRA_WONDER_INDEX] : CardMargins[i];
        Fields.Add(new Field { X = xfield, Type = type, Index = i, TypesAllowed = types, Margin = margin });
      }
    }
    public ObservableCollection<Card> GetCards()//index from 0 to 13 for 2 rows of 7 cards each!
    {
      var result = new ObservableCollection<Card>();
      foreach (var f in Fields)
      {
        var cardname = f.X.astring("name");
        result.Add(Card.MakeInitialCivCard(f, Name, cardname));// string.IsNullOrEmpty(cardname) ? Card.MakeDummyCard(f) : Card.MakeInitialCivCard(f, Name, cardname));
      }
      return result;
    }
    public ObservableCollection<Res> GetResources()
    {
      var result = new ObservableCollection<Res>();
      var resarr = X.Element("resources").Elements().ToArray();
      foreach (var res in resarr) { result.Add(new Res(res.astring("name"), res.aint("n"))); }
      return result;
    }
    public ObservableCollection<Worker> GetExtraWorkers()
    {
      var result = new ObservableCollection<Worker>();
      var resarr = X.Element("workers").Elements().ToArray();
      int i = 0;
      foreach (var res in resarr)
      {
        var costres = res.astring("res");
        var marginLeft = WorkerMargins.ContainsKey(Name) ? WorkerMargins[Name][i] : WorkerMargins["default"][i++];
        var margin = new Thickness(marginLeft, 8, 0, 0);
        result.Add(new Worker(costres, res.aint("n"), margin, false));// Helpers.URISTART + "misc/worker.png", true));//TODO!!!!!!!
      }
      return result;
    }
  }
}
