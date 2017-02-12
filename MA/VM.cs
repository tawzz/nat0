using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace ations.MA
{
  public class VMDesignTime { public VM VM { get { return new VM(); } } }

  public class VM : VMObject
  {
    public Game Game { get { return game; } set { if (game != value) { game = value; NotifyPropertyChanged(); } } }
    Game game;
    public bool ShowScore { get { return showScore; } set { if (showScore != value) { showScore = value; NotifyPropertyChanged(); } } }
    bool showScore;

    //public int WinHeight { get { return Settings.GetInt("WinHeight"); } set { if (Settings.SetInt("WinHeight", value)) NotifyPropertyChanged(); } }
    //public int WinHeight { get { return winHeight; } set { if (winHeight != value) { winHeight = value; NotifyPropertyChanged(); } } }
    //int winHeight = 800;
    //public int WinWidth { get { return Settings.GetInt("WinWidth"); } set { if (Settings.SetInt("WinWidth", value)) NotifyPropertyChanged(); } }
    //public int WinLeft { get { return Settings.GetInt("WinLeft"); } set { if (Settings.SetInt("WinLeft", value)) NotifyPropertyChanged(); } }
    //public int WinTop { get { return Settings.GetInt("WinTop"); } set { if (Settings.SetInt("WinTop", value)) NotifyPropertyChanged(); } }

    //  public string Name { get; set; }
    public VM()
    {
      Name = "Nations";
      Settings.Initialize();
      Game = new Game();
      // restore game from saved game?
    }


  }

  public class VMObject : DependencyObject, INotifyPropertyChanged, IXmlSerializable
  {
    public Brush Brush { get; set; }
    public String Name { get; set; }

    public void WriteXml(XmlWriter writer)
    {
      writer.WriteElementString("Val", "Title");
    }

    public void ReadXml(XmlReader reader)
    {
      if (reader.MoveToContent() == XmlNodeType.Element)// && reader.LocalName == "Event")
      {
        var Title = reader["Val"];
        reader.Read();
      }
    }
    public XmlSchema GetSchema() { return (null); }


    public event PropertyChangedEventHandler PropertyChanged; public void NotifyPropertyChanged([CallerMemberName] string propertyName = null) { this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }

  }
}
