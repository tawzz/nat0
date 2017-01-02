using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ations;
using System.Xml.Linq;
using System.Diagnostics;

namespace ations
{
  public partial class Game
  {
    static Dictionary<string, Func<string, IEnumerable<Player>, Player[]>> dPred = new Dictionary<string, Func<string, IEnumerable<Player>, Player[]>>()
    {
      {"least", new Func<string, IEnumerable<Player>, Player[]>((s,pls) =>
      {
        var bound = pls.Min(x => x.Res.n(s));
        var resname = s;
        var nplayers = pls.Count();
        var partition = Split(resname, bound, pls);
        var result = partition.Item2;
        return Split(s, pls.Min(x => x.Res.n(s)), pls).Item2;
      }) },
      {"most", new Func<string, IEnumerable<Player>, Player[]>((s,pls) =>
      {
        var bound = pls.Max(x => x.Res.n(s));
        var resname = s;
        var nplayers = pls.Count();
        var partition = Split(resname, bound, pls);
        var result = partition.Item2;
        if (result.Length>1) result = new Player[] { };
        return result; // Split(s, pl.Min(x => x.Res.n(s)), pl).Item3;
      }) },
      {"not_least", new Func<string, IEnumerable<Player>, Player[]>((s,pls) =>
      {
        //var least = dPred["least"].Invoke(s,pl);
        var notleast = pls.Except(dPred["least"].Invoke(s,pls)).ToArray();
        return notleast;
        //var nleast=pl.Where(x)
        //var bound = pl.Max(x => x.Res.n(s));
        //var resname = s;
        //var nplayers = pl.Count();
        //var partition = Split(resname, bound, pl);
        //var result = partition.Item2;
        //return Split(s, pl.Max(x => x.Res.n(s)), pl).Item2;
      }) },
      {"not_most", new Func<string, IEnumerable<Player>, Player[]>((s,pls) =>
      {
        var notmost= pls.Except(dPred["most"].Invoke(s,pls)).ToArray();
        return notmost;
        //var bound = pls.Max(x => x.Res.n(s));
        //var resname = s;
        //var nplayers = pls.Count();
        //var partition = Split(resname, bound, pls);
        //var result = partition.Item1;
        //if (result.Length>1) result = new Player[] { };
        //return result; // Split(s, pl.Max(x => x.Res.n(s)), pl).Item1;
      }) },
      {"greater_than_war", new Func<string, IEnumerable<Player>, Player[]>((s,pls) =>
      {
        var game = Game.GameInstance;
        var warlevel = game.Stats.WarLevel;
        var isWar = game.Stats.IsWar;
        if (!isWar) return new Player[] { };
        else return pls.Where(x=>x.Military>warlevel).ToArray();
      }) },
      {"bought", new Func<string, IEnumerable<Player>, Player[]>((s,pls) =>
      {
        return pls.Where(x=>dCounters["nbought"].Invoke(s,x)>0).ToArray();
      }) },
    };
    static Dictionary<string, Func<string, Player, int>> dCounters = new Dictionary<string, Func<string, Player, int>>()
    {
      {"nbought", new Func<string, Player, int>((s,pl) =>
      {
        var n= pl.CardsBoughtThisRound.Where(x=>x.Type == s).Count();
        return n;
      }) },
    };
    static Dictionary<string, Action<Game, Player>> dEffect = new Dictionary<string, Action<Game, Player>>()
    {
      {"regain_war_loss",new Action<Game,Player>((game,pl)=> {
        if (pl.Military < game.Stats.WarLevel)
        {
          foreach(var res in pl.WarLoss)
          {
            pl.UpdateResBy(res.Name,res.Num);// warloss muss berechnet werden!!!
          }
        }
      }) },
      {"go_first",new Action<Game,Player>((game,pl)=> {
        var others = game.Players.Where(x=>x!=pl).ToList();
        List<Player> newlist = new List<Player>();
        newlist.Add(pl);
        newlist.AddRange(others);
        game.Players.Clear();
        foreach(var p in newlist)game.Players.Add(p);
      }) },
    };
    //actions
    //static Dictionary<string, Action<IEnumerable<Player>, string, int>> act4 = new Dictionary<string, Action<IEnumerable<Player>, string, int>>()
    //{
    //  {":res",new Action<IEnumerable<Player>, string,int>((pl,s,i)=> { foreach (var p in pl) p.res(s).Num += i; }) },
    //};




    //helpers
    public static List<Res> GetResourcesForRule(XElement xel)
    {
      List<string> exceptions = new List<string> { "pred", "param", "text", "min", "effect", "foreach", "func" };
      List<Res> result = new List<Res>();
      var reslist = xel.Attributes().Where(x => !exceptions.Contains(x.Name.ToString())).ToList();
      foreach (var attr in reslist)
      {
        var name = attr.Name.ToString();
        int n = 0;
        var ok = int.TryParse(attr.Value, out n);
        Debug.Assert(ok, "GetResourcesForRule: Unparsable attribute value for resource " + name);
        result.Add(new Res(name, n));
      }
      return result;
    }
    public static Action<Game, Player> GetSpecialEffect(XElement xel)
    {
      var specialEffect = xel.astring("effect");
      Action<Game, Player> effectAction = null;
      if (!string.IsNullOrEmpty(specialEffect) && dEffect.ContainsKey(specialEffect)) { effectAction = dEffect[specialEffect]; }
      return effectAction;
    }
    public static Tuple<Player[], Player[], Player[]> Split(string res, int bound, IEnumerable<Player> pl)
    {
      // returns players with smaller,exactly,greater Res.n(res)
      var smaller = pl.Where(x => x.Res.n(res) < bound).ToArray();
      var same = pl.Where(x => x.Res.n(res) == bound).ToArray();
      var greater = pl.Where(x => x.Res.n(res) > bound).ToArray();

      return new Tuple<Player[], Player[], Player[]>(
        pl.Where(x => x.Res.n(res) < bound).ToArray(),
        pl.Where(x => x.Res.n(res) == bound).ToArray(),
        pl.Where(x => x.Res.n(res) > bound).ToArray());
    }
  }
}
