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
  public class Worker : INotifyPropertyChanged
  {
    public string CostRes { get; set; }
    public int Cost { get { return cost; } set { if (cost != value) { cost = value; NotifyPropertyChanged(); } } }
    int cost;
    public Thickness Margin { get; set; }
    public bool IsCheckedOut { get { return isCheckedOut; } set { if (isCheckedOut != value) { isCheckedOut = value; NotifyPropertyChanged(); } } }
    bool isCheckedOut;

    public Worker(string resname,int cost, Thickness margin, bool checkedOut=false)
    {
      CostRes = resname;
      Cost = cost;
      Margin = margin;
      IsCheckedOut = checkedOut;
    }

    public event PropertyChangedEventHandler PropertyChanged; public virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null) { this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }

  }
}
