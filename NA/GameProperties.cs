using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml.Linq;

namespace ations
{
  public partial class Game
  {
    #region properties

    public int NumPlayers { get; set; }
    public ObservableCollection<Player> Players { get; set; }
    public List<Player> PassOrder { get; set; }
    public Player MainPlayer
    {
      get { return mainPlayer; }
      set
      {
        if (mainPlayer != value)
        {
          var oldmain = mainPlayer;
          mainPlayer = value;
          Message = "Hi, " + mainPlayer.Name + "!"; //testing!
          if (oldmain != null) oldmain.IsMainPlayer = false;
          if (mainPlayer != null) mainPlayer.IsMainPlayer = true;
          NotifyPropertyChanged();
        }
      }
    }
    Player mainPlayer;
    public Progress Progress { get; set; }
    public Stats Stats { get; set; }
    public State State { get; set; }

    public string Description { get { return description; } set { description = value; NotifyPropertyChanged(); } }
    string description;
    public string Message { get { return message; } set { message = value; NotifyPropertyChanged(); } }
    string message;
    public string LongMessage { get { return longMessage; } set { longMessage = value; NotifyPropertyChanged(); } }
    string longMessage;
    public string Caption { get { return caption; } set { caption = value; NotifyPropertyChanged(); } }
    string caption = "Start";
    public string RightCaption { get { return rightCaption; } set { rightCaption = value; NotifyPropertyChanged(); } }
    string rightCaption = "Pass";
    public string LeftCaption { get { return leftCaption; } set { leftCaption = value; NotifyPropertyChanged(); } }
    string leftCaption = "Cancel";
    public string Title { get { return title; } set { title = value; NotifyPropertyChanged(); } }
    string title;

    public bool ShowScore { get { return showScore; } set { if (showScore != value) { showScore = value; foreach (var pl in Players) pl.ShowScore = value; NotifyPropertyChanged(); } } }
    bool showScore;
    public bool ShowResChoices { get { return showResChoices; } set { if (showResChoices != value) { showResChoices = value; NotifyPropertyChanged(); } } }
    bool showResChoices;
    public bool ShowChangesInResources { get { return showProduction; } set { if (showProduction != value) { showProduction = value; NotifyPropertyChanged(); } } }
    bool showProduction;
    public bool ShowMultiResChoices { get { return showMultiResChoices; } set { if (showMultiResChoices != value) { showMultiResChoices = value; NotifyPropertyChanged(); } } }
    bool showMultiResChoices;
    public int Number { get; set; }
    public bool ShowChoices { get { return showChoices; } set { showChoices = value; NotifyPropertyChanged(); } }
    bool showChoices;
    public bool ShowCardChoices { get { return showCardChoices; } set { showCardChoices = value; NotifyPropertyChanged(); } }
    bool showCardChoices;

    public bool Interrupt { get; set; }
    public bool CanInitialize = false; // signalize interrupt completed

    public bool IsRunning { get; private set; }
    public Action Kickoff { get; private set; }
    public List<Storyboard> AnimationQueue { get; set; }
    public FrameworkElement UIRoundmarker { get; set; }

    public bool AllPlayersPassed { get { return Players.All(x=>x.HasPassed); } }
    public bool NoChangeInTurn { get; set; }


    #endregion
  }
}
