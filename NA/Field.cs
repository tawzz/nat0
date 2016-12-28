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
    public ObservableCollection<string> TypesAllowed { get; set; }
    public Thickness Margin { get { return margin; } set { margin = value; }}
    Thickness margin=new Thickness(265, 425, 1344, 186);
    public bool IsEmpty { get { return Card == null || string.IsNullOrEmpty(Card.Name); } }
    public XElement X { get; set; }

    #region other safe helpers
    public event PropertyChangedEventHandler PropertyChanged; public void NotifyPropertyChanged([CallerMemberName] string propertyName = null) { this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
    public override string ToString() { return Card!=null?Card.ToString(): Type; }
    #endregion
  }
}
