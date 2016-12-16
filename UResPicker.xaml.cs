using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ations
{
  public partial class UResPicker : UserControl, INotifyPropertyChanged
  {
    //public ObservableCollection<Res> ResourceList { get; set; }

    //public static UResPicker ResourcePicker;
    //public IEnumerable<Res> ResourceList { get { return resourceList; } set { ResourceList = value; NotifyPropertyChanged(); } }
    //IEnumerable<Res> resourceList;
    //public Res SelectedRes { get; set; }
    public UResPicker()
    {
      //ResourceList = reslist;
      //ResourcePicker = this;
      InitializeComponent();
      //DataContext = this;
    }

    //private void btnDialogOk_Click(object sender, RoutedEventArgs e)
    //{
    //  if (DataContext as Game != null) (DataContext as Game).SelectedResource = lbHallo.SelectedItem as Res;
    //}
    //public event PropertyChangedEventHandler PropertyChanged; public virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null) { if (this.PropertyChanged != null) { this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); } }

  }
}
