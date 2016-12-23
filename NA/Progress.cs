using System;
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
    #region progress board
    public ObservableCollection<Card> Cards { get; set; }
    public XElement[] Deck { get; set; }
    public BitmapImage Image { get; set; }
    public Field[] Fields { get; set; }
    public Thickness Margin { get; set; }
    public int Rows { get; set; }
    public int Cols { get; set; }
    #endregion

    public Progress(int cols)
    {
      Rows = 3;
      Cols = cols;
      Image = Helpers.GetProgressboardImage();
      Fields = new Field[Rows*Cols];
      Margin = new Thickness(72, 50, 576 - 190 * (Cols - 4), -6); //fuer uniformgrid
      for (int r = 0; r < Rows; r++)
        for (int c = 0; c < Cols; c++)
          Fields[r * Cols + c] = new Field { Index = r * Cols + c, Row = r, Col = c };
      Cards = new ObservableCollection<Card>();
    }
    public void UpdateDeck(int age=1)
    {
      if (age > Game.MAX_AGE) throw new Exception("Cannot UpdateDeck for age " + age + "!!!!!");
      Deck = Helpers.GetCardarrayX(age).OrderBy(x => Rand.N()).ToArray();
    }
    public void Deal(int age=1)
    {
      if (Deck == null || Deck.Length < Rows * Cols) UpdateDeck(age);

      //preserve cards from 1. row
      var row1 = Fields.Take(Cols).Where(x => !x.IsEmpty).Select(x => x.Card).ToArray();// Cards.Take(Cols).Where(x=>!x.ToArray();
      var num = Rows * Cols - row1.Count(); //Rows*Cols;

      var cards = Deck.Take(num).Select(x=>Card.MakeCard(x)).ToArray();
      cards = cards.Concat(row1).ToArray();
      Deck = Deck.Skip(num).ToArray();

      Cards.Clear();
      for (int r = 0; r < Rows; r++)
        for (int c = 0; c < Cols; c++)
        {
          var idx = r * Cols + c;
          Fields[idx].Card = cards[idx];
          //var card = Card.MakeProgressCard(xcards[idx], Fields[idx]);
          //if (string.IsNullOrEmpty(card.Name))
          //{
          //  card.Name = "unknown card";
          //}
          //TODO* card.Cost = 3 * (3 - r); //cards of row 3 not affordable unless picked gold! nur zum testen! 3-r;
          Cards.Add(cards[idx]);
        }
    }
    public void Remove(Field field)
    {
      field.Card.IsSelected = false;
      Cards.Remove(field.Card);
      field.Card = null;
    }

    #region other safe helpers
    public event PropertyChangedEventHandler PropertyChanged; public void NotifyPropertyChanged([CallerMemberName] string propertyName = null) { this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
    #endregion
  }
}
