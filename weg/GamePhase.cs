using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ations
{
  public enum Phases { starting, roundmarker, progresscards, growth, newevent, architects, action, production, playerorder, war, events, famine, scoring, gameover }
  
  public class GamePhase : INotifyPropertyChanged
  {
    public static string[] Names = { "Nations", "Update Age", "Deal", "Growth", "New Event", "Architects", "ACTION!!!", "Production", "Player Order", "War", "Events", "Famine", "Scoring", "Game over" };
    public string Name { get; set; }
    public Phases Type { get; set; }
    public string ButtonText { get { return buttonText; } set { buttonText = value; NotifyPropertyChanged(); } }
    string buttonText;
    public string MessageTodo { get { return messageTodo; } set { messageTodo = value; NotifyPropertyChanged(); } }
    string messageTodo;
    public bool IsCompleted { get { return isCompleted; } set { isCompleted = value; NotifyPropertyChanged(); } }
    bool isCompleted;

    public Action NextKickoffAction { get; set; } // aendert sich fuer jedes prompt
    public Action PlayerChangeKickoffAction { get; set; } // stays same for each phase
    public Action<FrameworkElement> AnimationKickoffAction { get; set; }  //brauch ich das?!?
    public Action<object,object> AniCompletedAction { get; set; } 
    public Action SetupNextGamePhaseAction { get; set; }

    public GamePhase(Phases type, string cmd, string msg, 
      Action koNext, Action koPlayer, Action<FrameworkElement> koAni, Action<object,object> anicompl, Action setupNextPhase)
    {
      Type = type; Name = Names[(int)type]; ButtonText = cmd; MessageTodo = msg;
      NextKickoffAction = koNext;
      PlayerChangeKickoffAction = koPlayer;
      AnimationKickoffAction = koAni;
      AniCompletedAction = anicompl;
      SetupNextGamePhaseAction = setupNextPhase;
    }




    public event PropertyChangedEventHandler PropertyChanged; public virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null) { this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
    public override string ToString()
    {
      return Name + ", " + Type + " compl:" + (IsCompleted ? "YES" : "NO") + " " + ButtonText.ToCapital();
    }
  }
}
