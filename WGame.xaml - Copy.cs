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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using ations;

namespace ations
{
  public partial class WGame : Window
  {
    public Game Game { get; set; }
    //public static Dispatcher Disp;
    public WGame()
    {
      Game = new Game();
      InitializeComponent();
      //Disp = Dispatcher; // ?!?
      Dispatcher.BeginInvoke(DispatcherPriority.Loaded,
        new Action(() => {
          Console.WriteLine("Action Datacontext");
          DataContext = Game = new ations.Game();
          //Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() => Game.StartRound()));
        }));
      //Loaded += WGame_Loaded;
    }

    private void WGame_Loaded(object sender, RoutedEventArgs e)
    {
    }

    //UI tests
    private void bTestNextRound_Click(object sender, RoutedEventArgs e)
    {
      Game.Round=(Game.Round + 1)%8;
    }

    private void OnDeal(object sender, RoutedEventArgs e)
    {
      Game.DealProgressCards();
    }

    private void OnPickRes(object sender, RoutedEventArgs e)
    {
      var list = new List<Res>() { new Res("book"), new Res("stability"), new Res("wheat"), new Res("architect"), new Res("coal"), new Res("vp"), new Res("gold"), new Res("military") };

      var inputDialog = new WResourcePicker(list);
      if (inputDialog.ShowDialog() == true)
        lblName.Text = inputDialog.SelectedRes.Name;

    }

    private void OnStart(object sender, RoutedEventArgs e)
    {
      Game.StartRound();
    }
  }
}
