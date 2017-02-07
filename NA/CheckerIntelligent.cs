using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ations
{
  public static partial class Checker
  {
    public static async Task CheckGrowth(Res respicked) { await CheckGrowthStupid(respicked); }
    public static async Task PickEventCard() { await PickEventCardStupid(); }

    public static void CalcPlayerOrder() { CalcPlayerOrderStupid(); }
    public static int CalcPlayerOrderValue(Player pl) { return CalcPlayerOrderValueStupid(pl); }
    public static async Task<int> CalcNumActions(bool isFirstTurn) { return await CalcNumActionsStupid(isFirstTurn); }
    public static void CalcStabAndMil(Player pl, bool ignoreLargeRoundEffects = false) { CalcStabAndMilStupid(pl, ignoreLargeRoundEffects); }

    public static bool CheckActionPossible(Player pl, Card card)
    { //<a pred=x affects=a param=y effect=z [eaffects=c?!?] eparam=a res1=n1... /> // not good enough!
      // for now: all <a> with effect (pred or affects ALWAYS have effect) go to stupid!
      if (!card.ActionAvailable) return false;
      var ev = card.GetAction;
      var effectAction = ev.astring("effect"); if (!string.IsNullOrEmpty(effectAction)) { Console.WriteLine("CheckActionPossible: " + card.Name + " (stupid) "); return Checker.CheckActionPossibleStupid(pl, card); }

      // arrive here only if there is no "effect" clause!!!
      Console.WriteLine("CheckActionPossible: " + card.Name + " (intelligent)");
      var resEffects = GetResourcesForRule(ev);
      var resEffectsPossible = CheckResEffects(pl, resEffects);
      return resEffectsPossible;
    }
    public static async Task ExecuteAction(Player plMain, Card card)
    { // assume: when getting here the action IS possible, pl is owner of card
      var ev = card.GetAction;
      var effectAction = ev.astring("effect"); if (!string.IsNullOrEmpty(effectAction)) { await Checker.ExecuteActionStupid(plMain, card); return; }

      // arrive here only if there is no "effect" clause!!! in that case, there is also no pred or affects clause!!!
      Debug.Assert(ev.astring("pred") == "" && ev.astring("affects") == "", "ExecuteAction: Action has pred or affects but not effect: card=" + card.Name);
      var resEffects = GetResourcesForRule(ev);
      foreach (var reff in resEffects) await ApplyResEffectTask(reff);
    }

    #region buy / deploy / worker
    public static async Task CheckBuy(Card card, Field field) { await CheckBuyStupid(card, field); }
    public static async Task CheckBuyGoldenAge(Player pl, Card card, Res resPicked) { await CheckBuyGoldenAgeStupid(pl, card, resPicked); }
    public static int CalcDeployCost(Player pl, Card card) { return CalcDeployCostStupid(pl, card); }
    public static int CalcGoldenAgeBonus(Player pl) { return CalcGoldenAgeBonusStupid(pl); }
    public static int CalcGoldenAgeBonusForVP(Player pl) { return CalcGoldenAgeBonusForVPStupid(pl); }
    public static int CalcPrice(Card card) { return CalcPriceStupid(card); }
    public static int CalcRaid(Player pl) { return CalcRaidStupid(pl); }
    public static bool CheckMilminCondition(Player pl, Card card) { return CheckMilminConditionStupid(pl, card); }
    public static void CheckCheckOutExtraWorker(Player pl, Worker worker) { CheckCheckOutExtraWorkerStupid(pl, worker); }
    #endregion

    #region wonder architect turmoil dynasty
    public static async Task CheckReady(Field newwonder) { await CheckReadyStupid(newwonder); }
    public static int CheckArchitectCost(Player pl, Card card, int baseCost) { return CheckArchitectCostStupid(pl, card, baseCost); }
    public static async Task CheckHireArchitect(Player pl) { await CheckHireArchitectStupid(pl); }
    public static async Task CheckTurmoil(Player pl, bool tookGold) { await CheckTurmoilStupid(pl,tookGold); }
    public static async Task CheckUpgradeDynasty(Player pl, Card newdyn) { await CheckUpgradeDynastyStupid(pl, newdyn); }
    #endregion

    public static async Task CheckPreActionPhase() { await CheckPreActionPhaseStupid(); }
    public static async Task CheckPreAction(bool isFirstTurn) { await CheckPreActionStupid(isFirstTurn); }
    public static async Task CheckPostAction() { await CheckPostActionStupid(); }
    public static async Task CheckPostActionPhase() { await CheckPostActionPhaseStupid(); }

    public static async Task<Res[]> CalcProduction(Player pl) { return await CalcProductionStupid(pl); }
    public static async Task CheckPostProduction(Player pl) { await CheckPostProductionStupid(pl); }

    public static List<Res> CalcWarPenalty(Player pl, IEnumerable<Res> reslist) { return CalcWarPenaltyStupid(pl, reslist); }

    public static async Task HandleSimpleEvent(XElement ev, Card card) { await HandleSimpleEventStupid(ev, card); }
    public static async Task HandleEventCard(Card evcard) { await HandleEventCardStupid(evcard); }

    public static int CalcFamine(Player pl, int famine) { return CalcFamineStupid(pl, famine); }
    public static async Task CheckEndOfAge() { await CheckEndOfRoundOrAgeStupid(); }
    public static double CalcScore(Player pl) { return CalcScoreStupid(pl); }

    #region add remove
    public static async Task AddCivCard(Player pl, Card card, Field field)
    {
      if (field == pl.WICField) AddWIC(pl, card); else await AddCivCardStupid(pl, field, card);
    }
    public static void AddCivCardSync(Player pl, Card card, Field field)
    {
      if (field == pl.WICField) AddWIC(pl, card); else AddCivCardStupidSync(pl, field, card);
    }
    public static void RemoveCivCard(Player pl, Field field) { RemoveCivCardStupid(pl, field); }
    public static void RemoveCivCard(Player pl, string name)
    {
      var field = pl.Civ.Fields.FirstOrDefault(x => !x.IsEmpty && x.Card.Name == name);
      if (field != null) RemoveCivCardStupid(pl, field);
    }
    public static void AddWIC(Player pl, Card card) { AddWICStupid(pl, card); }
    public static void RemoveWIC(Player pl) { RemoveWICStupid(pl); }
    #endregion





    #region unused
    public static async Task ExecuteAction_overly_complex(Player plMain, Card card)
    { // assume: when getting here the action IS possible, pl is owner of card
      var ev = card.GetAction;
      var effectAction = ev.astring("effect"); if (!string.IsNullOrEmpty(effectAction)) { await Checker.ExecuteActionStupid(plMain, card); return; }

      // arrive here only if there is no "effect" clause!!!
      var resEffects = GetResourcesForRule(ev);
      var pls = CalcAffects(ev, new List<Player> { plMain }); // default is only owner of activated card affected

      foreach (var pl in pls)
      {
        var cando = CheckResEffects(pl, resEffects) || effectAction != null; if (!cando) continue;
        if (pl != Game.Inst.MainPlayer) { Game.Inst.MainPlayer = pl; await Task.Delay(1000); }
        //effectAction?.Invoke(ev.astring("eparam"), card, plMain);
        foreach (var reff in resEffects) await ApplyResEffectTask(reff);
      }

      if (plMain != Game.Inst.MainPlayer) { Game.Inst.MainPlayer = plMain; await Task.Delay(1000); }
      await Task.Delay(100);

      //if (waitForStartPress) { Caption = "start"; LongMessage = "click start to start effect handler task for: " + ev.ToString() + "... start!"; await WaitForButtonClick(); Caption = "Ok"; }

      //var plSelected = oarr == null ? CalcPlayersAffectedByPredOnPlayer(ev, basegroup) : CalcPlayersAffectedByPredOnObjects(ev, basegroup, oarr);
      //var resEffects = GetResourcesForRule(ev);
      //var effectAction = Checker.GetSpecialEffect(ev); // spaeter liste

      //foreach (var pl in plSelected)
      //{
      //  var cando = CheckResEffects(pl, resEffects) || effectAction != null; if (!cando) continue;

      //  var num = isCounter ? CalcCounter(ev, pl) : 1; if (num == 0) continue;

      //  MainPlayer = pl; //switch ui to this player
      //  await Task.Delay(longDelay);

      //  if (isYesNo && !isCounter)
      //  {
      //    var answer = await YesNoChoiceTask(ev.astring("text"));
      //    if (!answer) continue;
      //  }

      //  for (int i = 0; i < num; i++)
      //  {
      //    cando = CheckResEffects(pl, resEffects) || effectAction != null; if (!cando) continue;

      //    if (isYesNo && isCounter)
      //    {
      //      var answer = await YesNoChoiceTask(ev.astring("text"));
      //      if (!answer) break;
      //    }

      //    if (oarr == null) { Message = "oarr is null in EffectHandler!"; }
      //    effectAction?.Invoke(oarr);
      //    foreach (var reff in resEffects) await ApplyResEffectTask(reff);
      //  }

      //  await Task.Delay(shortDelay);
      //  await WaitForAnimationQueueCompleted();

      //  if (pl != plSelected.Last()) { Message = "next player..."; await WaitForButtonClick(); }
      //}
    }
    public static async Task ExecuteEffectForPlayer(XElement ev, Player pl, Card card)
    { // assume: when getting here the action IS possible, pl is owner of card

      //get the effects
      var resEffects = GetResourcesForRule(ev);
      var effectAction = GetEffectAction(ev);

      // select players affected (pred param / affects)
      var pls = CalcAffects(ev, new List<Player> { pl }); // default is only owner of activated card affected

      //check if effects are possible
      var cando = CheckResEffects(pl, resEffects) || effectAction != null; if (!cando) return;

      foreach (var reff in resEffects) await ApplyResEffectTask(reff);

      if (effectAction == ImpossibleEffect) await Checker.ExecuteEffectForPlayerStupid(ev, pl, card);
      else effectAction?.Invoke(ev.astring("eparam"), card, pl);

    }
    public static List<string> BuyResources(Card card) { return BuyResourcesStupid(card); }
    //public static List<Player> CalcPlayersDefeated() { return CalcPlayersDefeatedStupid(); }
    #endregion
  }
}
