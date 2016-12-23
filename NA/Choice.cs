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
  public class Choice : INotifyPropertyChanged
  {
    public string Name { get; set; }
    public string Text { get; set; }
    public int Num { get { return num; } set { num = value; NotifyPropertyChanged(); } }
    int num;
    public string Path { get; set; }
    public bool IsSelectable { get { return isSelectable; } set { if (isSelectable != value) { isSelectable = value; NotifyPropertyChanged(); } } }
    bool isSelectable;
    public bool IsSelected { get { return isSelected; } set { if (isSelected != value) { isSelected = value; NotifyPropertyChanged(); } } }
    bool isSelected;

    #region helpers
    public override string ToString() { return Name + " (" + Num + ")"; }
    public event PropertyChangedEventHandler PropertyChanged; public virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null) { this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
    #endregion
  }
}
