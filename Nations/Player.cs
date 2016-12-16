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
  public partial class Player : DependencyObject, INotifyPropertyChanged
  {
    public ObservableCollection<Card> Cards { get; set; }
    public Brush Brush { get; set; }
    public int Index { get; set; }
    public String Name { get; set; }
    public int Level { get; set; }
    public bool IsMainPlayer { get { return isMainPlayer; } set { isMainPlayer = value; NotifyPropertyChanged(); } }
    bool isMainPlayer;

    //_______________bis hier copiert
    public ObservableCollection<Res> Res { get; set; } //name,count,imgfile
    public int resnum(string resname) { var r = Res.FirstOrDefault(x => x.Name == resname); return r != null ? r.Num : 0; }
    public Res res(string resname) { return Res.FirstOrDefault(x => x.Name == resname); }
    public void resupdate(string resname, int inc) { var res = Res.FirstOrDefault(x => x.Name == resname); if (res != null) res.Num += inc; }
    public ObservableCollection<Worker> ExtraWorkers { get; set; }//cost res, cost count, margin, is checked out

    //public ObservableCollection<Card> Cards { get; set; }
    //public ObservableCollection<Res> Res { get; set; } 
    //public int resnum(string resname) { var r = Res.FirstOrDefault(x => x.Name == resname); return r != null ? r.Num : 0; }
    //public Res res(string resname) { return Res.FirstOrDefault(x => x.Name == resname); }
    //public void resupdate(string resname, int inc) { var res = Res.FirstOrDefault(x => x.Name == resname); if (res != null) res.Num += inc; else if (inc >= 0) Res.Add(new Res(resname, inc)); } //setresnum(resname, resnum(resname) + inc); }
    //public void setresnum(string resname, int val) { var res = Res.FirstOrDefault(x => x.Name == resname); if (res != null) res.Num = val; else if (val > 0) Res.Add(new Res(resname, val)); }
    //public ObservableCollection<Worker> ExtraWorkers { get; set; }
    public Civ Civ { get; set; }
    public bool PhaseCompleted { get { return phaseCompleted; } set { phaseCompleted = value; NotifyPropertyChanged(); } }
    bool phaseCompleted;
    public bool CanDeploy { get { return canDeploy; } set { canDeploy = value; NotifyPropertyChanged(); } }
    bool canDeploy;

    public override string ToString() { return Name; }

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
    public Dictionary<string, bool> Defaulted { get; set; }

    public Player() { }
    public Player(string name, string civ, Brush brush, int level, int index)
    {
      Index = index; //index in player order am anfang zum testen verwendet
      Books = Stability = Military = Index; //testing
      Level = level % LevelOffsetX.Length;
      LevelPosition = new Point(LevelOffsetX[Level] + LevelIncrementX * Index, LevelOffsetY[Level]);
      Name = name;
      Brush = brush;
      Civ = new Civ(civ);
      Cards = Civ.GetCards();
      Res = Civ.GetResources();
      ExtraWorkers = Civ.GetExtraWorkers();
      Defaulted = new Dictionary<string, bool>();
    }
    public void CheckOutWorker(Worker w)
    {
      w.IsCheckedOut = true;
      resupdate("worker", 1); // check extra rules!!!
    }
    public void Place(Card progresscard, Card civcard)
    {
      Card.TurnIntoCivCard(progresscard, civcard);
    }
    public void Pay(int cost) { resupdate("gold", -cost); } 
    public void Pay(string res, int cost)
    {
      var num = resnum(res);
      if (num < cost)
      {
        resupdate(res, -num); // apy all in this resource first
        if (!Defaulted.ContainsKey(res)) { Defaulted.Add(res, true); resupdate("vp", -1); }
        Pay("book", cost - num);
      }
    }


    #region zrest
    public event PropertyChangedEventHandler PropertyChanged; public virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null) { this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }

    //public bool IsMainPlayer { get { return (bool)GetValue(IsMainPlayerProperty); } set { SetValue(IsMainPlayerProperty, value); } }
    //public static readonly DependencyProperty IsMainPlayerProperty = DependencyProperty.Register("IsMainPlayer", typeof(bool), typeof(Player), new PropertyMetadata(false, IsMainPlayerChanged));
    //private static void IsMainPlayerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    //{
    //  var pl = (d as Player);
    //  if (pl != null)
    //  {
    //    var oldIsMain = (bool)e.OldValue;
    //    var newIsMain = (bool)e.NewValue;
    //    if (oldIsMain == newIsMain) return; //sollte eigentlich nicht passieren!!!
    //    // IsMainPlayer == newIsMain 
    //    var game = Game.GetGameInstance();
    //    if (game != null && newIsMain) game.MainPlayer = pl;
    //  }
    //}
    #endregion
  }
}
