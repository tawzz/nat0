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

    public Res() { }
    public Res(string name) { Name = name; Path = Helpers.URISTART + "misc/" + name + ".png"; }
    public Res(string name, int num) { Name = name; Num = num; Path = Helpers.URISTART + "misc/" + name + ".png"; }
    public override string ToString() { return Name; }
    public event PropertyChangedEventHandler PropertyChanged; public virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null) { this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }


    //*********************
    //public static readonly RoutedEvent ChanEvent = EventManager.RegisterRoutedEvent("Chan", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Res));
    //public event RoutedEventHandler Chan
    //{
    //  add { AddHandler(Res.ChanEvent, value); }
    //  remove { RemoveHandler(Res.ChanEvent, value); }
    //}
    //public void RaiseMyRoutedEvent()
    //{
    //  RaiseEvent(new RoutedEventArgs(CustomLineGraph.ThicknessEvent, this));
    //  // Your logic
    //  ChanEvent?.Invoke("sample parameter");
    //}
    //*********************
    //public delegate void MyPersonalizedUCEventHandler(string sampleParam);
    //public event MyPersonalizedUCEventHandler NumberChangedEvent;
    //public void RaiseMyEvent()
    //{
    //  // Your logic
    //  NumberChangedEvent?.Invoke("sample parameter");
    //}
    //public bool NumberChanged { get { return numberChanged; } set { if (numberChanged != value) { numberChanged = value; RaiseMyEvent(); NotifyPropertyChanged(); } } }
    //bool numberChanged;




  }
}
