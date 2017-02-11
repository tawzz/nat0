using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ations
{
  public class State_old
  {
    public GameState_old RoundBeginState { get; set; }
    public GameState_old ActionBeginState { get; set; }
    public GameState_old ActionEndState { get; set; }

    public void RoundBegin() { RoundBeginState = GameState_old.Record(); }
    public void ActionBegin() { ActionBeginState = GameState_old.Record(); }
    public void ActionEnd()
    {
      ActionEndState = GameState_old.Record();

      // calculate diff to state at action begin
      var diffState = GameState_old.CalcDifference(ActionBeginState, ActionEndState);

      // check cards for any events triggered by state changes
      //Checker.CheckStateChangeTriggeredEvents();
    }

  }
  public class GameState_old
  {
    public Dictionary<Player, PlayerState_old> PlayerStates { get; set; }

    public static GameState_old Record()
    {
      var state = new GameState_old();
      state.PlayerStates = new Dictionary<Player, PlayerState_old>();
      foreach (var pl in Game.Inst.Players) state.PlayerStates.Add(pl, PlayerState_old.Record(pl));
      return state;

    }

    public static GameState_old CalcDifference(GameState_old s1, GameState_old s2)
    {
      var diff = new GameState_old();
      diff.PlayerStates = new Dictionary<Player, PlayerState_old>();
      foreach (var pl in Game.Inst.Players) diff.PlayerStates.Add(pl, PlayerState_old.CalcDifference(s1.PlayerStates[pl], s2.PlayerStates[pl]));
      return diff;
    }
  }
  public class PlayerState_old
  {
    public List<Field> Fields { get; set; } //cardname,... [ev. isLargeField wegen dyn]
    public List<Card> Cards { get; set; } // cardname,numdeployed, [actiontaken,listofresources,tag]for actionbegin state
    public Dictionary<string, int> Resources { get; set; } //resname,resnum foreach Res

    public static PlayerState_old Record(Player pl)
    {
      var state = new PlayerState_old();
      state.Cards = pl.Cards.ToList();
      state.Resources = pl.GetResourceSnapshot();

      state.Fields = new List<Field>();
      foreach(var f in pl.Civ.Fields)
      {

      }
      return state;
    }
    public static PlayerState_old CalcDifference(PlayerState_old s1, PlayerState_old s2)
    {
      var diff = new PlayerState_old();
      diff.Cards = s2.Cards.Where(x => !s1.Cards.Contains(x)).ToList();
      diff.Resources = Player.GetResDiff(s1.Resources, s2.Resources);
      return diff;
    }
  }
}
