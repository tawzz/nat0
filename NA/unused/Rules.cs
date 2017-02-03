using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ations;

namespace ations
{
  public class Rules
  {
    public static Dictionary<string, Func<string, IEnumerable<Player>, Player[]>> Selector_old = new Dictionary<string, Func<string, IEnumerable<Player>, Player[]>>()
    {
      {"least", new Func<string, IEnumerable<Player>, Player[]>((s,pls) => { return Checker.PlayersWith("least",s,pls).ToArray(); }) },
      {"most", new Func<string, IEnumerable<Player>, Player[]>((s,pls) => { return Checker.PlayersWith("most",s,pls).ToArray(); }) },
      {"not_least", new Func<string, IEnumerable<Player>, Player[]>((s,pls) => { return Checker.PlayersWith("not_least",s,pls).ToArray(); }) },
      {"not_most", new Func<string, IEnumerable<Player>, Player[]>((s,pls) => { return Checker.PlayersWith("not_most",s,pls).ToArray(); }) },
      {"pass_first", new Func<string, IEnumerable<Player>, Player[]>((s,pls) =>
      {
        var game = Game.Inst;
        Debug.Assert(game.PassOrder.Count > 0,"pass_first: PassOrder has no elements!");
        var result = new Player[] { Game.Inst.PassOrder.First() };
        return result;
      }) },
      {"pass_last", new Func<string, IEnumerable<Player>, Player[]>((s,pls) =>
      {
        var game = Game.Inst;
        Debug.Assert(game.PassOrder.Count > 0,"pass_last: PassOrder has no elements!");
        var result = new Player[] { Game.Inst.PassOrder.Last() };
        return result;
      }) },
      {"greater_than_war", new Func<string, IEnumerable<Player>, Player[]>((s,pls) =>
      {
        var game = Game.Inst;
        var warlevel = game.Stats.WarLevel;
        var isWar = game.Stats.IsWar;
        if (!isWar) return new Player[] { };
        else return pls.Where(x=>x.Military>warlevel).ToArray();
      }) },
      {"bought", new Func<string, IEnumerable<Player>, Player[]>((s,pls) =>
      {
        return pls.ToArray();// pls.Where(x=>dCounters["nbought"].Invoke(s,x)>0).ToArray();
      }) },
    };

  }
}
