using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
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
        }));
    }
    #region UI tests
    //private void WGame_Loaded(object sender, RoutedEventArgs e) { }
    private void OnReset(object sender, RoutedEventArgs e) { Game.Reset(); }
    //private void OnRoundMarker(object sender, RoutedEventArgs e) { }
    //private void OnProgressCards(object sender, RoutedEventArgs e) { }
    //private void OnGrowth(object sender, RoutedEventArgs e) { }
    //private void OnNextPlayer(object sender, RoutedEventArgs e) { }
    //private void OnEvent(object sender, RoutedEventArgs e) { }

    #endregion
    private void OnClickNext(object sender, RoutedEventArgs e)
    {
      Game.Kickoff();
    }
    private void OnClickUndo(object sender, RoutedEventArgs e)
    {
      //MakeTransparent();
      Game.Reset(); 
    }


    private void OnMagnifyClose(object sender, RoutedEventArgs e)
    {
      pup.IsOpen = false;
    }

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
  }
}
