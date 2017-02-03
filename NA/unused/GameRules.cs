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
    delegate bool PredFunc_old(params object[] args);
    #region dPredicates
    static Dictionary<string, PredFunc_old> dPredicates_old = new Dictionary<string, PredFunc_old>()
    {
      {"cardprice", new PredFunc_old((oarr) => //usage: call(card,3); ////
      {
        var game = Game.Inst;
        var card = oarr[0] as Card;
        var cost = card.BasicCost;
        int n = (int)oarr[1];
        var result = cost == n;
        return result;
      }) },
      {"least", new PredFunc_old((oarr) => //usage: call("stability",pl,Players);
      {
        var game = Game.Inst;
        var resname = oarr[0] as string;
        var pl = oarr.Length>1?oarr[1] as Player:game.MainPlayer;
        var pls = oarr.Length>2?oarr[2] as IEnumerable<Player>:game.Players;
        var bound = pls.Min(x => x.Res.n(resname));
        var result = pl.Res.n(resname)<= bound;
        return result;
      }) },
      {"most", new PredFunc_old((oarr) =>
      {
        var game = Game.Inst;
        var resname = oarr[0] as string;
        var pl = oarr.Length>1?oarr[1] as Player:game.MainPlayer;
        var pls = oarr.Length>2?oarr[2] as IEnumerable<Player>:game.Players;
        var bound = pls.Max(x => x.Res.n(resname));
        var plsMost = pls.Where(x=>x.Res.n(resname) == bound).ToList();
        var result = plsMost.Count()==1 && plsMost.First() == pl;
        return result;
      }) },
      {"not_least", new PredFunc_old((oarr) =>
      {
        var game = Game.Inst;
        var resname = oarr[0] as string;
        var pl = oarr.Length>1?oarr[1] as Player:game.MainPlayer;
        var pls = oarr.Length>2?oarr[2] as IEnumerable<Player>:game.Players;
        var bound = pls.Min(x => x.Res.n(resname));
        var result = pl.Res.n(resname)>bound;
        return result;
      }) },
      {"not_most", new PredFunc_old((oarr) =>
      {
        var game = Game.Inst;
        var resname = oarr[0] as string;
        var pl = oarr.Length>1?oarr[1] as Player:game.MainPlayer;
        var pls = oarr.Length>2?oarr[2] as IEnumerable<Player>:game.Players;
        var bound = pls.Max(x => x.Res.n(resname));
        var result = pl.Res.n(resname) < bound;
        return result;
      }) },
      {"greater_than_war", new PredFunc_old((oarr) => //usage: call("military",pl);
      {
        var game = Game.Inst;
        var warlevel = game.Stats.WarLevel;
        var isWar = game.Stats.IsWar;
        if (!isWar) return false;
        var resname = oarr[0] as string;
        var pl = oarr.Length>1?oarr[1] as Player:game.MainPlayer;
        var result = pl.Res.n(resname)>warlevel;
        return result;
      }) },
      {"bought", new PredFunc_old((oarr) => //usage: call("military",pl,0);
      {
        var game = Game.Inst;
        var cardtype = oarr[0] as string;
        var pl = oarr.Length > 1 ? oarr[1] as Player : game.MainPlayer;
        var cardage = oarr.Length > 2 ? (int)oarr[2] : 0;
        var result = pl.CardsBoughtThisRound.Any(x=>(cardtype =="" || x.Type == cardtype) && (cardage == 0 || x.Age == cardage));
        return result;
      }) },
      {"deployed", new PredFunc_old((oarr) => //usage: dPredicates["deployed"].Invoke(new object[]{"military",pl,0});
      {
        var game = Game.Inst;
        var cardtype = oarr[0] as string;
        var pl = oarr.Length > 1 ? oarr[1] as Player : game.MainPlayer;
        var cardage = oarr.Length > 2 ? (int)oarr[2] : 0;
        var fields = pl.Civ.Fields.Where(x=>!x.IsEmpty && x.Card.NumDeployed > 0 && (cardtype =="" || x.Card.Type == cardtype) && (cardage == 0 || x.Card.Age == cardage));
        var result = fields.Count()>0;
        return result;
      }) },
      {"hasCard", new PredFunc_old((oarr) => //usage: dPredicates["hasCard"].Invoke(new object[]{"military",pl,0});
      {
        var game = Game.Inst;
        var cardtype = oarr[0] as string;
        var pl = oarr.Length > 1 ? oarr[1] as Player : game.MainPlayer;
        var cardage = oarr.Length > 2 ? (int)oarr[2] : 0;
        var fields = pl.Civ.Fields.Where(x=>!x.IsEmpty && (cardtype =="" || x.Card.Type == cardtype) && (cardage == 0 || x.Card.Age == cardage));
        var result = fields.Count()>0;
        return result;
      }) },
      {"cardtype", new PredFunc_old((oarr) => //usage: call(card,3);
      {
        var game = Game.Inst;
        Debug.Assert(oarr!=null && oarr.Any(x=>x is Card), "called cardtype predicate with not Card in params");
        var card = oarr.FirstOrDefault(x=>x is Card) as Card;
        var type = oarr.FirstOrDefault(x=>x is string) as string;
        var result = card.Type == type;
        return result;
      }) },
      {"pass_first",  new PredFunc_old((oarr) => //usage: call();
      {
        var game = Game.Inst;
        var pl = oarr.Length > 0 ? oarr[0] as Player: game.MainPlayer;
        Debug.Assert(game.PassOrder.Count > 0,"pass_first: PassOrder has no elements!");
        var result = pl == Game.Inst.PassOrder.First();
        return result;
      }) },
      {"pass_last",  new PredFunc_old((oarr) => //usage: call();
      {
        var game = Game.Inst;
        var pl = oarr.Length > 0 ? oarr[0] as Player: game.MainPlayer;
        Debug.Assert(game.PassOrder.Count > 0,"pass_last: PassOrder has no elements!");
        var result = pl == Game.Inst.PassOrder.Last();
        return result;
      }) },
    };
    #endregion

    #region dPlayerSelectors
    static Dictionary<string, Func<string, IEnumerable<Player>, Player[]>> dPlayerSelectors = new Dictionary<string, Func<string, IEnumerable<Player>, Player[]>>()
    {
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
        return result; 
      }) },
      {"not_least", new Func<string, IEnumerable<Player>, Player[]>((s,pls) =>
      {
        var not_least = pls.Except(dPlayerSelectors["least"].Invoke(s,pls)).ToArray();
        return not_least;
      }) },
      {"not_most", new Func<string, IEnumerable<Player>, Player[]>((s,pls) =>
      {
        var not_most= pls.Except(dPlayerSelectors["most"].Invoke(s,pls)).ToArray();
        return not_most;
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
        return pls.Where(x=>dCounters["nbought"].Invoke(s,x)>0).ToArray();
      }) },
    };

    #endregion

    #region dCounters, dCountersII

    static Dictionary<string, Func<string, Player, int>> dCounters = new Dictionary<string, Func<string, Player, int>>()
    {
      {"nbought", new Func<string, Player, int>((s,pl) =>
      {
        var n= pl.CardsBoughtThisRound.Where(x=>x.Type == s).Count();
        return n;
      }) },
    };
    static Dictionary<string, Func<object[], int>> dCountersII = new Dictionary<string, Func<object[], int>>()
    {
      {"nbought", new Func<object[], int>((oarr) =>//usage: dCounterII["nbought"].Invoke(new object[]{"type",{"military","building"},pl});
      {
        var attrname = oarr[0] as string;
        var attrvals = oarr[1] as string[];
        var pl = oarr[2] as Player;
        int n=0;
        foreach(var val in attrvals) {n+=pl.CardsBoughtThisRound.Where(x=>x.X.astring(attrname) == val).Count(); }
        return n;
      }) },
    };

    #endregion

    #region dEffect

    static Dictionary<string, Action<object[]>> dEffectII = new Dictionary<string, Action<object[]>>()
    {
      {"regain_war_loss",new Action<object[]>((oarr)=> {
        var game = Game.Inst;
        var pl = oarr!=null && oarr.Length>0? oarr[0] as Player:game.MainPlayer;
        if (pl.Military < game.Stats.WarLevel)
        {
          foreach(var res in pl.WarLoss)
          {
            pl.UpdateResBy(res.Name,res.Num);// warloss muss berechnet werden!!!
          }
        }
      }) },
      {"go_first",new Action<object[]>((oarr)=> {
        var game = Game.Inst;
        var pl = oarr!=null && oarr.Length>0? oarr[0] as Player:game.MainPlayer;
        var others = game.Players.Where(x=>x!=pl).ToList();
        List<Player> newlist = new List<Player>();
        newlist.Add(pl);
        newlist.AddRange(others);
        game.Players.Clear();
        foreach(var p in newlist)game.Players.Add(p);
      }) },
      {"go_last",new Action<object[]>((oarr)=> {
        var game = Game.Inst;
        var pl = oarr!=null && oarr.Length>0? oarr[0] as Player:game.MainPlayer;
        var others = game.Players.Where(x=>x!=pl).ToList();
        List<Player> newlist = new List<Player>();
        newlist.AddRange(others);
        newlist.Add(pl);
        game.Players.Clear();
        foreach(var p in newlist)game.Players.Add(p);
      }) },
      {"skip_turn",new Action<object[]>((oarr)=> {
        var game = Game.Inst;
        var pl = oarr!=null && oarr.Length>0? oarr[0] as Player:game.MainPlayer;
        pl.NumActions = 0;
      }) },
      {"double_turn",new Action<object[]>((oarr)=> {
        var game = Game.Inst;
        var pl = oarr!=null && oarr.Length>0? oarr[0] as Player:game.MainPlayer;
        pl.NumActions = 2;
      }) },
      {"remove",new Action<object[]>((oarr)=> {
        var game = Game.Inst;
        if (oarr == null) return;
        var len = oarr.Length;
        var pl = len==2?oarr[0] as Player:game.MainPlayer;
        var field = len==1?oarr[0] as Field: len==2?oarr[1] as Field:null;
        if (field != null) Checker.RemoveCivCard(pl,field);//...
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
      {"go_last",new Action<Game,Player>((game,pl)=> {
        var others = game.Players.Where(x=>x!=pl).ToList();
        List<Player> newlist = new List<Player>();
        newlist.AddRange(others);
        newlist.Add(pl);
        game.Players.Clear();
        foreach(var p in newlist)game.Players.Add(p);
      }) },
    };

    #endregion

    #region  helpers (GetResourcesForRule, GetSpecialEffect)
    //public static List<Res> GetResourcesForRule(XElement xel)
    //{
    //  List<string> exceptions = new List<string> { "special_effect", "affects","trigger","pred", "param", "text", "min", "effect", "foreach", "func", "task", "type", "age" };
    //  List<Res> result = new List<Res>();
    //  var reslist = xel.Attributes().Where(x => !exceptions.Contains(x.Name.ToString())).ToList();
    //  foreach (var attr in reslist)
    //  {
    //    var name = attr.Name.ToString();
    //    int n = 0;
    //    var ok = int.TryParse(attr.Value, out n);
    //    Debug.Assert(ok, "GetResourcesForRule: Unparsable attribute value for resource " + name);
    //    result.Add(new Res(name, n));
    //  }
    //  return result;
    //}
    //public static Action<object[]> GetSpecialEffect(XElement xel)
    //{
    //  var specialEffect = xel.astring("effect");
    //  Action<object[]> effectAction = null;
    //  if (!string.IsNullOrEmpty(specialEffect) && dEffectII.ContainsKey(specialEffect)) { effectAction = dEffectII[specialEffect]; }
    //  return effectAction;
    //}
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
    #endregion
  }
}
