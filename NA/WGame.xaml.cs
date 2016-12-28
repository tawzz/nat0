using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace ations
{
  public partial class WGame : Window
  {
    #region properties and constructor
    public Game Game { get; set; }
    public WGame()
    {
      Game = Game.GameInstance;
      InitializeComponent();
      Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
      {
        DataContext = Game;
        //Game.UIDispatcher = Dispatcher;
        Game.UIRoundmarker = elRoundMarker; // some ui elements needed for animations triggered in gameloop
      }));
    }
    #endregion
    private void OnResNumUpdated(object sender, DataTransferEventArgs e) { Game.ResourceUpdated((sender as Grid).Parent as StackPanel); }
    private void OnNumDeployedUpdated(object sender, DataTransferEventArgs e) { Game.NumDeployedUpdated((sender as TextBlock).Parent as Grid); }

    private void OnClickButton(object sender, RoutedEventArgs e) { if (!Game.IsRunning) Game.Kickoff(); else Game.OkStartClicked = true; }
    private void OnClickPass(object sender, RoutedEventArgs e) { Game.PassClicked = true; }
    private void OnClickSTOP(object sender, RoutedEventArgs e) { Game.Interrupt = true; }
    private void OnClickCancel(object sender, RoutedEventArgs e) { Game.CancelClicked = true; }

    private void OnClickProgressCard(object sender, RoutedEventArgs e) { Game.OnClickProgressField((sender as FrameworkElement).DataContext as Field); }
    private void OnClickCivCard(object sender, RoutedEventArgs e) { Game.OnClickCivField((sender as FrameworkElement).DataContext as Field); }
    private void OnClickArchitects(object sender, MouseButtonEventArgs e) { Game.OnClickArchitect(); }
    private void OnClickTurmoils(object sender, MouseButtonEventArgs e) { Game.OnClickTurmoil(); }
    private void OnClickCivResource(object sender, RoutedEventArgs e) { Game.OnClickCivResource((sender as FrameworkElement).DataContext as Res); }

    private void OnMagnifyClose(object sender, RoutedEventArgs e)
    {
      cardMagnifier.IsOpen = false;
    }
  }
}
