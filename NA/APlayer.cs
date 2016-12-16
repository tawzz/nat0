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
  public class APlayer : INotifyPropertyChanged
  {
    public ObservableCollection<ACard> Cards { get; set; }

    public AResDict Res { get; set; }
    public ObservableCollection<AWorker> ExtraWorkers { get; set; }//cost res, cost count, margin, is checked out
    public Dictionary<string, bool> Defaulted { get; set; }
    public Brush Brush { get; set; }
    public int Index { get; set; }
    public String Name { get; set; }
    public ACiv Civ { get; set; }
    public int Level { get; set; }
    public bool IsMainPlayer { get { return isMainPlayer; } set { isMainPlayer = value; NotifyPropertyChanged(); } }
    bool isMainPlayer;
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
          Res.set("book",value);
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

    public APlayer() { }
    public APlayer(string name, string civ, Brush brush, int level, int index)
    {
      Index = index; //index in player order
      Level = level % LevelOffsetX.Length;
      LevelPosition = new Point(LevelOffsetX[Level] + LevelIncrementX * Index, LevelOffsetY[Level]);
      Name = name;
      Brush = brush;
      Civ = new ACiv(civ);
      Cards = Civ.GetCards();
      Res = Civ.GetResources();
      ExtraWorkers = Civ.GetExtraWorkers();
      Defaulted = new Dictionary<string, bool>();


      Books = Stability = Military = 0; //testing
    }

    public bool MoreThanOneWorkerAvailable()
    {
      var wfree1 = ExtraWorkers.FirstOrDefault(x => !x.IsCheckedOut);
      var wfree2 = ExtraWorkers.LastOrDefault(x => !x.IsCheckedOut);
      return (wfree1.CostRes != wfree2.CostRes);
    }
    public void CheckOutWorker(AWorker w)
    {
      w.IsCheckedOut = true;
      Res.inc("worker", 1);
    }
    public void Pay(int cost) { Res.dec("gold", cost); }
    public void Pay(string res, int cost)
    {
      var num = Res.n(res);
      if (num < cost)
      {
        var diff = Res.dec(res, num); // apy all in this resource first
        if (!Defaulted.ContainsKey(res))
        {
          Defaulted.Add(res, true);
          var vp_remaining = Res.dec("vp", 1);
        }
        Pay("book", cost - num);
      }
    }


    #region other safe helpers
    public event PropertyChangedEventHandler PropertyChanged; public void NotifyPropertyChanged([CallerMemberName] string propertyName = null) { this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
    public override string ToString() { return Name; }
    #endregion
  }
}
