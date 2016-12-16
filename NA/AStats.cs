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
  public class AStats : DependencyObject, INotifyPropertyChanged
  {
    #region statsboard
    public BitmapImage StatsboardImage { get; set; }
    public int Round { get { return round; } set { if (round != value) { round = value % AGame.MAX_ROUNDS; RoundMarkerPosition = new Point(pRoundMarker[round, 0] - 221, pRoundMarker[round, 1] - 6); } } }
    int round = -1;
    static int[,] pRoundMarker = { { 0, 116 }, { 70, 116 }, { 200, 116 }, { 280, 116 }, { 0, 216 }, { 70, 216 }, { 200, 216 }, { 280, 216 } };
    public Point RoundMarkerPosition { get { return roundMarkerPosition; } set { roundMarkerPosition = value; NotifyPropertyChanged(); } }
    Point roundMarkerPosition;

    public int Age { get { return age; } set { if (age != value) { age = value % (AGame.MAX_AGE + 1); NotifyPropertyChanged(); } } }
    int age;

    public XElement[] EventCardDeck { get; set; }
    public ACard EventCard { get { return eventCard; } set { eventCard = value; NotifyPropertyChanged(); } }
    ACard eventCard;
    public int Architects { get { return architects; } set { architects = value; NotifyPropertyChanged(); } }
    int architects;
    public int Turmoils { get { return turmoils; } set { turmoils = value; NotifyPropertyChanged(); } }
    int turmoils;

    public bool IsWar { get; set; }
    public ACard WarCard { get { return warCard; } set { if (warCard != value) { warCard = value; NotifyPropertyChanged(); } } }
    ACard warCard;
    public Point WarPosition { get { return warPosition; } set { warPosition = value; NotifyPropertyChanged(); } }
    Point warPosition;

    AGame game;
    #endregion

    public AStats(AGame game)
    {
      StatsboardImage = Helpers.GetStatsboardImage();
      this.game = game;
    }
    public void UpdateWarPosition(APlayer p = null, ACard card = null)
    {
      if (p == null) { WarPosition = new Point(0, 0); WarCard = null; }
      else
      {
        Point milMargin = new Point(150, 1100);
        int diff = 10;
        WarPosition = new Point(p.MilitaryPosition.X + milMargin.X - diff, p.MilitaryPosition.Y - milMargin.Y - diff);
        IsWar = true;
        if (card != null) WarCard = card;
      }
    }
    public void UpdateRound()    {      UpdateRound(Round + 1);    }
    public void UpdateRound(int round)    {      Round = round;     }
    public void UpdateAge() { UpdateAge(Round / 2 + 1); }
    public void UpdateAge(int newage)
    {
      if (Age != newage)
      {
        Age = newage;
        UpdateEventDeck(newage);
        game.Progress.UpdateDeck(newage);
      }
    }
    public void PickEventCard() 
    {
      if (EventCardDeck == null || EventCardDeck.Count() == 0) UpdateEventDeck(Age); // just in case run out of event cards (which doesnt normally happen)
      EventCard = ACard.MakeEventCard(EventCardDeck.First());
      Turmoils = Architects = EventCard.X.aint("architects") + game.NumPlayers - (game.NumPlayers <= 3 ? 1 : 2);
    }
    public void UpdateEventDeck(int age=1)    {      EventCardDeck = Helpers.GetEventCardarrayX(age).OrderBy(x => Rand.N()).ToArray();    }



    #region other safe helpers
    public event PropertyChangedEventHandler PropertyChanged; public void NotifyPropertyChanged([CallerMemberName] string propertyName = null) { this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
    #endregion
  }
}
