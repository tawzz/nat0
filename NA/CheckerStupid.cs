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
    public static async Task CheckGrowthStupid(Res respicked)
    {
      var game = Game.Inst; var pl = game.MainPlayer;

      await Task.Delay(100);
      if (pl.HasCard("mali_dyn1") && respicked.Name == "gold") { pl.UpdateResBy("gold", 1); }
      if (pl.HasCard("spice_islands") && respicked.Name != "worker") { pl.UpdateResBy(respicked.Name, 4); }
      if (pl.HasCard("india_mauryan_empire") && respicked.Name == "worker" && pl.HasExtraworkers)
      {
        var isYes = await game.YesNoChoiceTask("pick two more workers?");
        if (isYes)
        {
          for (int i = 0; i < 2; i++)
          {
            await game.CheckoutExtraWorkerTask(pl);
            //var worker = await game.PickExtraWorkerTask();
            //if (worker == null) break;
            //game.Message = pl.Name + " picked " + worker.CostRes.ToCapital() + " worker";
            //pl.CheckOutWorker(worker);//weg
          }
        }
      }
    }
    public static async Task PickEventCardStupid()
    {
      var game = Game.Inst;
      Card evcard = null;
      foreach (var pl in game.Players)
      {
        if (pl.HasCard("niccolo_machiavelli")) { evcard = await game.PickACard(pl, game.Stats.GetEventCards(2), "new event"); }
      }

      game.Stats.PickEventCard(evcard);

      // cards affecting after revealing event:
      foreach (var pl in game.Players)
      {
        if (pl.HasCard("ethiopia_axumite_kingdom")) { var field = await game.PickProgressCardTask(pl); field.Card.NumDeployed = 1; pl.GetCard("ethiopia_axumite_kingdom").Tag = field; }
      }

    }

    public static void CalcPlayerOrderStupid()
    {
      var game = Game.Inst;
      if (game.Stats.EventCard.Name == "feudal_dues") return; // no change in turn

      var plarr = game.Players.OrderBy(x => Checker.CalcPlayerOrderValue(x)).Reverse().ToArray();
      game.Players.Clear();
      foreach (var pl in plarr) game.Players.Add(pl);
      for (int i = 0; i < game.Players.Count; i++) game.Players[i].Index = i;
      game.MainPlayer = game.Players[0];

    }
    public static int CalcPlayerOrderValueStupid(Player pl)
    {
      int val = pl.Military;

      if (pl.HasCard("ethiopia_dyn1")) val += pl.Stability;

      return val;
    }
    public static async Task<int> CalcNumActionsStupid(bool isFirstTurn)
    {
      var game = Game.Inst; var pl = game.MainPlayer;

      if (pl.HasNaturalWIC)
      {
        pl.RoundsToWaitForNaturalWonder--;
        if (pl.RoundsToWaitForNaturalWonder == 0) { await game.WonderReadyTask(); } else { game.Message = "Wait Round"; await Task.Delay(1000); }
        return 0;
      }

      if (isFirstTurn)
      {
        if (pl.HasCard("buddha")) return 0;
        if (pl.HasCard("sun_tzu")) return 2;
      }
      return 1;
    }
    public static void CalcStabAndMilStupid(Player pl, bool ignoreLargeRoundEffects = false)
    {
      var mil = 0; var stab = 0;
      var game = Game.Inst;
      var owncards = pl.Cards.ToList();
      foreach (var c in owncards) { var factor = c.buildmil() ? c.NumDeployed : 1; stab += c.GetStability * factor; mil += c.GetMilitary * factor; }
      foreach (var w in pl.ExtraWorkers.Where(x => x.IsCheckedOut)) { if (w.CostRes == "stability") { stab -= w.Cost; } if (w.CostRes == "military") { mil -= w.Cost; } }

      stab -= pl.TurmoilsTaken * 2;

      if (AnyPlayerHas("genghis_khan")) stab -= 3;
      if (pl.HasCard("elizabeth") && game.Stats.IsWar) mil += 8;
      if (pl.HasCard("korea_koryo_kingdom") && game.Stats.IsWar) mil += pl.NumMilitaryDeployed * 3;
      if (pl.HasCard("sahara") && game.Stats.IsWar) mil += 4;
      if (pl.HasCard("greece_sparta") && pl.NumMilitaryDeployed == 1) mil += 4;
      if (pl.HasCard("emperor")) mil += pl.GetCard("emperor").NumDeployed;

      var REff = ignoreLargeRoundEffects ? pl.RoundResEffects.Where(x => Math.Abs(x.Num) < 100).ToList() : pl.RoundResEffects;
      if (REff.Any(x => x.Name == "military")) mil += REff.Where(x => x.Name == "military").Sum(x => x.Num);
      if (REff.Any(x => x.Name == "stability")) stab += REff.Where(x => x.Name == "stability").Sum(x => x.Num);

      //if (pl.Military != mil) CheckMilitaryChange(pl);
      //if (pl.Stability != stab) CheckStabilityChange(pl);

      pl.Military = mil;
      pl.Stability = stab;
    }

    public static bool CheckActionPossibleStupid(Player pl, Card card) //WEITER HIER!!!
    { //checks precondition (players affected non-empty) and effect applicable if any)
      var game = Game.Inst;
      if (card.Name == "abraham_lincoln") { return pl.HasExtraworkers; }
      if (card.Name == "archimedes") { return pl.HasWIC; }
      if (card.Name == "alhazen" || card.Name == "grand_duchy_of_finland") { return true; }
      if (card.Name == "america_democratic_republicans") { return PlayersWith("least", "military", game.Players).Any(x => x.Civ.Dynasties.Count > 0); }
      if (card.Name == "china_qin_dynasty") { return pl.HasWIC && pl.NumWorkers >= 1; }
      if (card.Name == "egypt_old_kingdom") { return pl.HasAdvisor; }
      if (card.Name == "frederic_chopin") { return pl.Res.n("gold") >= 5; }
      if (card.Name == "galileo_galilei") { return game.Progress.Cards.Any(x => x.wonder() || x.golden()); }
      if (card.Name == "great_barrier_reef") { return pl.Res.n("vp") >= 1; }
      if (card.Name == "korea_joseon_kingdom") { return pl.Res.n("coal") > 0 || pl.Res.n("gold") > 0 || pl.Res.n("wheat") > 0; }
      if (card.Name == "mali_mali_empire") { return pl.Res.n("gold") >= 2 && pl.HasAdvisor; }
      if (card.Name == "marco_polo") { return pl.Res.n("wheat") >= 2 || pl.Res.n("coal") >= 2; }
      if (card.Name == "old_uppsala") { return pl.Res.n("wheat") >= 1; }
      if (card.Name == "piazza_san_marco") { return pl.Res.n("gold") >= 2; }
      if (card.Name == "petra") { return pl.Res.n("wheat") >= 1; }
      if (card.Name == "poland_polish-lithuanian_commonwealth") { return pl.Res.n("gold") >= 3 && WonderAdvisorsAvailableOnOwnerOf(card); }
      if (card.Name == "rome_roman_republic") { return pl.HasWIC && pl.WIC.NumDeployed > 0; }
      if (card.Name == "royal_society") { return pl.Res.n("worker") >= 1; }
      if (card.Name == "suleiman") { return PlayerHas(pl, "most", "military") && pl.HasExtraworkers; }
      if (card.Name == "simon_bolivar") { return pl.Cards.Any(x => x.colony()); }
      if (card.Name == "shwedagon_pagoda") { return !game.Players.Any(x => x.HasPassed); }

      return false;
    }
    public static async Task ExecuteActionStupid(Player pl, Card card)
    {// hier komme her wenn action zu kompliziert fuer Checker
      var game = Game.Inst;

      if (card.Name == "alhazen") { await game.SwapTwoProgressCardsTask(); }
      else if (card.Name == "america_democratic_republicans")
      {
        var pls = PlayersWith("least", "military", game.Players);
        foreach (var p in pls) { if (p.Civ.Dynasties.Count >= 1) { var newdyn = await game.UpgradeDynastyTask(p); } }
      }
      else if (card.Name == "china_qin_dynasty")
      {
        await game.ReturnWorkerTask(pl);
        await game.AddFreeArchitectsTask(pl, 1);
      }
      else if (card.Name == "egypt_old_kingdom") { pl.RemoveAdvisor(); pl.GetCard("egypt_old_kingdom").NumDeployed++; }
      else if (card.Name == "frederic_chopin")
      {
        var owner = game.Players.FirstOrDefault(x => x.HasCard("frederic_chopin"));
        Debug.Assert(owner != null, "ExecuteActionStupid: nobody owns frederic chopin!!!");
        pl.UpdateResBy("gold", -5); owner.UpdateResBy("gold", 5);
        var field = owner.GetFieldOf("frederic_chopin");
        var newcard = field.Card;
        RemoveCivCard(owner, field);//special options for chopin removed from owner and everyone else!
        var newfield = await game.PickFieldForType(pl, "advisor", "frederic_chopin");
        await AddCivCard(pl, newcard, newfield);
      }
      else if (card.Name == "grand_duchy_of_finland") { return; } //skips turn
      else if (card.Name == "galileo_galilei") { await game.PickWonderOrGoldenAgeTask(); }
      else if (card.Name == "korea_joseon_kingdom")
      {
        var reslist = new List<Res> { new Res("wheat"), new Res("coal"), new Res("gold") };
        reslist = await game.PickMultiResourceWithinRangeTask(reslist, 1, 3, "action");
        // ziehe resources dem player ab
        foreach (var res in reslist) { await game.PayTask(pl, res.Name, res.Num); }
        // add resources as Tag to dyn card
        card.Tag = card.ListOfResources = reslist.Where(x => x.Num != 0).ToList();// foreach (var res in reslist) { card.StoredResources.Add(res); }
      }
      else if (card.Name == "mali_mali_empire") { pl.RemoveAdvisor(); pl.UpdateResBy("book", 3); await game.PayTask(pl, "gold", 2); pl.UpdateResBy("vp", 1); }
      else if (card.Name == "marco_polo")
      {
        var resname = "";
        if (pl.Res.n("wheat") >= 2 && pl.Res.n("coal") >= 2)
        {
          var res = await game.PickResourceTask(new string[] { "wheat", "coal" }, "action");
          resname = res.Name;
        }
        else if (pl.Res.n("wheat") >= 2) resname = "wheat";
        else resname = "coal";
        pl.UpdateResBy(resname, -2);
        pl.UpdateResBy("gold", 4);
      }
      else if (card.Name == "old_uppsala") { pl.UpdateResBy("wheat", -1); pl.RoundResEffects.Add(new ations.Res("military", 3)); }
      else if (card.Name == "piazza_san_marco")
      {
        var res = await game.PickResourceAndGetItTask(new string[] { "book", "wheat", "coal" }, 5, "action");
        pl.UpdateResBy("gold", -2);
      }
      else if (card.Name == "petra") { var res = await game.PickResourceAndGetItTask(new string[] { "book", "coal", "gold" }, 3, "action"); pl.UpdateResBy("wheat", -1); }
      else if (card.Name == "poland_polish-lithuanian_commonwealth")
      {
        var owner = game.Players.FirstOrDefault(x => x.HasCard("poland_polish-lithuanian_commonwealth"));
        Debug.Assert(owner != null, "ExecuteActionStupid: nobody owns poland_polish-lithuanian_commonwealth!!!");
        pl.UpdateResBy("gold", -3); owner.UpdateResBy("gold", 3);
        // now pl must select among one of the advisors placed on owner's wonder spaces
        // get owners advisors placed on wonder spaces
        var ownerFields = owner.Civ.Fields.Where(x => x.TypesAllowed.Contains("wic")).ToList();
        var ownerFieldsWithAdvisors = ownerFields.Where(x => !x.IsEmpty && x.Card.adv()).ToList();

        // if there is more than one, put cards in card selector, let pl select one
        // otherwise, 
        Card newcard = null;
        Debug.Assert(ownerFieldsWithAdvisors.Count > 0, "ExecuteAction buy advisor from poland with no advisor to buy!!!");
        if (ownerFieldsWithAdvisors.Count > 1)
        {
          List<Choice> list = new List<Choice>();
          foreach (var f in ownerFieldsWithAdvisors) { var ch = new Choice(); ch.Tag = f.Card; ch.Text = "buy"; list.Add(ch); }

          var chosen = await game.PickCardChoiceTask(list, "buy action");
          newcard = chosen.Tag as Card;
        }
        else newcard = ownerFieldsWithAdvisors.First().Card;

        var fieldChosen = owner.Civ.Fields.FirstOrDefault(x => x.Card == newcard);

        //var field = //owner.GetFieldOf("poland_polish-lithuanian_commonwealth");
        //var newcard = field.Card;
        RemoveCivCard(owner, fieldChosen);//special options for chopin removed from owner and everyone else!

        // where to place new advisor on pl's board?
        var fieldForNewAdvisor = await game.PickFieldForType(pl, "advisor", "new advisor");
        await AddCivCard(pl, newcard, fieldForNewAdvisor);
      }
      else if (card.Name == "rome_roman_republic") { pl.RoundResEffects.Add(new Res("stability", 1)); pl.WIC.NumDeployed--; }
      else if (card.Name == "royal_society") { await game.DeployAvailableWorkerTask(pl); }
      else if (card.Name == "shwedagon_pagoda") { pl.RoundResEffects.Add(new Res("stability", 1)); }
      else if (card.Name == "suleiman" || card.Name == "abraham_lincoln")
      {
        await game.CheckoutExtraWorkerTask(game.MainPlayer);
        //var worker = await game.PickExtraWorkerTask();
        //game.Message = game.MainPlayer.Name + " picked " + worker.CostRes.ToCapital() + " worker";
        //game.MainPlayer.CheckOutWorker(worker);//weg
      }
      else if (card.Name == "simon_bolivar") // mod to pick colony field on civ, ist eleganter!
      {
        var colfields = pl.Civ.Fields.Where(x => !x.IsEmpty && x.Card.colony()).ToList();
        var fieldPicked = await game.PickCivFieldOutOf(pl, colfields, "simon bolivar");
        //var ncol = colfields.Count; Debug.Assert(ncol > 0, "ExecuteActionStupid: simon bolivar without colony!!!");
        //Card colcard = ncol > 1 ? await game.PickACard(pl, colfields.Select(x => x.Card).ToList(), "action") : colfields.First().Card;
        //var fieldPicked = colfields.FirstOrDefault(x => x.Card.Name == colcard.Name);
        RemoveCivCard(pl, fieldPicked);
        pl.UpdateResBy("gold", 4); pl.UpdateResBy("coal", 4);
      }




      await Task.Delay(100);
    }

    #region buy / deploy / worker
    public static async Task CheckBuyStupid(Card card, Field field)
    {
      var game = Game.Inst; var pl = game.MainPlayer;

      if (pl.HasCard("america_federalist_party") && card.build()) pl.UpdateResBy("coal", 2);
      if (pl.HasCard("arabia_dyn1") && card.battle() && pl.HasExtraworkers) { var answer = await game.YesNoChoiceTask("checkout worker?"); if (answer) await game.CheckoutExtraWorkerTask(pl); }
      if (pl.HasCard("abu_bakr") && card.battle()) pl.UpdateResBy("book", 2);
      if (pl.HasCard("assassin") && (card.battle())) { foreach (var p in Others(pl)) await game.PayOrRemoveChoice(p, "advisor", "wheat", 3); }
      if (pl.HasCard("chichen_itza") && card.war()) pl.UpdateResBy("vp", 1);
      if (pl.HasCard("great_lighthouse") && card.Price == 3) pl.UpdateResBy("book", 1);
      if (pl.HasCard("himegi_castle") && card.build() && !field.IsEmpty && field.Card.build() && card.Age > field.Card.Age) pl.UpdateResBy("coal", 4);
      if (pl.HasCard("japan_heian_period") && card.golden()) pl.UpdateResBy("book", 4);
      if (pl.HasCard("korea_dyn1") && card.golden())
      {
        var answer = await game.YesNoChoiceTask("hire 2 architects for free?");
        if (answer) await game.AddFreeArchitectsTask(pl, 2);
      }
      if (pl.HasCard("mit") && (card.build()))
      {
        var answer = await game.YesNoChoiceTask("deploy worker to new building for free?");
        if (answer)
        {
          if (pl.Res.n("worker") == 0) { await game.PickWorkerToUndeployTask(); }
          pl.DeployWorker(1);
          card.NumDeployed++;
        }

        pl.UpdateResBy("book", 3);
      }
      if (pl.HasCard("montezuma") && (card.battle() || card.war())) { pl.UpdateResBy("book", 3); }
      if (pl.HasCard("mansa_musa") && card.Price == pl.Res.n("gold")) { pl.UpdateResBy("wheat", 1); pl.UpdateResBy("book", 2); }
      if (pl.HasCard("portugal_kingdom_of_leon") && card.war()) { await game.PickResourceAndGetItTask(new string[] { "wheat", "coal", "book" }, pl.RaidValue, "dynasty effect"); }
      if (pl.HasCard("red_fort") && card.Price == 1) { pl.UpdateResBy("book", 2); }
      if (pl.HasCard("sejong_the_great") && card.golden()) { pl.UpdateResBy("coal", 2); }
      if (pl.HasCard("siwa_oasis") && card.colony()) { pl.UpdateResBy("book", 3); }
      if (pl.HasCard("shaka_zulu") && (card.colony())) { foreach (var p in Others(pl)) p.UpdateResBy("coal", 5); }
      if (pl.HasCard("vikings_varangians") && card.adv()) pl.UpdateResBy("book", 4);

      if (card.NumDeployed > 0) { var p = PlayerThatHas("ethiopia_axumite_kingdom"); await game.PayTask(pl, "gold", 3); p.UpdateResBy("gold", 3); }
      if (card.Name == "genghis_khan") foreach (var p in game.Players) p.Stability -= 3;

      Player plOther = null;
      if ((plOther = PlayerThatHas("arabia_abbasid_caliphate")) != null) { await game.OptionBuyVPTask(plOther); }
      if (card.war() && (plOther = PlayerThatHas("mongolia_golden_horde")) != null && plOther != pl) { plOther.UpdateResBy("gold", card.Price); }
      if ((card.war() || card.battle()) && (plOther = PlayerThatHas("venice_pactum_warmundi")) != null && plOther != pl) { plOther.UpdateResBy("gold", 1); }
    }
    public static async Task CheckBuyGoldenAgeStupid(Player pl, Card card, Res resPicked)
    {
      var game = Game.Inst;

      if (pl.HasCard("carolus_linneaus") && resPicked.Name == "vp") pl.UpdateResBy("vp", 1);

      if (resPicked == null || resPicked.Name != "vp") // execute special card effect
      {
        if (card.Name == "le_vite") { game.Stats.Architects += 3; pl.RoundCardEffects.Add(card); }
        if (card.Name == "uncle_toms_cabin")
        {
          foreach (var p in game.Players)
          {
            foreach (var f in p.NonEmptyBMFields)
            {
              var c = f.Card;
              var deployCost = c.GetDeployCost;
              if (deployCost < 0) deployCost = c.Age;
              if (deployCost == 1) RemoveCivCard(p, f);
            }
          }
        }
      }
      await Task.Delay(100);
    }
    public static int CalcDeployCostStupid(Player pl, Card card)
    {
      var carddeploy = card.GetDeployCost;
      var defaultDeployCost = Game.Inst.Stats.Age;

      int actualDeployCost = carddeploy < 0 ? defaultDeployCost : carddeploy;

      // special cards...

      return actualDeployCost;
    }
    public static int CalcGoldenAgeBonusStupid(Player pl)
    {
      // hier alle karten die golden age bonus geben!
      int gabonus = 0;
      if (pl.HasCard("greece_dyn1")) gabonus += 1;
      if (pl.HasCard("great_library")) gabonus += 1;
      if (pl.HasCard("mali_dyn1")) gabonus += 1;
      if (pl.HasCard("uraniborg")) gabonus += 2;

      return gabonus;
    }
    public static int CalcGoldenAgeBonusForVPStupid(Player pl)
    {
      int gabonus = CalcGoldenAgeBonusStupid(pl);
      if (pl.HasCard("constantinople")) gabonus += 1;

      return gabonus;
    }
    public static int CalcPriceStupid(Card card)
    {
      var game = Game.Inst; var pl = game.MainPlayer;

      if (pl.HasCard("egypt_new_kingdom") && card.battle()) return Math.Max(0, card.BasicCost - 2);
      if (OtherPlayerHas(pl, "hannibal") && card.battle()) return card.BasicCost + 1;
      if (pl.HasCard("portugal_dyn1") && card.BasicCost == 3) return 2;

      return card.BasicCost;
    }
    public static int CalcRaidStupid(Player pl)
    {
      if (pl.HasMilitaryDeployed)
      {
        var raid = pl.Cards.Where(x => x.mil() && x.NumDeployed > 0).Max(x => x.GetRaid);
        if (raid > 0) raid += pl.Cards.Where(x => !x.buildmil()).Sum(x => x.GetRaid);

        return raid;
      }
      return 0;
    }
    public static bool CheckMilminConditionStupid(Player pl, Card card)
    {
      var milmin = card.GetMilmin;

      if (pl.HasCard("cape_of_good_hope")) milmin -= 4;
      if (pl.HasCard("arabia_umayyad_caliphate")) milmin -= 4;


      return pl.Military >= milmin;
    }
    public static void CheckCheckOutExtraWorkerStupid(Player pl, Worker worker)
    {
      if (pl.HasCard("china_ming_dynasty")) pl.UpdateResBy("wheat", 4);
    }
    #endregion

    #region wonder architect turmoil dynasty
    public static int CheckArchitectCostStupid(Player pl, Card card, int baseCost)
    {
      if (pl.RoundCardEffects.Any(x => x.Name == "le_vite")) return baseCost - 1;
      return baseCost;
    }
    public static async Task CheckHireArchitectStupid(Player pl)
    {
      if (pl.HasCard("sistine_chapel")) pl.UpdateResBy("book", 3);
      await Task.Delay(100);
    }
    public static async Task CheckReadyStupid(Field newwonderfield)
    {
      var game = Game.Inst; var plMain = game.MainPlayer;
      var newwonder = newwonderfield.Card;

      await Task.Delay(100);
      if (plMain.HasCard("sphinx") && newwonder.Name != "sphinx") plMain.UpdateResBy("coal", 5);
      if (plMain.HasCard("hatshepsut")) plMain.UpdateResBy("book", 3);
      if (plMain.HasCard("india_mughal_empire")) plMain.UpdateResBy("vp", 1);
      if (plMain.HasCard("america_dyn1") && newwonder.natural()) plMain.UpdateResBy("wheat", 2);
      switch (newwonder.Name)
      {
        case "aurora_borealis": plMain.UpdateResBy("gold", 2); break;
        case "brandenburg_gate": foreach (var pl in PlayersWith("most", "military", game.Players)) { pl.UpdateResBy("gold", 6); pl.UpdateResBy("coal", 6); } break;
        case "colosseum": await game.PayTask(plMain, "wheat", 2); break;
        case "darwins_voyage": plMain.UpdateResBy("gold", 15); break;
        case "hawaii":
          var worker = await game.CheckoutExtraWorkerTask(plMain);
          //var worker = await game.PickExtraWorkerTask();
          //game.Message = game.MainPlayer.Name + " picked " + worker.CostRes.ToCapital() + " worker";
          //game.MainPlayer.CheckOutWorker(worker); //weg
          worker.Cost = 0;
          break;
        case "northwest_passage": for (int i = 0; i < 2; i++) await game.ReturnWorkerTask(plMain); break;
        case "stonehenge": plMain.UpdateResBy("book", 6); plMain.UpdateResBy("wheat", 4); break;
        case "moai_statues": plMain.UpdateResBy("coal", 12); break;
        case "mount_ararat": foreach (var p in Others(plMain)) { await game.PayTask(p, "wheat", 4); } break;
        case "sankore_university": plMain.UpdateResBy("book", 8); break;
        case "south_pole_expedition": await game.PayTask(plMain, "wheat", 5); break;
        case "taj_mahal": plMain.UpdateResBy("book", 15); break;
        case "the_pillar_of_hercules": plMain.RoundResEffects.Add(new Res("military", 7)); break;
        case "uluru": plMain.UpdateResBy("vp", Others(plMain).Count(x => x.HasPassed)); break;
        case "versailles": plMain.RoundResEffects.Add(new Res("stability", -100)); break;
        case "wardenclyffe_tower": plMain.NumActions += 3; break;
        case "siberia":
          // get extra siberia card
          var extra = Card.MakeCard("extra_siberia");
          var fields = plMain.GetPossiblePlacesForType("wic").Where(x => x != newwonderfield).ToList();
          var field = await game.PickCivFieldOutOf(plMain, fields, "extra siberia");
          await AddCivCard(plMain, extra, field);
          break;
        case "terracotta_army":
          foreach (var pl in PlayersWith("least", "stability", game.Players))
          {
            //if (!pl.CanPay(4, "gold")) { game.MainPlayer = pl; await Task.Delay(500); }
            await game.PayTask(pl, "gold", 4);//else pl.Pay(4, "gold");
          }
          game.MainPlayer = plMain;
          break;
        case "titanic":
          foreach (var pl in game.Players)
          {
            await game.PayOrRemoveChoice(pl, "advisor", "gold", 4);
            //if (!pl.HasAdvisor)
            //{
            //  //if (!pl.CanPay(4, "gold")) { game.MainPlayer = pl; await Task.Delay(500); }
            //  await game.PayTask(pl, "gold", 4);
            //}
            //else
            //{
            //  game.MainPlayer = pl; await Task.Delay(500);
            //  var txt = await game.WaitForPickChoiceCompleted(new string[] { "pay 4 gold", "remove advisor" }, "event");
            //  if (txt.Text.StartsWith("pay")) await game.PayTask(pl, "gold", 4);
            //  else pl.RemoveAdvisor();
            //}

          }
          game.MainPlayer = plMain;
          break;
        case "british_museum":
          foreach (var pl in PlayersWith("least", "military", game.Players))
          {
            if (!pl.CanPay(10, "book")) { game.MainPlayer = pl; await Task.Delay(500); }
            await game.PayTask(pl, "book", 10);
          }
          game.MainPlayer = plMain;
          break;
      }

    }
    public static async Task CheckTurmoilStupid(Player pl, bool tookGold)
    {
      if (pl.HasCard("persia_sassanid_empire") && tookGold) return;

      pl.TurmoilsTaken++;
      await Task.Delay(100);
    }
    public static async Task CheckUpgradeDynastyStupid(Player pl, Card newdyn)
    {
      var game = Game.Inst;

      if (newdyn.Name == "mongolia_yuan_dynasty") for (int i = 0; i < 3; i++) await game.CheckoutExtraWorkerTask(pl);

      foreach (var p in game.Players)
      {
        if (p.HasCard("america_democratic_republicans")) p.UpdateResBy("book", 3);
      }
      await Task.Delay(100);
    }
    #endregion

    public static async Task CheckPreActionPhaseStupid()
    {
      var game = Game.Inst;

      var pl = PlayerThatHas("poland_jagellonian_dynasty");
      if (pl != null && pl.Res.n("gold") > 0)
      {
        var plReceiver = pl == game.MainPlayer ? game.Players[1] : game.MainPlayer;
        var plMain = game.MainPlayer; game.MainPlayer = pl;
        game.Caption = "Ok"; game.AnimationQueue.Clear(); game.ContextStack.Clear(); game.ContextInit(ctx.start, game.MainPlayer.Name + ", choose action");
        var answer = await game.YesNoChoiceTask(pl.Name + " pay 1 gold to " + plReceiver.Name + " to take extra action before regular action round?");
        if (answer)
        {

          // pay 1 gold to first (or second) player
          await game.PayTask(pl, "gold", 1); plReceiver.UpdateResBy("gold", 1);

          // do the action:
          game.Caption = "Ok"; game.AnimationQueue.Clear(); game.ContextStack.Clear(); game.ContextInit(ctx.start, game.MainPlayer.Name + ", choose action");
          await game.ActionLoop();
          await game.WaitForAnimationQueueCompleted(); game.DisableAndUnselectAll(); game.PassClicked = game.OkStartClicked = game.CancelClicked = false;
        }
        game.MainPlayer = plMain;
      }

      await Task.Delay(100);
    }
    public static async Task CheckPreActionStupid(bool isFirstTurn)
    {
      var game = Game.Inst; var plMain = game.MainPlayer;
      CalcStabAndMil(plMain);

      if (isFirstTurn && plMain.HasCard("persia_achaimenid_empire") && plMain.GrowthResourcePicked.Name == "worker")
      {
        var answer = await game.YesNoChoiceTask("place new worker for free?");
        if (answer) { game.ActionComplete = true; await game.DeployAvailableWorkerTask(plMain); }
      }

    }
    public static async Task CheckPostActionStupid()
    {
      var game = Game.Inst; var plMain = game.MainPlayer;
      CalcStabAndMil(plMain);

      if (plMain.HasCard("boudica") && plMain.HasMilitaryDeployed) plMain.RemoveAdvisor();
      if (plMain.HasCard("qin_shi_huang") && PlayerHas(plMain, "least", "stability")) plMain.RemoveAdvisor();
      if (plMain.HasCard("harald_hardrada") && PlayerHas(plMain, "least", "military")) plMain.RemoveAdvisor();
      if (plMain.HasCard("great_library") && PlayerHas(plMain, "least", "stability")) RemoveCivCard(plMain, "great_library");
      if (plMain.HasCard("hypatia") && plMain.Stability > 2) RemoveCivCard(plMain, "hypatia");

      await Task.Delay(100);
    }
    public static async Task CheckPostActionPhaseStupid()
    {
      var game = Game.Inst;
      foreach (var pl in game.Players)
      {
        if (pl.CardsBoughtThisRound.Any(x => x.Name == "arabian_nights")) pl.UpdateResBy("book", 2);
        if (pl.HasCard("oresund_dues")) pl.UpdateResBy("coal", 2);
      }

      await Task.Delay(100);
    }

    public static async Task<Res[]> CalcProductionStupid(Player pl)
    {
      var game = Game.Inst;
      List<Res> production = new List<Res>(); List<Res> militarycost = new List<Res>(); List<Res> buildingcost = new List<Res>(); List<Res> extraworkerscost = new List<Res>();
      foreach (var c in pl.Cards)
      {
        var resources = c.GetResources(Res.ProductionResources);
        var factor = c.buildmil() ? c.NumDeployed : 1; if (factor == 0) continue;
        foreach (var res in resources)
        {
          var n = res.Num * factor;
          var resname = res.Name;
          if (res.Num > 0) AddToRes(production, resname, n);
          else if (c.mil()) AddToRes(militarycost, resname, n);
          else AddToRes(buildingcost, resname, n);
        }
      }
      foreach (var w in pl.ExtraWorkers.Where(x => x.IsCheckedOut)) { if (w.CostRes != "military" && w.CostRes != "stability") AddToRes(extraworkerscost, w.CostRes, -w.Cost); }

      if (pl.HasCard("alfred_nobel")) { var n = pl.Cards.Where(x => x.build() && x.Age == 4).Sum(x => x.NumDeployed); AddToRes(production, "coal", 3 * n); }
      if (pl.HasCard("angkor_wat") && PlayerHas(pl, "least", "military")) { AddToRes(production, "book", -4); }
      if (pl.HasCard("anna_komnene") || pl.HasCard("ethiopia_sheba") && pl.CardsBoughtThisRound.Any(x => x.colony())) militarycost.Clear();
      if (pl.HasCard("augustus") && PlayerHas(pl, "most", "military")) AddToRes(production, "coal", 2);
      if (pl.HasCard("benjamin_disraeli") && pl.CardsBoughtThisRound.Any(x => x.colony())) AddToRes(production, "wheat", 8);
      if (pl.HasCard("coffee_house") && pl.PassedLast) AddToRes(production, "book", 2 * pl.GetCard("coffee_house").NumDeployed);
      if (pl.HasCard("cyrus_the_great") && pl.CardsBoughtThisRound.Any(x => x.colony())) AddToRes(production, "gold", 3);
      if (pl.HasCard("china_dyn1") && pl.PassedFirst) AddToRes(production, "wheat", 1);
      if (pl.HasCard("egypt_old_kingdom")) { var c = pl.GetCard("egypt_old_kingdom"); AddToRes(production, "book", c.NumDeployed * 2); if (pl.Defeated && c.NumDeployed > 0) c.NumDeployed--; }
      if (pl.HasCard("eleanor_of_aquitaine") && pl.CardsBoughtThisRound.Any(x => x.colony())) AddToRes(production, "gold", 5);
      if (pl.HasCard("frederic_chopin")) { var resbook = production.FirstOrDefault(x => x.Name == "book"); if (resbook != null) AddToRes(production, "book", resbook.Num * 2); }
      if (pl.HasCard("forbidden_palace") && pl.PassedFirst) AddToRes(production, "vp", 1);
      if (pl.HasCard("hypatia")) { var card = pl.GetCard("hypatia"); card.NumDeployed++; AddToRes(production, "gold", card.NumDeployed); }
      if (pl.HasCard("lin_zexu") && PlayerHas(pl, "least", "military")) { AddToRes(production, "book", -8); }
      if (pl.HasCard("isabella")) { var n = pl.Cards.Where(x => x.colony() && x.Age == 3); AddToRes(production, "gold", 3 * n.Count()); }
      if (pl.HasCard("notre_dame") && PlayerHas(pl, "most", "stability")) { AddToRes(production, "book", 3); }
      if (pl.HasCard("poland_dyn1") && game.Stats.IsWar && pl.Military >= game.Stats.WarLevel) { AddToRes(production, "book", 3); }
      if (pl.HasCard("peter_the_great") && game.Stats.IsWar && pl.Military > game.Stats.WarLevel)
      {
        var res = await game.PickResourceTask(new string[] { "gold", "coal", "book" }, "Peter the Great");
        AddToRes(production, res.Name, 5);
      }
      if (pl.HasCard("rome_roman_empire") && PlayerHas(pl, "most", "stability")) { if (PlayerHas(pl, "most", "military")) AddToRes(production, "vp", 1); else AddToRes(production, "book", 2); }
      if (pl.HasCard("saint_augustine") && PlayerHas(pl, "most", "stability")) { AddToRes(production, "book", 2); }
      if (pl.HasCard("thomas_aquino") && PlayerHas(pl, "most", "stability")) AddToRes(production, "book", 4);
      if (pl.HasCard("tokugawa")) { var n = pl.Cards.Where(x => x.buildmil() && x.Age == 3); AddToRes(production, "coal", 2 * n.Count()); }
      if (pl.HasCard("varanasi")) { var numUndeployedWorkers = pl.Res.n("worker"); AddToRes(production, "book", numUndeployedWorkers); AddToRes(production, "wheat", numUndeployedWorkers); }
      if (pl.HasCard("venice_dyn1") && pl.PassedLast) AddToRes(production, "book", 2);


      await Task.Delay(100);

      //aus den 4 listen mache jetzt 1 liste result
      List<Res> result = production;
      foreach (var res in militarycost) AddToRes(result, res.Name, res.Num);
      foreach (var res in buildingcost) AddToRes(result, res.Name, res.Num);
      foreach (var res in extraworkerscost) AddToRes(result, res.Name, res.Num);

      return result.Where(x => x.Num != 0).ToArray();
    }
    public static async Task CheckPostProductionStupid(Player pl)
    {
      var game = Game.Inst;

      if (pl.HasCard("vikings_dyn1"))
      {
        var plMain = game.MainPlayer;game.MainPlayer = plMain;
        var res = await game.PickResourceTask(new string[] { "wheat", "coal", "gold", "book" }, "penalty");
        foreach (var p in Others(pl)) await game.PayTask(p, res.Name, 1);
        game.MainPlayer = plMain;
      }
      if (pl.HasCard("greece_athens") && PlayerHas(pl, "most", "book")) pl.UpdateResBy("gold", 3);

      await Task.Delay(100);
    }

    public static List<Res> CalcWarPenaltyStupid(Player pl, IEnumerable<Res> reslist)
    {
      if (pl.HasCard("japan_edo_period")) return new List<ations.Res>();

      var result = new List<Res> { new Res("vp", -1) };

      if (pl.HasCard("great_wall")) result.Clear(); //no vp loss to war!
      if (AnyPlayerHas("martin_luther")) AddToRes(result, "wheat", -4);
      if (AnyPlayerHas("mongolia_dyn1")) foreach (var res in reslist) res.Num += 2;

      foreach (var res in reslist) { var cost = res.Num - pl.Stability; if (cost > 0) { AddToRes(result, res.Name, -cost); } }
      return result;
    }

    public static async Task HandleSimpleEventStupid(XElement ev, Card evcard)
    {
      var game = Game.Inst;
      var plSelected = CalcAffects(ev, game.Players.ToList());
      var resEffects = GetResourcesForRule(ev);
      var effectAction = ev.astring("effect");

      foreach (var pl in plSelected)
      {
        game.MainPlayer = pl;

        if (!string.IsNullOrEmpty(effectAction)) await ExecuteEffectAction(effectAction, ev.astring("eparam"), pl);//effectAction?.Invoke(this, MainPlayer);

        foreach (var resEff in resEffects) await ApplyResEffectTask(resEff);

        await game.WaitForAnimationQueueCompleted();

        //if (pl != plSelected.Last()) { Message = "next player..."; await WaitForButtonClick(); }
      }

    }
    public static async Task HandleEventCardStupid(Card evcard)
    {
      var game = Game.Inst;
      var pls = game.Players.ToList();
      if (evcard.Name == "absolute_monarchy")
      {
        foreach (var pl in pls.Where(x => x.Cards.Any(y => y.adv() && y.Age < 3))) await game.PayTask(pl, "book", 4);
        foreach (var pl in PlayersWith("least", "stability"))
        {
          var chose = "remove";
          if (pl.Res.n("gold") >= 3 && pl.HasAdvisor)
          {
            var choice = await game.PickTextChoiceTask(new string[] { "pay 3 gold", "remove advisor" }, "event");
            chose = choice.Text.StringBefore(" ");
          }
          else if (!pl.HasAdvisor) chose = "pay";
          if (chose == "pay") await game.PayTask(pl, "gold", 3); else pl.RemoveAdvisor();
        }
      }
      else if (evcard.Name == "african_slave_trade")
      {
        foreach (var pl in PlayersWith("most", "stability"))
        {
          var answer = await game.YesNoChoiceTask("go last for 6 gold?");
          if (answer) { pl.UpdateResBy("gold", 6); await ExecuteEffectAction("go_last", null, pl); }
        }
        foreach (var pl in PlayersWith("least", "military")) await game.PayTask(pl, "vp", 1);
      }
      else if (evcard.Name == "american_revolution") //************************** HIER!!!
      {
        foreach (var pl in PlayersWith("least", "military")) { await ExecuteEffectAction("remove_colony_if_none_pay_vp", null, pl); }
        foreach (var pl in PlayersWith("least", "stability")) { var n = pl.NumAdvisors; if (n >= 1) { pl.RemoveAdvisor(); } else { await game.PayTask(pl, "vp", 2); } }
      }
      else if (evcard.Name == "bread_and_games")
      {
        foreach (var pl in PlayersWith("most", "stability")) pl.UpdateResBy("coal", 4);
        foreach (var pl in pls.Where(x => x.Res.n("wheat") >= 4))
        {
          var answer = await game.YesNoChoiceTask("Trade 4 wheat to get 6 books?");
          if (answer) { pl.UpdateResBy("wheat", -4); pl.UpdateResBy("book", 6); }
        }
      }
      else if (evcard.Name == "benedictine_rule")
      {
        foreach (var pl in PlayersWith("most", "stability")) pl.UpdateResBy("wheat", 4);
        foreach (var pl in pls.Where(x => x.Res.n("wheat") >= 3))
        {
          var answer = await game.YesNoChoiceTask("Trade 3 wheat to get 5 gold?");
          if (answer) { pl.UpdateResBy("wheat", -3); pl.UpdateResBy("gold", 5); }
        }
      }
      else if (evcard.Name == "black_death")
      {
        foreach (var pl in pls) await game.ReturnWorkerTask(pl);
        foreach (var pl in PlayersWith("most", "military")) pl.UpdateResBy("book", 3);
      }
      else if (evcard.Name == "ecological_collapse")
      {
        foreach (var pl in pls)
        {
          var chose = "go";
          if (pl.Res.n("wheat") >= 2)
          {
            var choice = await game.PickTextChoiceTask(new string[] { "pay 2 wheat", "go last" }, "event");
            chose = choice.Text.StringBefore(" ");
          }
          if (chose == "pay") await game.PayTask(pl, "wheat", 2); else await ExecuteEffectAction("go_last", null, pl);
        }
        foreach (var pl in PlayersWith("least", "stability")) await game.PayTask(pl, "book", 4);
      }
      else if (evcard.Name == "fourth_crusade")
      {
        foreach (var pl in PlayersWith("most", "military").Where(x => x.Res.n("gold") >= 3))
        {
          var answer = await game.YesNoChoiceTask("pay 3 gold for 1 vp?");
          if (answer) { pl.UpdateResBy("gold", -3); pl.UpdateResBy("vp", 1); }
        }
        foreach (var pl in PlayersWith("least", "military")) await game.PayTask(pl, "book", 4);
        foreach (var pl in PlayersWith("most", "stability")) await ExecuteEffectAction("regain_war_loss", null, pl);
      }
      else if (evcard.Name == "hellenism")
      {
        foreach (var pl in PlayersWith("most", "military")) pl.UpdateResBy("coal", 2);
        foreach (var pl in PlayersWith("not_most", "military")) await game.PayTask(pl, "wheat", 2);
        foreach (var pl in pls.Where(x => x.HasMilitaryDeployed))
        {
          var plMain = game.MainPlayer; game.MainPlayer = pl;

          var answer = await game.YesNoChoiceTask("undeploy military for 2 books each?");
          if (answer) await game.PickUndeployForEachTask(pl, new string[] { "military" }, "book", 2);

          game.MainPlayer = plMain;
        }
      }
      if (evcard.Name == "little_ice_age")
      {
        foreach (var pl in PlayersWith("most", "stability")) pl.UpdateResBy("book", 6);
        foreach (var pl in pls)
        {
          var chose = "pay 3";
          if (pl.Res.n("wheat") >= 3 && pl.Books >= 5)
          {
            var choice = await game.PickTextChoiceTask(new string[] { "pay 3 wheat", "pay 5 books" }, "event");
            chose = choice.Text.StringBeforeLast(" ");
          }
          else if (pl.Books >= 5) chose = "pay 5";
          if (chose == "pay 3") await game.PayTask(pl, "wheat", 3); else await game.PayTask(pl, "book", 5);
        }
      }
      if (evcard.Name == "magellans_expedition")
      {
        foreach (var pl in pls) { var ncol = pl.CardsBoughtThisRound.Count(x => x.colony()); pl.UpdateResBy("gold", 5 * ncol); }
        foreach (var pl in PlayersWith("most", "stability").Where(x => x.HasWIC))
        {
          var answer = await game.YesNoChoiceTask("hire architect for free?");
          if (answer) await game.AddFreeArchitectsTask(pl, 1);
        }
        foreach (var pl in PlayersWith("not_most", "stability")) await game.PayTask(pl, "gold", 2);
      }
      if (evcard.Name == "sinking_of_the_vasa")
      {
        var leaststab = PlayersWith("least", "stability");
        var leastmil = PlayersWith("least", "military");
        foreach (var pl in pls)
        {
          if (leaststab.Contains(pl) && leastmil.Contains(pl))
          {
            var chose = "pay 3";
            if (pl.Res.n("gold") >= 3 && pl.Res.n("wheat") >= 5 && pl.Books >= 10)
            {
              var choice = await game.PickTextChoiceTask(new string[] { "pay 3 gold and 5 wheat", "pay 10 books" }, "event");
              chose = choice.Text;
            }
            else if (pl.Books >= 10) chose = "pay 10";
            if (chose.StartsWith("pay 3")) { await game.PayTask(pl, "gold", 3); await game.PayTask(pl, "wheat", 5); } else await game.PayTask(pl, "book", 10);
          }
          else if (leaststab.Contains(pl)) await game.PayTask(pl, "gold", 3);
          else if (leastmil.Contains(pl)) await game.PayTask(pl, "wheat", 5);
        }
      }
      else if (evcard.Name == "pax_romana")
      {
        foreach (var pl in PlayersWith("most", "military")) pl.UpdateResBy("vp", 1);
        foreach (var pl in PlayersWith("most", "stability").Where(x => x.HasExtraworkers))
        {
          var answer = await game.YesNoChoiceTask("take out 1 worker and get 3 wheat with it?");
          if (answer) { await game.CheckoutExtraWorkerTask(pl); pl.UpdateResBy("wheat", 3); }
        }
      }
      else if (evcard.Name == "peace_of_god")
      {
        foreach (var pl in PlayersWith("most", "military")) { if (game.Stats.IsWar && !pl.CardsBoughtThisRound.Any(x => x.war())) pl.UpdateResBy("book", 4); }
        foreach (var pl in pls) { var n = pl.Cards.Count(x => x.colony() && x.Age >= 2); pl.UpdateResBy("vp", n); }
      }
      else if (evcard.Name == "rigveda")
      {
        foreach (var pl in PlayersWith("most", "wheat")) pl.UpdateResBy("wheat", 4);
        foreach (var pl in pls) { var n = pl.CardsBoughtThisRound.Count(x => x.war() || x.battle()); pl.UpdateResBy("vp", n); }
      }
      else if (evcard.Name == "sea_peoples")
      {
        foreach (var pl in PlayersWith("least", "military"))
        {
          string chose = "pay";
          if (pl.HasWIC && pl.WIC.NumDeployed > 0 && pl.Res.n("vp") >= 1)
          {
            var choice = await game.PickTextChoiceTask(new string[] { "remove architect", "pay 1 vp" }, "event");
            chose = choice.Text.StringBefore(" ");
          }
          else if (pl.HasWIC && pl.WIC.NumDeployed > 0) { chose = "remove"; }
          if (chose == "pay") await game.PayTask(pl, "vp", 1); else pl.WIC.NumDeployed -= 1;
        }
        var safe = PlayersWith("most", "military").ToList();
        var safe1 = PlayersWith("most", "stability").ToList();
        safe.AddRange(safe1);
        foreach (var pl in pls.Where(x => !safe.Contains(x))) await game.PayTask(pl, "vp", 1);
      }
      else if (evcard.Name == "sokoto_caliphate")
      {
        foreach (var pl in pls.Where(x => x.Res.n("wheat") >= 4))
        {
          var answer = await game.YesNoChoiceTask("Trade 4 wheat to get 8 gold?");
          if (answer) { pl.UpdateResBy("wheat", -4); pl.UpdateResBy("gold", 8); }
        }
        foreach (var pl in PlayersWith("least", "stability")) await game.PayTask(pl, "gold", 7);
      }
      else if (evcard.Name == "weltpolitik")
      {
        foreach (var pl in Filter("if_at_least_1_industrial_colony", null, pls)) { int n = pl.Colonies.Count(y => y.Card.Age >= 4); pl.UpdateResBy("vp", n); }
        foreach (var pl in PlayersWith("least", "stability")) await game.ReturnWorkerTask(pl);
      }
    }

    public static int CalcFamineStupid(Player pl, int famine)
    {
      var game = Game.Inst;

      if (PlayerHas(pl, "least", "military") && OtherPlayerHas(pl, "mali_songhai_empire")) { famine += 3; }

      return famine;
    }
    public static async Task CheckEndOfRoundOrAgeStupid()
    {
      var game = Game.Inst;
      var endage = Game.Inst.Stats.IsEndOfAge;
      foreach (var pl in game.Players)
      {
        if (pl.HasCard("korea_joseon_kingdom"))
        {
          var card = pl.GetCard("korea_joseon_kingdom");
          var reslist = card.Tag as List<Res>;
          if (reslist != null) { foreach (var res in reslist) pl.UpdateResBy(res.Name, 2 * res.Num); }
          await game.WaitForAnimationQueueCompleted();
          card.Tag = card.ListOfResources = null;
        }
        if (endage && pl.HasCard("solomons_temple")) pl.UpdateResBy("vp", 1);
        if (endage && pl.HasCard("vesuvius")) await game.ReturnWorkerTask(pl);
      }

      if (!endage) return;

      // book scoring
      var pls = game.Players.OrderByDescending(x => x.Books).ToList();
      int count = pls.Count;
      while (count > 0)
      {
        int maxbooks = pls[0].Books;
        var newplayers = pls.TakeWhile(x => x.Books == maxbooks).ToList();
        pls = pls.Skip(newplayers.Count).ToList();
        foreach (var pl in newplayers) pl.UpdateResBy("vp", pls.Count);
        count = pls.Count;
      }



    }
    public static double CalcScoreStupid(Player pl)
    {
      var game = Game.Inst;
      int vp = pl.Res.n("vp");
      int resnum = pl.Res.List.Where(x => Res.ProductionResources.Contains(x.Name)).Sum(x => x.Num);

      var cardvps = pl.Cards.Sum(x => x.GetVP);
      vp += cardvps;

      CalcStabAndMil(pl, true);
      vp += pl.Military / 10;
      resnum += pl.Military % 10;

      vp += pl.Stability / 10;
      resnum += pl.Stability % 10;

      foreach (var bm in pl.Cards.Where(x => x.buildmil()).ToList())
      {
        var scoring = bm.GetScoringArray;
        for (int i = 0; i < scoring.Length; i++) { if (bm.NumDeployed > i) vp += scoring[i]; }
      }


      // special cards
      if (pl.HasCard("potala_palace")) vp += pl.Cards.Count(x => x.adv());
      if (pl.HasCard("big_ben")) vp += pl.Cards.Count(x => x.colony() && x.Age == 4);
      if (pl.HasCard("statue_of_liberty")) vp += PlayerHas(pl, "most", "stability") ? 2 : 0;
      if (pl.HasCard("grand_canyon")) vp += pl.Cards.Count(x => x.natural());
      if (pl.HasCard("titusville")) resnum += pl.Res.n("coal");

      vp += resnum / 10;
      resnum = resnum % 10;
      return ((double)resnum) / 10 + vp;
    }

    #region add remove
    public static async Task AddCivCardStupid(Player pl, Field field, Card card)
    {
      var game = Game.Inst;
      if (pl.HasCard("venice_domains_of_the_sea") && card.colony()) { var answer = await game.YesNoChoiceTask("discard colony for 2 gold and 1 vp?"); if (answer) { pl.UpdateResBy("gold", 2); pl.UpdateResBy("vp", 1); return; } }
      AddCivCardStupidSync(pl, field, card);
    }
    //  if (pl.HasCard("emperor") && card.adv()) { pl.GetCard("emperor").NumDeployed++; return; }

    //  if (card.adv() && !field.IsEmpty && field.Card.Name == "zhu_xi") { pl.ExtraAdvisor = field.Card; }
    //  else { RemoveCivCard(pl, field); if (card.adv() && pl.HasExtraAdvisor) pl.ExtraAdvisor = null; }

    //  pl.Cards.Add(card);
    //  field.Card = card;
    //  card.NumDeployed = 0;
    //  if (card.natural()) field.TypesAllowed.Clear();

    //  if (card.Name == "frederic_chopin") AddSpecialOption(Others(pl), card);
    //  if (card.Name == "porcelain_tower") { field.TypesAllowed.Add("advisor"); }
    //  if (card.Name == "poland_polish-lithuanian_commonwealth")
    //  {
    //    foreach (var f in pl.Civ.Fields.Where(x => x.TypesAllowed.Contains("wic"))) f.TypesAllowed.Add("advisor");
    //    AddSpecialOption(Others(pl), card);
    //  }
    //  if (card.Name == "portugal_portugese_empire") { foreach (var f in pl.Civ.Fields.Where(x => x.TypesAllowed.Contains("wic"))) f.TypesAllowed.Add("colony"); }
    //}
    public static void AddCivCardStupidSync(Player pl, Field field, Card card)
    {
      if (pl.HasCard("emperor") && card.adv()) { pl.GetCard("emperor").NumDeployed++; return; }

      if (card.adv() && !field.IsEmpty && field.Card.Name == "zhu_xi") { pl.ExtraAdvisor = field.Card; }
      else { RemoveCivCard(pl, field); if (card.adv() && pl.HasExtraAdvisor) pl.ExtraAdvisor = null; }

      pl.Cards.Add(card);
      field.Card = card;
      card.NumDeployed = 0;
      if (card.natural()) field.TypesAllowed.Clear();

      if (card.Name == "frederic_chopin") AddSpecialOption(Others(pl), card);
      if (card.Name == "porcelain_tower") { field.TypesAllowed.Add("advisor"); }
      if (card.Name == "poland_polish-lithuanian_commonwealth")
      {
        foreach (var f in pl.Civ.Fields.Where(x => x.TypesAllowed.Contains("wic"))) f.TypesAllowed.Add("advisor");
        AddSpecialOption(Others(pl), card);
      }
      if (card.Name == "portugal_portugese_empire") { foreach (var f in pl.Civ.Fields.Where(x => x.TypesAllowed.Contains("wic"))) f.TypesAllowed.Add("colony"); }
    }
    public static void RemoveCivCardStupid(Player pl, Field field)
    {
      if (field.IsEmpty) return;
      var card = field.Card;
      if (card.buildmil()) pl.UndeployWorker(card.NumDeployed);
      field.Card = Card.MakeEmptyCard(field);
      pl.Cards.Remove(card);

      if (card.Name == "porcelain_tower" && !pl.HasCard("poland_polish-lithuanian_commonwealth")) { field.TypesAllowed.Remove("advisor"); }
      if (card.Name == "frederic_chopin") { RemoveSpecialOption(card.Name); }
      if (card.Name == "ethiopia_axumite_kingdom") { var f = card.Tag as Field; if (!f.IsEmpty) f.Card.NumDeployed = 0; }
      if (card.Name == "poland_polish-lithuanian_commonwealth") { foreach (var f in pl.Civ.Fields.Where(x => x.TypesAllowed.Contains("wic"))) if (f.IsEmpty || f.Card.Name != "porcelain_tower") f.TypesAllowed.Remove("advisor"); RemoveSpecialOption(card.Name); }
      if (card.Name == "portugal_portugese_empire") { foreach (var f in pl.Civ.Fields.Where(x => x.TypesAllowed.Contains("wic"))) f.TypesAllowed.Remove("colony"); }
    }
    public static void AddWICStupid(Player pl, Card card)
    {
      var field = pl.WICField;
      RemoveWIC(pl);
      if (card.natural()) { pl.RoundsToWaitForNaturalWonder = card.GetTurns; }
      field.Card = card;

    }
    public static void RemoveWICStupid(Player pl)
    {
      var wicfield = pl.WICField;
      if (!wicfield.IsEmpty) wicfield.Card = Card.MakeEmptyCard(wicfield);
    }

    public static void AddSpecialOption(IEnumerable<Player> pls, Card card)
    {
      Choice choice = new Choice();
      if (card.Name == "frederic_chopin")
      {
        choice.Tag = card;
        choice.Text = "buy Chopin";
        choice.IsSelectable = true;
      }
      if (card.Name == "poland_polish-lithuanian_commonwealth")
      {
        choice.Tag = card;
        choice.Text = "buy Advisor";
        choice.IsSelectable = true;
      }
      foreach (var pl in pls) pl.SpecialOptions.Add(choice);
    }
    //public static void TransferSpecialOption(string cardname, Player fromPl, Player toPl)
    //{
    //  var option = GetSpecialOption(cardname, fromPl);
    //  Debug.Assert(option != null, "TransferSpecialOption: option is null!!!");
    //  fromPl.SpecialOptions.Remove(option);
    //  toPl.SpecialOptions.Add(option);

    //}
    public static void RemoveSpecialOption(string cardname) { foreach (var pl in Game.Inst.Players) RemoveSpecialOption(cardname, pl); }
    public static Choice GetSpecialOption(string cardname, Player pl) { return pl.SpecialOptions.FirstOrDefault(x => x.Tag != null && x.Tag is Card && (x.Tag as Card).Name == cardname); }
    public static void RemoveSpecialOption(string cardname, Player pl)
    {
      var option = GetSpecialOption(cardname, pl);// pl.SpecialOptions.FirstOrDefault(x => x.Tag != null && x.Tag is Card && (x.Tag as Card).Name == cardname);
      if (option != null)
      {
        pl.SpecialOptions.Remove(option);
      }
    }
    #endregion

    public static bool WonderAdvisorsAvailableOnOwnerOf(Card card)
    {
      // get owner of this card
      var owner = PlayerThatHas(card.Name);
      var ownerFields = owner.Civ.Fields.Where(x => x.TypesAllowed.Contains("wic")).ToList();
      var ownerFieldsWithAdvisors = ownerFields.Where(x => !x.IsEmpty && x.Card.adv()).ToList();
      return ownerFieldsWithAdvisors.Count > 0;
    }




    #region unused

    public static async Task ExecuteEffectForPlayerStupid(XElement ev, Player pl, Card card)
    {
      await Task.Delay(100);
    }
    public static List<string> BuyResourcesStupid(Card card)
    {
      //card effects
      // there could be cards saying you can pay battle cards with wheat in addition... 
      return new List<string> { "gold" };
    }

    //public static List<Player> CalcPlayersDefeatedStupid()
    //{
    //  var game = Game.Inst;
    //  var result = new List<Player>();
    //  foreach (var pl in game.Players)
    //  {
    //    var mil = pl.Military;


    //    if (mil < game.Stats.WarLevel) result.Add(pl);
    //  }
    //  return result;
    //}
    //public static async Task Check(string timing, Card card = null, Res res = null, Field from = null, Field to = null) { }
    #endregion
  }
}
