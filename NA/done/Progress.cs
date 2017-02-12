using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace ations
{
  public class Progress : INotifyPropertyChanged
  {
    public ObservableCollection<Card> Cards { get; set; }
    public XElement[] XDeck { get; set; }
    //public List<Card> CardDeck { get; set; }
    public BitmapImage Image { get; set; }
    public Field[] Fields { get; set; }
    public Thickness Margin { get; set; }
    public int Rows { get; set; }
    public int Cols { get; set; }
    public bool IncludeDynasty { get; set; }
    public bool IncludeFake { get; set; }
    public bool BalancedDeal { get; set; }
    public int MaxBuildings { get; set; }
    public int MaxWonderNatural { get; set; }
    public int MaxOthers { get; set; }

    Game game;

    public Progress(Game game, int cols, bool inclDyn = true, bool inclFake = false, bool balancing = true)
    {
      this.game = game;
      Rows = 3;
      Cols = cols;
      IncludeDynasty = inclDyn;
      IncludeFake = inclFake;
      BalancedDeal = balancing;
      MaxBuildings = cols <= 5 ? 5 : cols == 6 ? 6 : 8;
      MaxWonderNatural = cols <= 5 ? 3 : cols == 6 ? 4 : 5;
      MaxOthers = cols <= 4 ? 2 : cols <= 6 ? 3 : 4;
      Image = Helpers.GetProgressboardImage();
      Fields = new Field[Rows * Cols];
      Margin = new Thickness(72, 50, 576 - 190 * (Cols - 4), -6); //fuer uniformgrid
      for (int r = 0; r < Rows; r++)
        for (int c = 0; c < Cols; c++)
          Fields[r * Cols + c] = new Field
          {
            Index = r * Cols + c,
            Row = r,
            Col = c,
            RenderTransformOrigin = new Point(c == 0 ? 0.2 : c == 6 ? .8 : 0.5, r == 0 ? .2 : r == 2 ? .8 : .5)
          };
      Cards = new ObservableCollection<Card>();
    }
    public void UpdateDeck(int age)
    {
      if (age > Game.MAX_AGE) throw new Exception("Cannot UpdateDeck for age " + age + "!!!!!");
      if (age < 1) { age = 1; game.Stats.UpdateRound(0); game.Stats.UpdateAge(); }
      XDeck = Helpers.GetCardarrayX(age, IncludeDynasty, IncludeFake).OrderBy(x => Rand.N()).ToArray();
      //CardDeck = XDeck.Select(x=> Card.MakeCard(x)).ToList(); // wenn das zulange dauert, wieder weg!
    }
    public void Deal()
    {
      try
      {
        if (XDeck == null) { UpdateDeck(game.Stats.Age); }

        // preserve cards from 1. row
        var row1 = Fields.Take(Cols).Where(x => !x.IsEmpty).Select(x => x.Card).ToArray();
        //var num = Rows * Cols - row1.Count();

        // while not enough cards
        List<Card> newcards = row1.ToList(); // am ende reverse this list to get row1 at end
        var xlist = XDeck.ToList();
        while (newcards.Count < Rows * Cols)
        {
          // pick card from top of deck, if meet balancing rule, put the
          var card = Card.MakeCard(xlist.First());
          var type = card.Type;
          var numThisType = newcards.Count(x => x.Type == type);
          var typeBound = BalancedDeal ? type == "building" ? MaxBuildings
            : type == "wonder" || type == "natural" ? MaxWonderNatural 
            : MaxOthers : 999;
          if (numThisType < typeBound) newcards.Add(card);
          xlist.Add(card.X);
          xlist.RemoveAt(0);
        }

        var cards = newcards.ToArray().Reverse().ToArray();
        XDeck = xlist.ToArray();

        //cards = XDeck.Take(num).Select(x => Card.MakeCard(x)).ToArray();
        //cards = cards.Concat(row1).ToArray();
        //XDeck = XDeck.Skip(num).ToArray();

        Cards.Clear();
        for (int r = 0; r < Rows; r++)
          for (int c = 0; c < Cols; c++)
          {
            var idx = r * Cols + c;
            Fields[idx].Card = cards[idx];
            cards[idx].BasicCost = 3 - r;
            Cards.Add(cards[idx]);
          }
      }
      catch (Exception ex) { string msg = ex.Message; }
    }
    public void Deal_noBalancing()
    {
      Card[] cards = null;
      try
      {
        if (XDeck == null || XDeck.Length < Rows * Cols) UpdateDeck(game.Stats.Age);

        //preserve cards from 1. row
        var row1 = Fields.Take(Cols).Where(x => !x.IsEmpty).Select(x => x.Card).ToArray();// Cards.Take(Cols).Where(x=>!x.ToArray();
        var num = Rows * Cols - row1.Count(); //Rows*Cols;

        cards = XDeck.Take(num).Select(x => Card.MakeCard(x)).ToArray();
        cards = cards.Concat(row1).ToArray();
        XDeck = XDeck.Skip(num).ToArray();

        Cards.Clear();
        for (int r = 0; r < Rows; r++)
          for (int c = 0; c < Cols; c++)
          {
            var idx = r * Cols + c;
            Fields[idx].Card = cards[idx];
            cards[idx].BasicCost = 3 - r;
            Cards.Add(cards[idx]);
          }
      }
      catch (Exception ex) { string msg = ex.Message; }
    }
    public void Remove(Field field)
    {
      field.Card.IsSelected = false;
      Cards.Remove(field.Card);
      field.Card = null;
    }

    public XElement ToXml()
    {
      var result = new XElement("progress");
      foreach (var f in Fields) result.Add(new XAttribute("f" + f.Index, f.IsEmpty ? "" : f.Card.Name));
      return result;
    }
    public void LoadXml(XElement xprogress, int age = 1) // assumes that the correct number of fields has been created
    {
      Cards.Clear();
      for (int r = 0; r < Rows; r++)
        for (int c = 0; c < Cols; c++)
        {
          var idx = r * Cols + c;
          var cardname = xprogress.astring("f" + idx);
          if (!string.IsNullOrEmpty(cardname))
          {
            var card = Card.MakeCard(cardname);
            card.BasicCost = 3 - r;
            Fields[idx].Card = card;
            Cards.Add(card);
          }
          else Fields[idx].Card = null;
        }
    }

    public event PropertyChangedEventHandler PropertyChanged; public void NotifyPropertyChanged([CallerMemberName] string propertyName = null) { this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
  }
}
