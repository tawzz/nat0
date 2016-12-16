using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ations
{
  public class ARes : INotifyPropertyChanged
  {
    public string Name { get; set; }
    public int Num { get { return num; } set { num = value; NotifyPropertyChanged(); } }
    int num;
    public string Path { get; set; }

    public ARes() { }
    public ARes(string name) { Name = name; Path = Helpers.URISTART + "misc/" + name + ".png"; }
    public ARes(string name, int num) { Name = name; Num = num; Path = Helpers.URISTART + "misc/" + name + ".png"; }
    public override string ToString() { return Name; }
    public event PropertyChangedEventHandler PropertyChanged; public virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null) { this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
  }
}
