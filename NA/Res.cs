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
  public class Res : INotifyPropertyChanged
  {
    public string Name { get; set; }
    public int Num { get { return num; } set { num = value; NotifyPropertyChanged(); } }
    int num;
    public string Path { get; set; }
    public bool IsSelectable { get { return isSelectable; } set { if (isSelectable != value) { isSelectable = value; NotifyPropertyChanged(); } } }
    bool isSelectable;
    public bool IsSelected { get { return isSelected; } set { if (isSelected != value) { isSelected = value; NotifyPropertyChanged(); } } }
    bool isSelected;

    public bool CanPayWith { get { return !NotReal.Contains(Name) && Name!="book"; } }
    public bool CanPayDefaultWith { get { return !NotReal.Contains(Name); } }
    public bool VP { get { return Name == "vp"; } }

    public Res() { }
    public Res(string name) { Name = name; Path = Helpers.URISTART + "misc/" + name + ".png"; }
    public Res(string name, int num) { Name = name; Num = num; Path = Helpers.URISTART + "misc/" + name + ".png"; }

    public static string[] NotReal = { "raid", "military", "stability", "vp", "arch", "worker" };


    public override string ToString() { return Name + " ("+Num+")"; }
    public event PropertyChangedEventHandler PropertyChanged; public virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null) { this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
  }
}
