﻿using System;
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
    #region properties and contructor
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
    #region helpers
    #endregion
    //#region handlers version 0

    //private void OnClickRedeal(object sender, RoutedEventArgs e) { Game.Progress.Deal(); }
    //private void OnClickUndo(object sender, RoutedEventArgs e) { }
    //private void OnClickBuy(object sender, RoutedEventArgs e)
    //{
    //  var selplace = GetSelectedRadio(icCivcards);
    //  var selbuy = GetSelectedRadio(icProgress);

    //  if (selbuy != null && selplace != null)
    //  {
    //    selbuy.IsChecked = selplace.IsChecked = false;
    //    Game.Buy0(selbuy.DataContext as AField, selplace.DataContext as AField);
    //  }
    //  else if (selbuy != null && Game.IsUnambiguousBuy0(selbuy.DataContext as AField))
    //  {
    //    selbuy.IsChecked = false;
    //    Game.Buy0(selbuy.DataContext as AField);
    //  }
    //  else if (selbuy != null) Game.Message = "select a place on civ board!";
    //}
    //private void OnClickNext(object sender, RoutedEventArgs e) { }

    //private void OnClickPick(object sender, RoutedEventArgs e) { Game.OnPick(); }
    //private void OnClickPickWorker(object sender, RoutedEventArgs e) { Game.OnPickWorker(); }
    //private void OnClickStart(object sender, RoutedEventArgs e) { Game.Kickoff(); }
    //private void OnClickPrompt(object sender, RoutedEventArgs e) { }
    //#endregion
    private void OnResNumUpdated(object sender, DataTransferEventArgs e) { Game.ResourceUpdated((sender as Grid).Parent as StackPanel); }
    private void OnNumDeployedUpdated(object sender, DataTransferEventArgs e) { Game.NumDeployedUpdated((sender as TextBlock).Parent as Grid); }

    private void OnClickArchitects(object sender, MouseButtonEventArgs e) { Game.OnClickArchitects(); }
    private void OnClickTurmoils(object sender, MouseButtonEventArgs e) { Game.OnClickTurmoils(); }
    //ToggleButton sel = null;
    private void OnClickProgressCard(object sender, RoutedEventArgs e)
    {//hack!!!
      //var b = (sender as ToggleButton);
      //if (sel == b && sel.IsChecked == true) { sel.IsChecked = false; var fld = sel.DataContext as AField; if (fld.Card != null) fld.Card.IsSelected = false; }

      //var unchecksel = sel.IsChecked == true;
      ////if (sel != null && sel.IsChecked==true) { sel.IsChecked = false; var fld = sel.DataContext as AField; if (fld.Card !=null) fld.Card.IsSelected = false; }
      //if (sel != b || (sel == b && sel.IsChecked == false))
      //{
      //  b.IsChecked = true; var field = b.DataContext as AField; field.Card.IsSelected = true;
      //  sel = b;
        Game.OnClickProgressCard((sender as FrameworkElement).DataContext as Field);
      //}
      
    }
    private void OnClickCivCard(object sender, RoutedEventArgs e) { Game.OnClickCivCard((sender as FrameworkElement).DataContext as Field); }

    private void OnClickButton(object sender, RoutedEventArgs e) { if (!Game.IsRunning) Game.Kickoff(); else Game.OkStartClicked = true; }
    private void OnClickSTOP(object sender, RoutedEventArgs e) { Game.Interrupt = true; }
    private void OnClickCancel(object sender, RoutedEventArgs e) { Game.CancelClicked = true; }
    private void OnClickPass(object sender, RoutedEventArgs e) { Game.PassClicked = true; }

    //private void OnClickResource(object sender, MouseButtonEventArgs e)
    //{
    //  var res = (sender as FrameworkElement).DataContext as ARes;
    //  Debug.Assert(res != null, "OnClickResource with resource==null! (AWGame.xaml.cs)");
    //  if (res.IsSelectable) Game.OnClickWorker(res);
    //}

    private void OnClickResource(object sender, RoutedEventArgs e)
    {
      var res = (sender as FrameworkElement).DataContext as Res;
      Debug.Assert(res != null, "OnClickResource with resource==null! (AWGame.xaml.cs)");
      if (res.IsSelectable) Game.OnClickWorker(res);
    }

    Choice lastChoice;
    private void OnChoiceSelected(object sender, SelectionChangedEventArgs e)
    {
      var newchoice = (sender as ListBox).SelectedItem as Choice;
      if (newchoice != null)
      {
        if (lastChoice != null) lastChoice.IsSelected = false;
        newchoice.IsSelected = true;
        Game.SelectedChoice = lastChoice = newchoice;

      }
    }
  }
}
