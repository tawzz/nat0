using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using System.Xml.Serialization;
using Microsoft.Win32;

namespace ations
{
  public class State
  {
    const string strFilter = "game XML files (*.gXml)|" +
                             "*.gXml|All files (*.*)|*.*";

    public static bool SaveGame(Window win, Game game)
    {
      SaveFileDialog dlg = new SaveFileDialog();
      dlg.Filter = strFilter;

      if ((bool)dlg.ShowDialog(win))
      {
        try
        {
          StreamWriter writer = new StreamWriter(dlg.FileName);
          XmlSerializer xml = new XmlSerializer(typeof(Game));
          xml.Serialize(writer, game);
          writer.Close();
        }
        catch (Exception exc)
        {
          MessageBox.Show("Could not save file: " + exc.Message,
                          win.Title, MessageBoxButton.OK,
                          MessageBoxImage.Exclamation);
          return false;
        }
      }
      return true;
    }

    //public static Game LoadGame(Window win)
    //{
    //  OpenFileDialog dlg = new OpenFileDialog();
    //  dlg.Filter = strFilter;
    //  Game people = null;

    //  if ((bool)dlg.ShowDialog(win))
    //  {
    //    try
    //    {
    //      StreamReader reader = new StreamReader(dlg.FileName);
    //      XmlSerializer xml = new XmlSerializer(typeof(Game));
    //      people = (Game)xml.Deserialize(reader);
    //      reader.Close();
    //    }
    //    catch (Exception exc)
    //    {
    //      MessageBox.Show("Could not load file: " + exc.Message,
    //                      win.Title, MessageBoxButton.OK,
    //                      MessageBoxImage.Exclamation);
    //      people = null;
    //    }
    //  }
    //  return people;
    //}















































    //public Stack<XElement> Rounds { get; set; }
    //public Stack<XElement> Actions { get; set; }
    //public XElement RoundState { get; set; } // current round state
    //public XElement ActionState { get; set; } // current action state

    //public void RoundBegin()
    //{
    //  // alles ueber current game state recorden was noetig um den state wieder herzustellen
    //  // zu round begin gibt es nur die progress cards, age, round, und den state der players aber noch kein event
    //  // wenn dieser state geladen wird, kann growth phase starten


    //}
    //public void ActionBegin() {  }
  }





















  //public class GameState
  //{
  //  public Dictionary<Player, XElement> PlayerStates { get; set; }

  //  public static GameState Record()
  //  {
  //    var state = new GameState();
  //    state.PlayerStates = new Dictionary<Player, PlayerState>();
  //    foreach (var pl in Game.Inst.Players) state.PlayerStates.Add(pl, PlayerState.Record(pl));
  //    return state;

  //  }

  //  public static GameState CalcDifference(GameState s1, GameState s2)
  //  {
  //    var diff = new GameState();
  //    diff.PlayerStates = new Dictionary<Player, PlayerState>();
  //    foreach (var pl in Game.Inst.Players) diff.PlayerStates.Add(pl, PlayerState.CalcDifference(s1.PlayerStates[pl], s2.PlayerStates[pl]));
  //    return diff;
  //  }
  //}
  //public class PlayerState
  //{
  //  public List<Field> Fields { get; set; } //cardname,... [ev. isLargeField wegen dyn]
  //  public List<Card> Cards { get; set; } // cardname,numdeployed, [actiontaken,listofresources,tag]for actionbegin state
  //  public Dictionary<string, int> Resources { get; set; } //resname,resnum foreach Res

  //  public static PlayerState Record(Player pl)
  //  {
  //    var state = new PlayerState();
  //    state.Cards = pl.Cards.ToList();
  //    state.Resources = pl.GetResourceSnapshot();

  //    state.Fields = new List<Field>();
  //    foreach(var f in pl.Civ.Fields)
  //    {

  //    }
  //    return state;
  //  }
  //  public static PlayerState CalcDifference(PlayerState s1, PlayerState s2)
  //  {
  //    var diff = new PlayerState();
  //    diff.Cards = s2.Cards.Where(x => !s1.Cards.Contains(x)).ToList();
  //    diff.Resources = Player.GetResDiff(s1.Resources, s2.Resources);
  //    return diff;
  //  }
  //}
}
