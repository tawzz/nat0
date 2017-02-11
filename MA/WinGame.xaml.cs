using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace ations.MA
{
  public partial class WinGame : Window
  {
    public VM VM { get { return (Application.Current as App).VM; } }

    public WinGame()
    {
      InitializeComponent();
      LoadSettings();
    }
    #region load save closing
    protected override void OnClosing(CancelEventArgs e)
    {
      base.OnClosing(e);
      if (Settings.GetBool("Autosave", true)) SaveAll();
    }
    public void SaveAll()
    {
      SaveSettings();
      // Save game
    }
    public void LoadSettings()
    {
      this.Height = Settings.GetInt("WinHeight", 800);
      this.Width = Settings.GetInt("WinWidth", 1400);
      this.Left = Settings.GetInt("WinLeft", 200);
      this.Top = Settings.GetInt("WinTop", 100);
    }
    public void SaveSettings()
    {
      Settings.SetInt("WinHeight", (int)this.ActualHeight);
      Settings.SetInt("WinWidth", (int)this.ActualWidth);
      Settings.SetInt("WinLeft", (int)this.GetWindowLeft());
      Settings.SetInt("WinTop", (int)this.GetWindowTop());
      Settings.Save();
    }
    private void OnClickSaveSettings(object sender, RoutedEventArgs e) { SaveAll(); }
    #endregion
  }
}
