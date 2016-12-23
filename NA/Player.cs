using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ations
{
  public class Player : INotifyPropertyChanged
  {
    public ObservableCollection<Card> Cards { get; set; } //foreach field there is a card, either real or empty!

    public ResDict Res { get; set; }
    public int RaidValue { get { return raidValue; } set { if (raidValue != value) { raidValue = value; NotifyPropertyChanged(); } } }
    int raidValue = 2;//testing TODO: compute!!!
    public ObservableCollection<Worker> ExtraWorkers { get; set; }//cost res, cost count, margin, is checked out
    public Dictionary<string, int> Defaulted { get; set; } //to record by how much defaulted per resource in current round
    public Brush Brush { get; set; }
    public int Index { get; set; }
    public String Name { get; set; }
    public Civ Civ { get; set; }
    public int Level { get; set; }
    public bool IsMainPlayer { get { return isMainPlayer; } set { isMainPlayer = value; NotifyPropertyChanged(); } }
    bool isMainPlayer;
    public int NumActions { get { return numActions; } set { if (numActions != value) { numActions = value; NotifyPropertyChanged(); } } }
    int numActions;
    public bool HasPassed { get { return hasPassed; } set { if (hasPassed != value) { hasPassed = value; NotifyPropertyChanged(); } } }
    bool hasPassed;

    public bool HasWIC { get { return !WICField.IsEmpty; } }
    public Field WICField { get { return Civ.Fields[CardType.WIC]; } }
    public Card WIC { get { return HasWIC ? WICField.Card : null; } }
    public bool HasPrivateArchitect { get { return false; } } //gehoert zu checks
    #region positioning on stats board
    public Point LevelPosition { get; set; }
    static int[] LevelOffsetX = { 20, 50, 20, 50 };
    static int[] LevelOffsetY = { 20, 80, 140, 200 };
    static int LevelIncrementX = 15;

    public int Stability
    {
      get { return stability; }
      set
      {
        if (stability != value)
        {
          stability = value;
          Res.set("stability", value);
          var x = StabilityOffsetX + StabilityIncrementX * Index;
          var staby = stability > 15 ? 15 : stability < -3 ? -3 : stability;
          var y = staby >= 0 ? StabilityOffsetY - staby * StabilityIncrementY
            : StabilityOffsetYNegative - (staby * StabilityIncrementY);

          StabilityPosition = new Point(x, y);
          NotifyPropertyChanged("StabilityPosition");
          NotifyPropertyChanged();
        }
      }
    }
    int stability = -1; //trigger update at start if value 0
    public Point StabilityPosition { get; set; }
    static int StabilityOffsetX = 20;
    static int StabilityOffsetY = 855;
    static int StabilityOffsetYNegative = 935;
    static int StabilityIncrementY = 56;
    static int StabilityIncrementX = 7;

    public int Military
    {
      get { return military; }
      set
      {
        if (military != value)
        {
          military = value;
          Res.set("military", value);
          var xoffset = military <= 20 ? MilitaryOffsetXUpTo20 : MilitaryOffsetX20To40;
          var x = xoffset + MilitaryIncrementX * Index;
          var mily = military > 40 ? 40 : military;
          var y = mily <= 20 ? MilitaryOffsetY - mily * MilitaryIncrementY
            : MilitaryOffsetY - (41 - mily) * MilitaryIncrementY;

          MilitaryPosition = new Point(x, y);
          //MilitaryPosition = new Point(20 + MilitaryIncrementX * Index, MilitaryOffsetY);
          NotifyPropertyChanged("MilitaryPosition");
          NotifyPropertyChanged();
        }
      }
    }
    int military = -1; //trigger update at start if value 0
    public Point MilitaryPosition { get; set; }
    static int MilitaryOffsetXUpTo20 = 14;
    static int MilitaryOffsetX20To40 = 80;
    static int MilitaryOffsetY = 1120;
    static int MilitaryIncrementY = 54;
    static int MilitaryIncrementX = 4;

    public int Books
    {
      get { return books; }
      set
      {
        if (books != value)
        {
          books = value;
          Res.set("book", value);
          var b = value % 50;
          var x = BooksOffsetX0;
          x = b < 10 ? x + b * BooksX + 4 * Index //ok
            : b >= 10 && b <= 25 ? x + 10 * BooksX//ok
            : b > 25 && b < 35 ? x + (35 - b) * BooksX + 4 * Index // ok
            : x;
          var y = BooksOffsetY0;
          y = b <= 10 ? y //x + b * BooksX + 4 * Index //ok
            : b >= 11 && b <= 25 ? y + (b - 10) * BooksY + 4 * Index//ok
            : b > 25 && b <= 35 ? y + 15 * BooksY // ok
            : y + (50 - b) * BooksY + 4 * Index;

          BooksPosition = new Point(x, y);
          NotifyPropertyChanged("BooksPosition");

          NotifyPropertyChanged();
        }
      }
    }
    int books = -1; //trigger update at start if value 0
    public Point BooksPosition { get; set; }
    static int BooksOffsetX0 = 27;
    static int BooksOffsetY0 = 25;
    static int BooksX = 90;
    static int BooksY = 90;
    #endregion

    public Player() { }
    public Player(string name, string civ, Brush brush, int level, int index)
    {
      Index = index; //index in player order
      Level = level % LevelOffsetX.Length;
      LevelPosition = new Point(LevelOffsetX[Level] + LevelIncrementX * Index, LevelOffsetY[Level]);
      Name = name;
      Brush = brush;
      Civ = new Civ(civ);
      Cards = Civ.GetCards();
      Res = Civ.GetResources();
      ExtraWorkers = Civ.GetExtraWorkers();
      Defaulted = new Dictionary<string, int>();

      Books = Stability = Military = 0; //testing
    }

    public int UpdateResBy(string resname, int inc) //n pos or neg! returns new number
    {
      var newResCount = Res.updateBy(resname, inc);

      //recalc pos on stats board:
      if (resname == "book") Books = newResCount;
      else if (resname == "military") Military = newResCount;
      else if (resname == "stability") Stability = newResCount;

      return newResCount;
    }
    public bool Pay(int cost, string resname = "gold") //expect cost positive number!, gold is default resname
    { //returns true if debt has been payed (even if defaulted), false if default in vp or cannot pay rest in books (>need picker!)
      var rescount = Res.n(resname);
      UpdateResBy(resname, -cost);
      if (rescount < cost)
      {
        if (resname == "vp") return false; //this player cannot pay!
        var diff = cost - rescount;
        if (!Defaulted.ContainsKey(resname))
        {
          Defaulted.Add(resname, diff);
          return Pay(1, "vp");
        }
        else Defaulted[resname] += diff;

        if (resname == "book") return false; //this player cannot pay in books!
        return Pay(diff, "book");
      }
      return true; // this player has payed
    }

    public void AddCivCard(Card card, Field field)
    {
      Cards.Remove(field.Card);
      Cards.Add(card);
      Civ.Add(card, field);
    }
    public void MoveCivCard(Card card, Field fromField, Field toField)
    {
      // fromField will then contain empty card as in beginning
      Cards.Remove(toField.Card);
      toField.Card = card;
      fromField.Card = Card.MakeEmptyCard(fromField, fromField.Type);
      Cards.Add(fromField.Card);

    }

    public bool MoreThanOneWorkerAvailable()
    {
      var wfree1 = ExtraWorkers.FirstOrDefault(x => !x.IsCheckedOut);
      var wfree2 = ExtraWorkers.LastOrDefault(x => !x.IsCheckedOut);
      return (wfree1.CostRes != wfree2.CostRes);
    }
    public void CheckOutWorker(Worker w)
    {
      w.IsCheckedOut = true;
      Res.inc("worker", 1);
    }
    public int ComputeRaidValue()
    {
      var milcards = Cards.Where(x => x.mil()).ToArray();
      var maxraid = milcards.Max(x => x.NumDeployed > 0 ? x.X.aint("battle") : 0);
      return maxraid;
    }
    public void CalcMilitary()
    {
      var mil = 0;
      foreach (var c in Cards.Where(x => x != WIC)) { var factor = c.buildmil() ? c.NumDeployed : 1; mil += c.GetMilitary * factor; }
      foreach (var w in ExtraWorkers.Where(x => x.IsCheckedOut)) { if (w.CostRes == "military") { mil -= w.Cost; } }
      Military = mil; //add dyn or other special rules
    }
    public void CalcStability()
    {
      var stab = 0;
      foreach (var c in Cards.Where(x => x != WIC)) { var factor = c.buildmil() ? c.NumDeployed : 1; stab += c.GetStability * factor; }
      foreach (var w in ExtraWorkers.Where(x => x.IsCheckedOut)) { if (w.CostRes == "stability") { stab -= w.Cost; } }
      Stability = stab; //add dyn or other special rules
    }

    #region other safe helpers
    public event PropertyChangedEventHandler PropertyChanged; public void NotifyPropertyChanged([CallerMemberName] string propertyName = null) { this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
    public override string ToString() { return Name; }
    #endregion
  }
}
