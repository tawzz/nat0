using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ations
{
  public partial class WResourcePicker : Window
  {
    public IEnumerable<Res> ResourceList { get; set; }
    public Res SelectedRes { get; set; } 
    public WResourcePicker(IEnumerable<Res> reslist)
    {
      ResourceList = reslist;
      InitializeComponent();
      DataContext = this;
    }

    private void Window_ContentRendered(object sender, EventArgs e)
    {

    }

    private void btnDialogOk_Click(object sender, RoutedEventArgs e)
    {
      SelectedRes = lbHallo.SelectedItem as Res;
      this.DialogResult = true;
    }
  }
}
