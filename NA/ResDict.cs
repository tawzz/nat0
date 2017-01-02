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
    public Res get(string resname) { EnsureRes(resname); return dict[resname]; }
    public void set(string resname, int val) { EnsureRes(resname); dict[resname].Num = val; }
    public int inc(string resname, int val) { EnsureRes(resname); EnsurePos(val); return dict[resname].Num += val; }


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

    public event PropertyChangedEventHandler PropertyChanged; public void NotifyPropertyChanged([CallerMemberName] string propertyName = null) { this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
  }
}
