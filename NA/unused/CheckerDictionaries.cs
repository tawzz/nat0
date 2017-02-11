using System;
using System.Collections.Generic;
using System.Linq;

namespace ations
{
  public delegate List<Player> SelFunc(string param = null, IEnumerable<Player> pls = null, Card card = null, Player owner = null, object state = null);
  public delegate bool PredFunc(string param = null, Card card = null, Player owner = null, object state = null);
  public delegate void EffectAction(string param = null, Card card = null, Player owner = null, object state = null);

  public static partial class Checker
  {
    public static Dictionary<string, SelFunc> Selectors = new Dictionary<string, SelFunc>()
    {
      {"least", new SelFunc((resname,players,_c,_o,_s) => { return PlayersWith("least",resname,players).ToList(); }) },
      {"most", new SelFunc((resname,players,_c,_o,_s) => { return PlayersWith("most",resname,players).ToList(); }) },
      {"not_least", new SelFunc((resname,players,_c,_o,_s) => { return PlayersWith("not_least",resname,players).ToList(); }) },
      {"not_most", new SelFunc((resname,players,_c,_o,_s) => { return PlayersWith("not_most",resname,players).ToList(); }) },

    };
    public static Dictionary<string, PredFunc> Predicates = new Dictionary<string, PredFunc>()
    {

    };
    public static Dictionary<string, EffectAction> Effects = new Dictionary<string, EffectAction>()
    {

    };


    









    public static Dictionary<string, Action<object[]>> dEffectII = new Dictionary<string, Action<object[]>>()
    {
      {"regain_war_loss",new Action<object[]>((oarr)=> {
        var game = Game.Inst;
        var pl = oarr!=null && oarr.Length>0? oarr[0] as Player:game.MainPlayer;
        if (pl.Military < game.Stats.WarLevel) // lost at war
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
  }
}
