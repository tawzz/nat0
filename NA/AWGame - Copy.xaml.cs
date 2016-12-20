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
    #region properties and contructor
    public AGame Game { get; set; }
    public AWGame()
    {
      Game = AGame.GameInstance;
      InitializeComponent();
      Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
      {
        DataContext = Game;
        Game.UIRoundmarker = elRoundMarker; // some ui elements needed for animations triggered in gameloop
      }));
    }
    #endregion
    #region helpers
    RadioButton GetSelectedRadio(ItemsControl ic) 
    {
      var chi = ic.FindVisualChildren<Grid>();
      var radio = chi.Select(x => x.Children[0] as RadioButton).ToArray();
      return radio.FirstOrDefault(x => x != null && x.IsChecked == true);
    }
    #endregion
    #region handlers version 0

    private void OnClickRedeal(object sender, RoutedEventArgs e) { Game.Progress.Deal(); }
    private void OnClickUndo(object sender, RoutedEventArgs e) { }
    private void OnClickBuy(object sender, RoutedEventArgs e)
    {
      var selplace = GetSelectedRadio(icCivcards);
      var selbuy = GetSelectedRadio(icProgress);

      if (selbuy != null && selplace != null)
      {
        selbuy.IsChecked = selplace.IsChecked = false;
        Game.Buy0(selbuy.DataContext as AField, selplace.DataContext as AField);
      }
      else if (selbuy != null && Game.IsUnambiguousBuy0(selbuy.DataContext as AField))
      {
        selbuy.IsChecked = false;
        Game.Buy0(selbuy.DataContext as AField);
      }
      else if (selbuy != null) Game.Message = "select a place on civ board!";
    }
    private void OnClickNext(object sender, RoutedEventArgs e) { }

    private void OnClickPick(object sender, RoutedEventArgs e) { Game.OnPick(); }
    private void OnClickPickWorker(object sender, RoutedEventArgs e) { Game.OnPickWorker(); }
    private void OnClickStart(object sender, RoutedEventArgs e) { Game.Kickoff(); }
    private void OnClickPrompt(object sender, RoutedEventArgs e) { }
    #endregion
    private void OnResNumUpdated(object sender, DataTransferEventArgs e) { Game.ResourceUpdated((sender as Grid).Parent as StackPanel); }

    private void OnClickArchitects(object sender, MouseButtonEventArgs e) { Game.OnClickArchitects(); }
    private void OnClickTurmoils(object sender, MouseButtonEventArgs e) { Game.OnClickTurmoils(); }
    private void OnClickProgressCard(object sender, RoutedEventArgs e) { Game.OnClickProgressCard((sender as FrameworkElement).DataContext as AField); }
    private void OnClickCivCard(object sender, RoutedEventArgs e) { Game.OnClickCivCard((sender as FrameworkElement).DataContext as AField); }

    private void OnClickButton(object sender, RoutedEventArgs e) { if (!Game.IsRunning) Game.Kickoff(); else Game.OkStartClicked = true; }
    private void OnClickSTOP(object sender, RoutedEventArgs e) { Game.Interrupt = true; }
    private void OnClickCancel(object sender, RoutedEventArgs e) { Game.CancelClicked = true; }
    private void OnClickPass(object sender, RoutedEventArgs e) { Game.PassClicked = true; }

  }
}
