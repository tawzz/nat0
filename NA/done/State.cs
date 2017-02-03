using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ations
{
  public class State
  {
    public GameState RoundBeginState { get; set; }
    public GameState ActionBeginState { get; set; }
    public GameState ActionEndState { get; set; }

    public void RoundBegin() { RoundBeginState = GameState.Record(); }
    public void ActionBegin() { ActionBeginState = GameState.Record(); }
    public void ActionEnd()
    {
      ActionEndState = GameState.Record();

      // calculate diff to state at action begin
      var diffState = GameState.CalcDifference(ActionBeginState, ActionEndState);

      // check cards for any events triggered by state changes
      //Checker.CheckStateChangeTriggeredEvents();
    }

  }
  public class GameState
  {
    public Dictionary<Player, PlayerState> PlayerStates { get; set; }

    public static GameState Record()
    {
      var state = new GameState();
      state.PlayerStates = new Dictionary<Player, PlayerState>();
      foreach (var pl in Game.Inst.Players) state.PlayerStates.Add(pl, PlayerState.Record(pl));
      return state;

    }

    public static GameState CalcDifference(GameState s1, GameState s2)
    {
      var diff = new GameState();
      diff.PlayerStates = new Dictionary<Player, PlayerState>();
      foreach (var pl in Game.Inst.Players) diff.PlayerStates.Add(pl, PlayerState.CalcDifference(s1.PlayerStates[pl], s2.PlayerStates[pl]));
      return diff;
    }
  }
  public class PlayerState
  {
    public List<Card> Cards { get; set; }
    public Dictionary<string, int> Resources { get; set; }

    public static PlayerState Record(Player pl)
    {
      var state = new PlayerState();
      state.Cards = pl.Cards.ToList();
      state.Resources = pl.GetResourceSnapshot();
      return state;
    }
    public static PlayerState CalcDifference(PlayerState s1, PlayerState s2)
    {
      var diff = new PlayerState();
      diff.Cards = s2.Cards.Where(x => !s1.Cards.Contains(x)).ToList();
      diff.Resources = Player.GetResDiff(s1.Resources, s2.Resources);
      return diff;
    }
  }
}
