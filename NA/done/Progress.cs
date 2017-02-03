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
    public XElement[] Deck { get; set; }
    public BitmapImage Image { get; set; }
    public Field[] Fields { get; set; }
    public Thickness Margin { get; set; }
    public int Rows { get; set; }
    public int Cols { get; set; }
    public bool IncludeDynasty { get; set; }
    public bool IncludeFake { get; set; }

    public Progress(int cols, bool inclDyn=true, bool inclFake=false)
    {
      Rows = 3;
      Cols = cols;
      IncludeDynasty = inclDyn;
      IncludeFake = inclFake;
      Image = Helpers.GetProgressboardImage();
      Fields = new Field[Rows * Cols];
      Margin = new Thickness(72, 50, 576 - 190 * (Cols - 4), -6); //fuer uniformgrid
      for (int r = 0; r < Rows; r++)
        for (int c = 0; c < Cols; c++)
          Fields[r * Cols + c] = new Field { Index = r * Cols + c, Row = r, Col = c };
      Cards = new ObservableCollection<Card>();
    }
    public void UpdateDeck(int age)
    {
      if (age > Game.MAX_AGE) throw new Exception("Cannot UpdateDeck for age " + age + "!!!!!");
      if (age < 1) { age = 1; Game.Inst.Stats.UpdateRound(0);Game.Inst.Stats.UpdateAge(); }
      Deck = Helpers.GetCardarrayX(age,IncludeDynasty,IncludeFake).OrderBy(x => Rand.N()).ToArray();
    }
    public void Deal()
    {
      Card[] cards = null;
      try
      {
        if (Deck == null || Deck.Length < Rows * Cols) UpdateDeck(Game.Inst.Stats.Age);

        //preserve cards from 1. row
        var row1 = Fields.Take(Cols).Where(x => !x.IsEmpty).Select(x => x.Card).ToArray();// Cards.Take(Cols).Where(x=>!x.ToArray();
        var num = Rows * Cols - row1.Count(); //Rows*Cols;

        cards = Deck.Take(num).Select(x => Card.MakeCard(x)).ToArray();
        cards = cards.Concat(row1).ToArray();
        Deck = Deck.Skip(num).ToArray();

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
      foreach (var f in Fields) result.Add(new XAttribute("f"+f.Index,f.IsEmpty?"":f.Card.Name));
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
