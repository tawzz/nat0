using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml.Linq;

namespace ations
{
  public class AProgress : INotifyPropertyChanged
  {
    #region progress board
    public ObservableCollection<ACard> Cards { get; set; }
    public XElement[] Deck { get; set; }
    public BitmapImage Image { get; set; }
    public AField[] Fields { get; set; }
    public Thickness Margin { get; set; }
    public int Rows { get; set; }
    public int Cols { get; set; }
    #endregion

    public AProgress(int cols)
    {
      Rows = 3;
      Cols = cols;
      Image = Helpers.GetProgressboardImage();
      Fields = new AField[Rows*Cols];
      Margin = new Thickness(72, 50, 576 - 190 * (Cols - 4), -6); //fuer uniformgrid
      for (int r = 0; r < Rows; r++)
        for (int c = 0; c < Cols; c++)
          Fields[r * Cols + c] = new AField { Index = r * Cols + c, Row = r, Col = c };
      Cards = new ObservableCollection<ACard>();
    }
    public void UpdateDeck(int age=1)
    {
      if (age > AGame.MAX_AGE) throw new Exception("Cannot UpdateDeck for age " + age + "!!!!!");
      Deck = Helpers.GetCardarrayX(age).OrderBy(x => Rand.N()).ToArray();
    }
    public void Deal(int age=1)
    {
      if (Deck == null || Deck.Length < Rows * Cols) UpdateDeck(age);

      var cards = Deck.Take(Rows * Cols).ToArray();
      Deck = Deck.Skip(Rows * Cols).ToArray();
      Cards.Clear();
      for (int r = 0; r < Rows; r++)
        for (int c = 0; c < Cols; c++)
        {
          var idx = r * Cols + c;
          var card = ACard.MakeProgressCard(cards[idx], Fields[idx]);
          if (string.IsNullOrEmpty(card.Name))
          {
            card.Name = "unknown card";
          }
          //TODO* card.Cost = 3 * (3 - r); //cards of row 3 not affordable unless picked gold! nur zum testen! 3-r;
          Cards.Add(card);
        }
    }
    public void Remove(AField field)
    {
      Cards.Remove(field.Card);
      field.Card = null;
    }

    #region other safe helpers
    public event PropertyChangedEventHandler PropertyChanged; public void NotifyPropertyChanged([CallerMemberName] string propertyName = null) { this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
    #endregion
  }
}
