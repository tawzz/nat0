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
    public List<Res> StandardList { get { return dict.Values.Where(x => Res.StandardResources.Contains(x.Name)).ToList(); } }
    Dictionary<string, Res> dict = new Dictionary<string, Res>();

    public int n(string resname) { if (dict.ContainsKey(resname)) return dict[resname].Num; else return 0; }
    public int n(IEnumerable<string> resnames)
    {
      int result = 0;
      foreach (var resname in resnames)
      {
        if (dict.ContainsKey(resname)) result+=dict[resname].Num; 
      }
      return result;
    }
    public Res get(string resname) { EnsureRes(resname); return dict[resname]; }
    public void set(string resname, int val) { EnsureRes(resname); dict[resname].Num = val; }
    public int inc(string resname, int val) { EnsureRes(resname); EnsurePos(val); return dict[resname].Num += val; }
    public void Clear() { foreach (var key in dict.Keys) dict[key].Num = 0;}

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
