using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ations
{
  public class ResDict : INotifyPropertyChanged
  {
    public List<Res> List { get { return dict.Values.ToList(); } }
    public List<Res> StandardList { get { return dict.Values.Where(x => new string[] { "wheat", "coal", "gold", "vp", "worker" }.Contains(x.Name)).ToList(); } }
    Dictionary<string, Res> dict = new Dictionary<string, Res>();
    public int n(string resname) { if (dict.ContainsKey(resname)) return dict[resname].Num; else return 0; }
    void EnsureRes(string resname)
    {
      if (!dict.ContainsKey(resname))
      {
        var res = new Res(resname, 0);
        dict.Add(resname, res);
        NotifyPropertyChanged("List");
        NotifyPropertyChanged("StandardList");
      }
    }
    void EnsurePos(int n) { Debug.Assert(n >= 0, "resource val must be positive: " + n + ", (EnsurePos)"); }
    public Res get(string resname) { EnsureRes(resname); return dict[resname]; }
    public void set(string resname, int val) { EnsurePos(val); EnsureRes(resname); dict[resname].Num = val; }

    // dec and inc expect pos value! dec will return new res num if >=0, or set res num to 0 and return neg difference (=default)
    public int dec(string resname, int val) { EnsurePos(val); EnsureRes(resname); var num = dict[resname].Num; num -= val; dict[resname].Num = Math.Max(0, num); return num; }
    public int inc(string resname, int val) { EnsurePos(val); EnsureRes(resname); return dict[resname].Num += val; }


    public event PropertyChangedEventHandler PropertyChanged; public void NotifyPropertyChanged([CallerMemberName] string propertyName = null) { this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
  }
  //public static class AResDictExtensions
  //{
  //  public static int n(this ARes r) { return r.Num; }
  //  public static int inc(this ARes r, int n) { return Math.Max(0, r.Num += n); }
  //  public static int dec(this ARes r, int n) { return Math.Max(0, r.Num -= n); }
  //  public static int set(this ARes r, int n) { return Math.Max(0, r.Num = n); }
  //  public static int gold(this APlayer pl) { return pl.Res["gold"].Num; }
  //  public static int coal(this APlayer pl) { return pl.Res["coal"].Num; }
  //  public static int wheat(this APlayer pl) { return pl.Res["wheat"].Num; }
  //  public static int mil(this APlayer pl) { return pl.Res["military"].Num; }
  //  public static int stab(this APlayer pl) { return pl.Res["stability"].Num; }
  //  public static int arch(this APlayer pl) { return pl.Res["architects"].Num; }
  //  public static int book(this APlayer pl) { return pl.Res["book"].Num; }
  //  public static int vp(this APlayer pl) { return pl.Res["vp"].Num; }
  //}
}
