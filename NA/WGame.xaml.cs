using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using ations.MA;

namespace ations
{
  public partial class WGame : Window
  {
    #region properties and constructor
    public VM VM { get { return (Application.Current as App).VM; } }
    public Game Game { get { return VM.Game; } }
    public WGame()
    {
      //Game = GameInst;
      InitializeComponent();
      Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
      {
        //DataContext = Game;
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

    private void OnMagnifyClose(object sender, RoutedEventArgs e)    {      cardMagnifier.IsOpen = false;    }

    private void OnClickMultiResource(object sender, MouseButtonEventArgs e)    {      Game.OnClickResourceInMultiPicker((sender as FrameworkElement).DataContext as Res);    }

    private void OnClickWorkerOnCivCard(object sender, MouseButtonEventArgs e)    {      Game.OnClickWorkerCounter((sender as FrameworkElement).DataContext as Field);    }

    private void OnClickNextTest(object sender, RoutedEventArgs e)    {      Game.Tests.NextTestRequested();    }
    private void OnClickRepeatTest(object sender, RoutedEventArgs e) { Game.Tests.RepeatRequested(); }
    private void OnClickPlayMode(object sender, RoutedEventArgs e) { Game.Tests.PlayModeRequested(); }

    private void OnClickTesting(object sender, RoutedEventArgs e)    {      Game.Message = "no current test function";    }


    private void CalculatePosition(object sender, DependencyPropertyChangedEventArgs e)
    {
      var left = gRound.ActualLeft(this);
      var bottom = gButtons.ActualTop(this); ;// roundMessage.ActualTop(this);
      var right = gRound.ActualWidth + left;

      gCardPicker.Margin = new Thickness(left - 200, 0, 0, 0);
      gCardPicker.Width = gRound.ActualWidth+400;
      gCardPicker.Height = bottom;
    }

    private void OnClickRedeal(object sender, RoutedEventArgs e)
    {
      Game.Progress.Deal();
    }

    private void OnClickAge1(object sender, RoutedEventArgs e)
    {
      Game.Stats.UpdateRound(1);Game.Stats.UpdateAge(); Game.Progress.Deal();
    }

    private void OnClickAge2(object sender, RoutedEventArgs e)
    {
      Game.Stats.UpdateRound(2); Game.Stats.UpdateAge(); Game.Progress.Deal();
    }

    private void OnClickSpecialOption(object sender, RoutedEventArgs e)
    {
      Game.OnClickSpecialOption((sender as FrameworkElement).DataContext as Choice);
    }
    private void OnClickSave(object sender, RoutedEventArgs e) { State.SaveGame(this, Game); }

    private async void OnClickLoad(object sender, RoutedEventArgs e) { await Game.InterruptGame(); }// Game =  State.LoadGame(this); }
  }
}
