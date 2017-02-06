using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml.Linq;

namespace ations
{
  public class Field: INotifyPropertyChanged
  {
    public Card Card { get { return card; } set { if (card != value) { card = value; NotifyPropertyChanged(); } } }
    Card card;

    public string Type { get; set; } 
    public int Index { get; set; }
    public int Row { get; set; }
    public int Col { get; set; }
    public Point RenderTransformOrigin { get; set; }
    //public List<string> TypesAllowedOriginal { get; set; }
    public ObservableCollection<string> TypesAllowed { get; set; }
    public Thickness Margin { get { return margin; } set { margin = value; NotifyPropertyChanged(); } }
    Thickness margin=new Thickness(265, 425, 1344, 186);
    public bool IsEmpty { get { return Card == null || string.IsNullOrEmpty(Card.Name); } }
    public XElement X { get; set; }


    public string CounterText { get { return counterText; } set { if (counterText != value) { counterText = value; NotifyPropertyChanged(); } } }
    string counterText;
    public int Counter { get { return counter; } set { if (counter != value) { counter = value; NotifyPropertyChanged(); } } }
    int counter;
    public bool IsCounterEnabled { get { return isCounterEnabled; } set { if (isCounterEnabled != value) { isCounterEnabled = value; NotifyPropertyChanged(); } } }
    bool isCounterEnabled;

    public XElement ToXml()
    {
      var result = new XElement("field",
        new XAttribute("type", Type),
        new XAttribute("index", Index),
        new XAttribute("row", Row),
        new XAttribute("col", Col),
        new XAttribute("isempty", IsEmpty));
      result.Add(X);
      if (!IsEmpty) result.Add(Card.ToXml());
      return result;
    }

    #region other safe helpers
    public event PropertyChangedEventHandler PropertyChanged; public void NotifyPropertyChanged([CallerMemberName] string propertyName = null) { this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
    public override string ToString() { return Card!=null?Card.ToString(): Type; }
    #endregion
  }
}
