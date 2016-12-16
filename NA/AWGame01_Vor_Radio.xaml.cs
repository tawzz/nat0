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
using System.Windows.Threading;

namespace ations
{
  public partial class AWGame : Window
  {
    public AGame Game { get; set; }
    public AWGame()
    {
      Game = AGame.GameInstance;
      InitializeComponent();
      Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() => { DataContext = Game; }));
    }
  }
}
