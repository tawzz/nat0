﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ations
{
  public partial class Checker
  {
    public Game GameInst { get; set; }
    public Checker(Game game) { GameInst = game; }
    public void AddToRes(List<Res> list, string name, int num) { var res = list.FirstOrDefault(x => x.Name == name); if (res == null) list.Add(new Res(name, num)); else res.Num += num; }
    public async Task ApplyResEffectTask(Res resEffect)
    {
      var game = GameInst;
      //var game = gameinst; Debug.Assert(game != null, "gameinst is null!");
      var resname = resEffect.Name;
      var resnum = resEffect.Num;
      if (resname == "worker" && resnum < 0) // needs to return a worker!
      {
        var workerRes = await game.ReturnWorkerTask(game.MainPlayer); //"" if its a gratis extra worker
      }
      else if (resname == "worker" && resnum > 0)
      {
        await game.CheckoutExtraWorkerTask(game.MainPlayer);
        //var worker = await game.PickExtraWorkerTask();
        //game.Message = game.MainPlayer.Name + " picked " + worker.CostRes.ToCapital() + " worker";
        //game.MainPlayer.CheckOutWorker(worker);//weg
      }
      else if (resname == "arch" && resnum > 0)// this is NOT a free architect!!!
      {
        // distinguish between free architects and private architects
        // if it is private architect, can take this action resnum times!!!, need to have enough coal to take it and need to pay
        // if it is free architects, can place as many as get and wonder can use, no coal check, no pay
        await game.AddPrivateArchitectTask();
      }
      else if (resname == "arch" && resnum < 0)
      {
        game.MainPlayer.WIC.NumDeployed += resnum;//remove architects from card (since resnum < 0!!!)
        Debug.Assert(game.MainPlayer.WIC.NumDeployed >= 0);
      }
      else
      {
        await game.PayTask(game.MainPlayer, resEffect.Name, -resEffect.Num);

      }

    }
    public async Task ExecuteEffectAction(string effect, string param, Player pl)
    {
      var game = GameInst;
      switch (effect) //TODO streamline shortcuts!
      {
        case "go_first": { List<Player> newlist = new List<Player> { pl }; newlist.AddRange(Others(pl)); game.Players.Clear(); foreach (var p in newlist) game.Players.Add(p); break; }
        case "go_last": { List<Player> newlist = Others(pl).ToList(); newlist.Add(pl); game.Players.Clear(); foreach (var p in newlist) game.Players.Add(p); } break;
        case "lose_all_gold_except_2": if (pl.Res.n("gold") > 2) pl.Res.set("gold", 2); break;
        case "may_undeploy_building_worker_for_2_coal_each": if (pl.HasBuildingDeployed) { var answer = await game.YesNoChoiceTask("undeploy from building for 2 coal each?"); if (answer) await game.PickUndeployForEachTask(pl, new string[] { "building" }, "coal", 2); } break;
        case "may_take_2_workers_and_4_coal": if (pl.ExtraWorkers.Count >= 2) { var answer = await game.YesNoChoiceTask("take out 2 worker and get 4 coal?"); if (answer) { for (int i = 0; i < 2; i++) await game.CheckoutExtraWorkerTask(pl); pl.UpdateResBy("coal", 4); } } break;
        case "may_hire_2_arch_for_free": if (pl.HasWIC) { var answer = await game.YesNoChoiceTask("hire 2 architects for free?"); if (answer) await game.AddFreeArchitectsTask(pl, 2); } break;
        case "may_deploy_2_workers_to_military_for_free": if (pl.NumMilitaryFields > 0) { var answer = await game.YesNoChoiceTask("deploy 2 workers to military for free?"); if (answer) { for (int i = 0; i < 2; i++) { await game.DeployWorkerForFreeTask(pl, new string[] { "military" }); } } } break;
        case "regain_war_loss": if (pl.Military < game.Stats.WarLevel) { foreach (var res in pl.WarLoss) { pl.UpdateResBy(res.Name, res.Num); } } break;
        case "remove_advisors": game.RemoveAdvisor(pl); break;
        case "remove_colony_if_none_pay_vp": { var n = pl.NumColonies; if (n > 1) { var f = await game.PickCivFieldOutOf(pl, pl.Colonies, "event"); RemoveCivCard(pl, f); } else if (n == 1) { RemoveCivCard(pl, pl.Colonies.First()); } else { await game.PayTask(pl, "vp", 1); } } break;
      }
      await Task.Delay(100);
    }
    public List<Player> Filter(string pred, string param, IEnumerable<Player> basePlayerSet)
    {
      var game = GameInst;
      switch (pred)
      {
        case "least":
        case "not_least":
        case "most":
        case "not_most": return PlayersWith(pred, param, basePlayerSet);
        case "most_bought": var max = basePlayerSet.Max(x => x.CardsBoughtThisRound.Count(y => y.Type == param)); return basePlayerSet.Where(x => x.CardsBoughtThisRound.Count(y => y.Type == param) == max).ToList();
        case "pass_first": return basePlayerSet.Where(x => x.HasPassed && game.PassOrder.First() == x).ToList();
        case "pass_last": return game.AllPlayersPassed ? basePlayerSet.Where(x => x.HasPassed && game.PassOrder.Last() == x).ToList() : new List<Player>();
        case "defeated": return game.Stats.IsWar ? basePlayerSet.Where(x => x.Military < game.Stats.WarLevel).ToList() : new List<Player>();
        case "not_defeated": return game.Stats.IsWar ? basePlayerSet.Where(x => x.Military >= game.Stats.WarLevel).ToList() : new List<Player>();
        case "greater_than_war": return game.Stats.IsWar ? basePlayerSet.Where(x => x.Military > game.Stats.WarLevel).ToList() : new List<Player>();
        case "have_medieval_advisor": return basePlayerSet.Where(x => x.Cards.Any(y => y.adv() && y.Age == 2)).ToList();
        case "have_medieval_or_antiquity_advisor": return basePlayerSet.Where(x => x.Cards.Any(y => y.adv() && y.Age <= 2)).ToList();
        case "if_at_least_1_industrial_colony": { return basePlayerSet.Where(x => x.Colonies.Count(y => y.Card.Age >= 4) >= 1).ToList(); }
        case "most_workers_in_industrial_buildings":
          {
            Player plbest = null;
            int nmax = 0;
            foreach (var pl in basePlayerSet)
            {
              var nworkers = pl.Buildings.Where(x => x.Card.Age >= 4).Sum(x => x.Card.NumDeployed);
              if (nworkers > nmax) { nmax = nworkers; plbest = pl; }
              else if (nworkers == nmax) { plbest = null; }
            }
            if (plbest != null) return new List<Player> { plbest }; else return new List<Player>();
          }
        default: Console.WriteLine("\t\tunknown predicate: " + pred); break;
      }
      return basePlayerSet.ToList();
    }
    public List<Player> CalcAffects(XElement ev, List<Player> basePlayerSet)
    {
      var game = GameInst;
      var affects = ev.astring("affects"); // affects attribute overrides basePlayerSet
      if (!string.IsNullOrEmpty(affects))
      {
        switch (affects)
        {
          case "all": basePlayerSet = game.Players.ToList(); break;
          case "others": basePlayerSet = game.Players.Where(x => x != game.MainPlayer).ToList(); break;
          case "self": basePlayerSet = new List<Player> { game.MainPlayer }; break;
          default: break;
        }
      }
      var pred = ev.astring("pred");
      var param = ev.astring("param");
      if (string.IsNullOrEmpty(pred)) return basePlayerSet; else return Filter(pred, param, basePlayerSet);
      //else if (Selectors.ContainsKey(pred)) return Selectors[pred](ev.astring("param"), basePlayerSet);
      //else if (Predicates.ContainsKey(pred)) return basePlayerSet.Where(x => Predicates[pred](ev.astring("param"))).ToList();
      //else { Console.WriteLine(game.LongMessage = "pred " + pred + " unknown!"); return basePlayerSet; }
    }
    public bool CheckResEffects(Player pl, IEnumerable<Res> resEffects)
    {
      foreach (var res in resEffects)
      {
        if (res.Name == "worker")
        {
          if (res.Num < 0)
          {
            var ndeployed = pl.BMCards.Sum(x => x.NumDeployed);
            var workers = pl.Res.n("worker");
            if (ndeployed + workers <= 0) return false;
          }
          else if (res.Num > 0)
          {
            var workersavail = pl.ExtraWorkers.Count(x => !x.IsCheckedOut) + pl.GratisExtraWorkers;
            if (workersavail <= 0) return false;
          }
        }
        else if (res.Name == "arch")
        {
          if (res.Num > 0 && (!pl.HasWIC || !GameInst.CalcCanAffordArchitect(pl))) return false;
          if (res.Num < 0 && (!pl.HasWIC || pl.WIC.NumDeployed < -res.Num)) return false;
        }
        else if (res.Num < 0 && pl.Res.n(res.Name) < -res.Num) return false;
      }
      return true;
    }
    public List<Res> GetResourcesForRule(XElement xel)
    {
      List<string> exceptions = new List<string> { "pred", "param", "effect", "eparam", "affects", "text", "max" };
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
    public List<Player> PlayersWith(string pred, string resname, IEnumerable<Player> pls = null)
    {
      var game = GameInst;
      if (pls == null) pls = game.Players.ToList();
      Debug.Assert(pls.Count() >= 1, "0 players in PlayersWith");
      var sorted = pls.OrderBy(x => x.Res.n(resname)).Select(x => new Tuple<Player, int>(x, x.Res.n(resname))).ToList();
      var min = sorted.First().Item2;
      var max = sorted.Last().Item2;
      var resultingTuples = pred == "least" ? sorted.Where(x => x.Item2 == min).ToList()
        : pred == "most" ? sorted.Where(x => x.Item2 == max).ToList()
        : pred == "not_least" ? sorted.Where(x => x.Item2 != min).ToList()
        : sorted.Where(x => x.Item2 != max).ToList();
      if (pred == "most") return resultingTuples.Count == 1 ? resultingTuples.Select(x => x.Item1).ToList() : new List<Player>();
      else if (pred == "not_most") return resultingTuples.Count == pls.Count() - 1 ? resultingTuples.Select(x => x.Item1).ToList() : pls.ToList();
      else return resultingTuples.Select(x => x.Item1).ToList();
    }
    public bool PlayerHas(Player pl, string pred, string resname) { return PlayersWith(pred, resname).Contains(pl); }
    public bool OtherPlayerHas(Player pl, string name)
    {
      var game = GameInst;
      foreach (var other in game.Players)
      {
        if (other == pl) continue;
        if (other.Cards.Any(x => x.Name == name)) return true;
      }
      return false;
    }
    public bool AnyPlayerHas(string name)
    {
      var game = GameInst;
      foreach (var pl in game.Players)
      {
        if (pl.Cards.Any(x => x.Name == name)) return true;
      }
      return false;
    }
    public Player PlayerThatHas(string name) { return GameInst.Players.FirstOrDefault(x => x.Cards.Any(y => y.Name == name)); }
    public List<Player> Others(Player pl) { return GameInst.Players.Where(x => x != pl).ToList(); }

    #region unused
    //public static List<Player> CalcAffects_old(XElement ev, List<Player> basePlayerSet)
    //{
    //  var game = GameInst;
    //  var affects = ev.astring("affects"); // affects attribute overrides basePlayerSet
    //  if (!string.IsNullOrEmpty(affects))
    //  {
    //    switch (affects)
    //    {
    //      case "all": basePlayerSet = game.Players.ToList(); break;
    //      case "others": basePlayerSet = game.Players.Where(x => x != game.MainPlayer).ToList(); break;
    //      case "self": basePlayerSet = new List<Player> { game.MainPlayer }; break;
    //      default: break;
    //    }
    //  }
    //  var pred = ev.astring("pred");
    //  if (string.IsNullOrEmpty(pred)) return basePlayerSet;
    //  else if (Selectors.ContainsKey(pred)) return Selectors[pred](ev.astring("param"), basePlayerSet);
    //  else if (Predicates.ContainsKey(pred)) return basePlayerSet.Where(x => Predicates[pred](ev.astring("param"))).ToList();
    //  else { Console.WriteLine(game.LongMessage = "pred " + pred + " unknown!"); return basePlayerSet; }
    //}
    //public static EffectAction GetEffectAction(XElement xel)
    //{
    //  var specialEffect = xel.astring("effect");
    //  Console.WriteLine("\teffect=" + specialEffect);
    //  var result = string.IsNullOrEmpty(specialEffect) ? null
    //    : Effects.ContainsKey(specialEffect) ? Effects[specialEffect]
    //    : ImpossibleEffect;
    //  return result;
    //}
    //public static void ImpossibleAction(object[] oarr) { }
    //public static EffectAction ImpossibleEffect = new EffectAction((a, b, c, d) => { });
    //public static Action<object[]> GetSpecialEffect_legacy(XElement xel) // TODO: ersetze dEffectII durch EffectActions
    //{
    //  var specialEffect = xel.astring("effect");
    //  var result = string.IsNullOrEmpty(specialEffect) ? null
    //    : dEffectII.ContainsKey(specialEffect) ? dEffectII[specialEffect]
    //    : ImpossibleAction;
    //  return result;
    //}

    #endregion


  }
}
