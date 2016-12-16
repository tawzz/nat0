using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ations
{
  public partial class WGame : Window
  {
    public Game Game { get; set; }
    public WGame()
    {
      Game = Game.GameInstance;
      InitializeComponent();
      Dispatcher.BeginInvoke(DispatcherPriority.Loaded,
        new Action(() =>
        {
          Console.WriteLine("Action Datacontext");
          DataContext = Game;
          Game.NextButton = bNext;
          Game.RoundMarkerUI = elRoundMarker;
          //Game.
          //Game.SetupGrowthAnimation(gRes);
        }));
    }
    void DisableUI() { bNext.IsEnabled = false; }
    private void OnClickNext(object sender, RoutedEventArgs e) { DisableUI(); Game.Kickoff(); }
    private void OnClickUndo(object sender, RoutedEventArgs e) { Game.Message = "Not Implemented"; }// Game.Reset(); }
    private void OnMagnifyClose(object sender, RoutedEventArgs e) { cardMagnifier.IsOpen = false; }

    #region drag drop
    private bool isDragging = false;
    private Point mouseOffset;
    Card card = null;
    UCard ui = null;
    //dieses event einhaengen um dd zu aktivieren, aber dann muss man MouseUp anders machen! 
    private void OnProgressCardMouseDown(object sender, MouseButtonEventArgs e)
    {
      ui = sender as UCard;
      card = ui.DataContext as Card;
      //ddstart(ui, e);
    }

    private void ddstart(Control sender, MouseButtonEventArgs e)
    {
      isDragging = true;
      dducard.DataContext = card;
      dducard.Width = ui.ActualWidth;
      dducard.SetValue(Canvas.LeftProperty, ui.ActualLeft(this));
      dducard.SetValue(Canvas.TopProperty, ui.ActualTop(this));
      mouseOffset = e.GetPosition(ui);
      ui.Visibility = Visibility.Collapsed;
      ddcanvas.Visibility = Visibility.Visible;
      dducard.MouseMove += ddmove;
      dducard.MouseLeftButtonUp += ddend;
      dducard.CaptureMouse();
    }
    private void ddmove(object sender, MouseEventArgs e)
    {
      if (isDragging)
      {
        UCard card = (UCard)sender;
        Point point = e.GetPosition(ddcanvas);
        card.SetValue(Canvas.TopProperty, point.Y - mouseOffset.Y);
        card.SetValue(Canvas.LeftProperty, point.X - mouseOffset.X);
      }
    }
    private void ddend(object sender, MouseButtonEventArgs e)
    {
      if (isDragging)
      {
        UCard card = (UCard)sender;
        card.MouseMove -= ddmove;
        card.MouseLeftButtonUp -= ddend;
        card.ReleaseMouseCapture();
        ddcanvas.Visibility = Visibility.Collapsed;
        //either move the card (remove from progressboard, add to civ board in game!) or make card visible!
        // if this is a battle,war,golden card, should not be draggable!
        isDragging = false;
      }
    }
    #endregion

    #region UI tests
    //private void WGame_Loaded(object sender, RoutedEventArgs e) { }
    private void OnReset(object sender, RoutedEventArgs e) { Game.Reset(); }
    //private void OnRoundMarker(object sender, RoutedEventArgs e) { }
    //private void OnProgressCards(object sender, RoutedEventArgs e) { }
    //private void OnGrowth(object sender, RoutedEventArgs e) { }
    //private void OnNextPlayer(object sender, RoutedEventArgs e) { }
    //private void OnEvent(object sender, RoutedEventArgs e) { }
    private void BuyCommand_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
    {
      var game = DataContext as Game;
      e.CanExecute = true;
    }

    private void BuyCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
    {
      //TODO: drag drop cards
      var game = DataContext as Game;
      var x = (sender as FrameworkElement).DataContext;
      var card = (e.OriginalSource as FrameworkElement).DataContext as Card;
      //game.BuyCard(card);
      int n = 5;
    }

    //*********************************************************************************** koennte reaktivieren
    //private void OnDropOnCivBoard(object sender, DragEventArgs e)
    //{
    //  var data = e.Data.GetData(DataFormats.Text);
    //  var ucard = data as UCard;
    //  var xcard = Helpers.GetCardX(data as string, 1); //brauch ich nicht, ist schon in global card
    //  int n = 4;
    //}

    //private void gridMainMouseMove(object sender, MouseEventArgs e)
    //{
    //  move(sender, e);
    //}
    //private void ddpopupMouseMove(object sender, MouseEventArgs e)
    //{
    //  move(sender, e);
    //}
    //private void move(object sender, MouseEventArgs e)
    //{
    //  if (e.LeftButton == MouseButtonState.Pressed)
    //  {
    //    if (Mouse.OverrideCursor == Cursors.Hand)
    //    {
    //      var ddel = ddpopup;
    //      var mp = e.GetPosition(this);

    //      var y = mp.Y;
    //      var h = Application.Current.MainWindow.ActualHeight;
    //      var marginBottom = h - (y + ddel.ActualHeight + SystemParameters.CaptionHeight + SystemParameters.BorderWidth + 4);
    //      var x = mp.X;
    //      var w = Application.Current.MainWindow.ActualWidth;
    //      var marginRight = w - (x + ddel.ActualWidth + SystemParameters.CaptionWidth + SystemParameters.BorderWidth + 4);

    //      ddel.Margin = new Thickness(x, y, marginRight, marginBottom);

    //      Mouse.OverrideCursor = Cursors.Hand;
    //    }
    //  }
    //  else Mouse.OverrideCursor = Cursors.Arrow;
    //}

    //****************************************************************************************************
    //private void OnMouseMove(object sender, MouseEventArgs e)
    //{
    //  if (e.LeftButton == MouseButtonState.Pressed)
    //  {
    //    ddpopup.Width += 10;
    //    //ddpopup.Visibility = Visibility.Visible;
    //    //ttt.X += 20;
    //    //var rt = ddpopup.RenderTransform as TransformGroup;
    //    //var tt = rt.Children[0] as TranslateTransform;
    //    //tt.X++;// = e.GetPosition(sender as FrameworkElement).X;
    //    //tt.Y++;// = e.GetPosition(sender as FrameworkElement).Y;
    //    //var tt = new PropertyPath("RenderTransform.Children[0].ScaleY");
    //  }
    //}

    //private void TestUnselectResource(object sender, RoutedEventArgs e)
    //{
    //  //Game.SelectedResource = null;
    //  //MakeTransparent();
    //}
    //public void MakeTransparent()
    //{
    //  var pixelWidth = 40; // eye.Width;
    //  int pixelHeight = 40; // eye.Height;

    //  //var ah = eye.ActualHeight;
    //  //var aw = eye.ActualWidth;
    //  //if(pixelWidth == 0 || pixelHeight == 0) { pixelWidth = aw; pixelHeight = ah; }


    //  var Stride = pixelWidth * 4;
    //  BitmapSource imgSource = (BitmapSource)eye.Source;
    //  byte[] pixels = new byte[pixelHeight * Stride];
    //  imgSource.CopyPixels(pixels, Stride, 0);
    //  byte TransparentByte = byte.Parse("0");
    //  byte Byte255 = byte.Parse("255");
    //  int N = pixelWidth * pixelHeight;
    //  //Operate the pixels directly
    //  for (int i = 0; i < N; i++)
    //  {
    //    byte a = pixels[i * 4];
    //    byte b = pixels[i * 4 + 1];
    //    byte c = pixels[i * 4 + 2];
    //    byte d = pixels[i * 4 + 3];
    //    if (a == Byte255 && b == Byte255 && c == Byte255 && d == Byte255)
    //    {
    //      pixels[i * 4] = TransparentByte;
    //      pixels[i * 4 + 1] = TransparentByte;
    //      pixels[i * 4 + 2] = TransparentByte;
    //      pixels[i * 4 + 3] = TransparentByte;
    //    }
    //  }
    //  WriteableBitmap writeableBitmap = new WriteableBitmap(pixelWidth, pixelHeight, 96, 96,
    //      System.Windows.Media.PixelFormats.Pbgra32, BitmapPalettes.Halftone256Transparent);
    //  writeableBitmap.WritePixels(new Int32Rect(0, 0, pixelWidth, pixelHeight), pixels, Stride, 0);
    //  eye.Source = writeableBitmap;
    //}
    //bool isDragging = false;
    //protected override void OnMouseMove(MouseEventArgs e)
    //{
    //  base.OnMouseMove(e);
    //  if (e.LeftButton == MouseButtonState.Pressed)
    //  {

    //    //  if (!isDragging)
    //    //  {
    //    //    // Package the data.
    //    //    DataObject data = new DataObject();
    //    //    data.SetData(DataFormats.StringFormat, "archimedes");
    //    //    //data.SetData("Double", circleUI.Height);
    //    //    data.SetData("Object", this);

    //    //    // Inititate the drag-and-drop operation.
    //    //    isDragging = true;
    //    //    DragDrop.DoDragDrop(this, data, DragDropEffects.Copy | DragDropEffects.Move);
    //    //  }
    //    //  else
    //    ttt.X = e.GetPosition(this).X;
    //    ttt.Y=e.GetPosition(this).Y;
    //  }
    //}
    //protected override void OnDrop(DragEventArgs e)
    //{
    //  base.OnDrop(e);

    //  // If the DataObject contains string data, extract it.
    //  if (e.Data.GetDataPresent(DataFormats.StringFormat))
    //  {
    //    string dataString = (string)e.Data.GetData(DataFormats.StringFormat);
    //    Game.AddCardToCivboard(dataString);
    //    //// If the string can be converted into a Brush, 
    //    //// convert it and apply it to the ellipse.
    //    //BrushConverter converter = new BrushConverter();
    //    //if (converter.IsValid(dataString))
    //    //{
    //    //  Brush newFill = (Brush)converter.ConvertFromString(dataString);
    //    //  circleUI.Fill = newFill;

    //    //  // Set Effects to notify the drag source what effect
    //    //  // the drag-and-drop operation had.
    //    //  // (Copy if CTRL is pressed; otherwise, move.)
    //    //  if (e.KeyStates.HasFlag(DragDropKeyStates.ControlKey))
    //    //  {
    //    //    e.Effects = DragDropEffects.Copy;
    //    //  }
    //    //  else
    //    //  {
    //    //    e.Effects = DragDropEffects.Move;
    //    //  }
    //    //}
    //  }
    //  e.Handled = true;
    //}
    #endregion

    private void OnProgressCardMouseUp(object sender, MouseButtonEventArgs e)
    {
      Game.BuyCard((sender as FrameworkElement).DataContext as Card);
    }

    private void OnResNumUpdated(object sender, System.Windows.Data.DataTransferEventArgs e)
    {
      // number text block has been updated, now I got the ui for tb 
      // now I coult tell game to fire storyboard on that ui!
      var ui = sender as Grid;
      var sp = ui.Parent as StackPanel;
      //bNext.
      //hier muss ich ui freezen!!! indem ich all ui interaction nicht mehr weiterleite!
      DisableUI();
      Game.AniResourceUpdate(sp);
    }

    private void OnRoundMarkerPositionUpdated(object sender, System.Windows.Data.DataTransferEventArgs e)
    {
      if (Game.Round >= 0)
      {
        DisableUI();
        var ui = sender as Ellipse;
        Game.AniRoundMarker(ui);
      }
    }

    private void OnCivCardMouseUp(object sender, MouseButtonEventArgs e)
    {
      Game.PlaceCard((sender as FrameworkElement).DataContext as Card);
    }

    private void OnCivCardMouseDown(object sender, MouseButtonEventArgs e)
    {

    }
  }
}
