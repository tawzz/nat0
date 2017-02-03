using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using ations;
using System.Xml.Linq;

namespace ations
{
  public partial class Game
  {
    public async void GameLoop()
    {
      try
      {
        IsOkStartEnabled = false; IsRunning = true;
        while (Stats.Round < rounds)
        {
          TestOrPlayMode(); Title = TestKey; iphase = 0; State.RoundBegin(); RoundBegin();

          if (SwitchRoundAndAgeOn) { Stats.UpdateRound(); Stats.UpdateAge(); await WaitForRoundMarkerAnimationCompleted(); }

          if (SwitchProgressOn) Progress.Deal(); CustomizeProgress?.Invoke();

          if (SwitchGrowthOn) { TestPreGrowth?.Invoke(); await GrowthPhaseTask(); TestPostGrowth?.Invoke(); }
          CustomizeGrowth?.Invoke();

          if (SwitchNewEventOn) { TestPreNewEvent?.Invoke(); await Checker.PickEventCard(); TestPostNewEvent?.Invoke(); }
          CustomizeEvent?.Invoke();

          if (SwitchActionOn) { TestPreAction?.Invoke(); await PlayerActionTask(); TestPostAction?.Invoke(); }

          if (SwitchProductionOn) { TestPreProduction?.Invoke(); await ProductionTask(); TestPostProduction?.Invoke(); }
          CustomizeProduction?.Invoke();

          if (SwitchOrderOn && !NoChangeInTurn) PlayerOrder();

          if (SwitchWarOn) { TestPreWar?.Invoke(); await WarResolutionTask(); TestPostWar?.Invoke(); }

          if (SwitchEventOn) { TestPreEventResolution?.Invoke(); await EventResolutionTask(); TestPostEventResolution?.Invoke(); }

          if (SwitchFamineOn) await FamineTask();

          await Checker.CheckEndOfAge();

          if (ShowScore || IsTesting) Scoring();
          if (TestCheckFailed() || stopAfterTest) await WaitForButtonClick();

          TestCleanup?.Invoke(); iround++; iplayer = 0; iphase = 0; Stats.UpdateWarPosition(); //cleanup
        }
        Message = "Game Over"; ShowScore = true; Scoring(); await WaitForButtonClick(); // LongMessage = Message = "TEST FAILED!"; IsTesting = false;
      }
      catch (Exception e)
      {
        CanInitialize = true;
        LongMessage = "interrupt!!!: " + e.Message;

      }
    }

    void RoundBegin()
    {
      LongMessage = "round " + iround; Message = "preparing for player actions...";
      iphase = 0;

      foreach (var pl in Players)
      {
        pl.HasPassed = false;
        pl.TurmoilsTaken = 0;
        pl.RoundsToWaitForNaturalWonder = 0;
        pl.Defaulted.Clear();
        pl.RoundResEffects.Clear();
        pl.RoundCardEffects.Clear();
        pl.CardsBoughtThisRound.Clear();
        pl.WarLoss.Clear();
        foreach (var card in pl.Cards) { card.ActionTaken = 0; }
        Checker.CalcStabAndMil(pl);
      }
      if (PassOrder == null) PassOrder = new List<Player>(); else PassOrder.Clear();
      NoChangeInTurn = false;
      iplayer = iturn = 0;
      MainPlayer = Players[iplayer];
    }

    public void Scoring()
    {
      foreach (var pl in Players) pl.Score = Checker.CalcScore(pl);

      var plscore = Players.OrderBy(x => x.Score).ToList();
      LongMessage = "The Winner is " + plscore.Last().Name + "!!!!!!!!!!!!!!!!!!!!!!";
    }

    async Task GrowthPhaseTask()
    {
      //growth phase --------------------------
      Debug.Assert(IsOkStartEnabled == false, "Start growth phase with button enabled!!!");
      //Title = "Round " + (ird + 1); LongMessage = "growth phase is starting...";

      Debug.Assert(iplayer == 0, "growth phase starting with ipl != 0!");
      while (iplayer < NumPlayers)
      {
        MainPlayer = Players[iplayer];

        // just 1 growth pick action for now

        var reslist = new List<string> { "wheat", "coal", "gold" };
        if (MainPlayer.ExtraWorkers.Count(x => !x.IsCheckedOut) > 0) reslist.Add("worker");
        var num = LevelGrowth[MainPlayer.Level];

        if (Interrupt) throw (new Exception("STOPPED BY PLAYER!"));

        var resPicked = await PickResourceAndGetItTask(reslist, num, "growth");

        await Checker.CheckGrowth(resPicked);

        await WaitForAnimationQueueCompleted(1);

        iplayer++;
      }
      LongMessage = "growth phase ending..."; await Task.Delay(longDelay); LongMessage = "growth phase over!"; //Console.WriteLine("\t" + LongMessage);
    }

    async Task PlayerActionTask()
    {
      while (!MainPlayer.HasPassed)
      {
        MainPlayer.NumActions = await Checker.CalcNumActions(iturn == 0);

        while (iaction < MainPlayer.NumActions && !MainPlayer.HasPassed)
        {
          Caption = "Ok"; AnimationQueue.Clear(); ContextStack.Clear(); ContextInit(ctx.start, MainPlayer.Name + ", choose action");

          await ActionLoop();

          await WaitForAnimationQueueCompleted();

          iaction++; DisableAndUnselectAll(); PassClicked = OkStartClicked = CancelClicked = false;
        }
        iaction = 0;
        SwitchToNextPlayer();
      }
      iplayer = 0;
      await Checker.CheckPostActionPhase();
    }

    public async Task ActionLoop()
    {
      int testCounter = 0; ActionComplete = false; State.ActionBegin();
      while (!ActionComplete)
      {
        UpdateUI();
        await WaitForThreeButtonClick();

        if (CancelClicked) { Message = "Action Canceled!"; ClearSteps(); ActionComplete = false; await Task.Delay(longDelay); }
        else if (PassClicked) { Message = MainPlayer.Name + " Passed"; PassOrder.Add(MainPlayer); DisableAndUnselectAll(); await Task.Delay(longDelay); MainPlayer.HasPassed = true; ActionComplete = true; }
        else if (ActionComplete)
        {  //achtung: reihenfolge wichtig bei folgenden clauses
          if (ArchitectSelected) { await TakeArchitectTask(); }
          else if (TurmoilSelected) { await TakeTurmoilTask(); }
          else if (IsCardActivation) { Debug.Assert(!(Step.Obj as Field).IsEmpty, "PerformCardActionTask: empty field!"); await PerformCardActionTask((Step.Obj as Field).Card); }
          else if (IsSpecialOptionActivation) { Debug.Assert(Step.Obj is Choice, "PerformSpecialOptionTask: step has no choice!!!!"); await PerformCardActionTask((Step.Obj as Choice).Tag as Card); }
          else if (ProgressField != null) { await BuyProgressCardTask(); }
          else if (WorkerSelected && DeployTarget != null) { DeployAvailableWorker(); }
          else if (DeploySource != null && DeployTarget != null) { DeployFromField(); }
        }
        else LongMessage = "action incomplete... please complete your action " + (++testCounter);
      }
      State.ActionEnd(); await Checker.CheckPostAction();
    }

    public async Task PerformCardActionTask(Card card) { await Checker.ExecuteAction(MainPlayer, card); card.ActionTaken++; }

    public void SwitchToNextPlayer()
    {
      var currentIndex = MainPlayer.Index;
      var plarr = Players.SkipWhile(x => x != MainPlayer).Skip(1).SkipWhile(x => x.HasPassed).ToArray();
      MainPlayer = plarr.FirstOrDefault() ?? Players.SkipWhile(x => x.HasPassed).FirstOrDefault() ?? Players[0];
      var newIndex = MainPlayer.Index;
      if (newIndex <= currentIndex) iturn++; //next turn round starts
    }

    public async Task ProductionTask()
    {
      LongMessage = "round " + iround + " production is starting..."; iphase = 2; Message = "Production...";
      Debug.Assert(iplayer == 0, "production phase not starting with ipl != 0!");
      foreach (var pl in Players) Checker.CalcStabAndMil(pl); // brauch ich dann auch in War und Event resolution

      ChangesInResources.Clear();
      ShowChangesInResources = true;
      while (iplayer < NumPlayers)
      {
        MainPlayer = Players[iplayer];

        await Task.Delay(shortDelay);

        var netProduction = await Checker.CalcProduction(MainPlayer);
        var getting = netProduction.Where(x => x.Num > 0).ToList();
        var needToPay = netProduction.Where(x => x.Num < 0).ToList();
        foreach (var res in getting)
        {
          await PayTask(MainPlayer, res.Name, -res.Num);
          ChangesInResources.Add(res);
          await Task.Delay(shortDelay);
          await WaitForAnimationQueueCompleted();
        }
        foreach (var res in needToPay)
        {
          await PayTask(MainPlayer, res.Name, -res.Num);
          ChangesInResources.Add(res);
          await Task.Delay(shortDelay);
          await WaitForAnimationQueueCompleted();
          if (MainPlayer.IsBroke) break;
        }

        iplayer++;
        if (iplayer < NumPlayers) { Message = "next player..."; }//if (!IsTesting || stopAfterTest) await WaitForButtonClick(); }
        ChangesInResources.Clear();
      }
      iplayer = 0;
      LongMessage = "production phase ending..."; await Task.Delay(longDelay); LongMessage = "production phase over!";// Console.WriteLine("\t" + LongMessage);
      ShowChangesInResources = false;

      await Task.Delay(longDelay);
    }

    public void PlayerOrder()
    {
      var plarr = Players.OrderBy(x => x.Military).Reverse().ToArray();
      Players.Clear();
      foreach (var pl in plarr) Players.Add(pl);
      for (int i = 0; i < Players.Count; i++) Players[i].Index = i;
      MainPlayer = Players[0];

      //RandomPlayerOrder();
    }

    public async Task WarResolutionTask()
    {
      if (!Stats.IsWar) return;

      //Title = "Round " + (ird + 1) + ": War";
      LongMessage = "round " + iround + " war is being resolved..."; iphase = 2; Message = "War Resolution...";
      Debug.Assert(iplayer == 0, "war resolution phase not starting with ipl != 0!");

      var warcard = Stats.WarCard;
      var xel = warcard.X;
      var reslist = warcard.GetResources().ToList();

      foreach (var pl in Players) Checker.CalcStabAndMil(pl);
      var pls = Players.Where(x => x.Military < Stats.WarLevel).ToList(); 

      ChangesInResources.Clear();
      foreach (var pl in pls)
      {
        MainPlayer = pl; await Task.Delay(longDelay); Message = MainPlayer.Name + ", you lost in war!";
        var resBefore = MainPlayer.GetResourceSnapshot();

        //first show production
        ShowChangesInResources = true;

        var totalPenalty = Checker.CalcWarPenalty(pl, reslist);//penalty is calculated negative

        foreach (var res in totalPenalty)
        {
          ChangesInResources.Add(res);
          await PayTask(MainPlayer, res.Name, -res.Num);
          //await Task.Delay(shortDelay);
          //await WaitForAnimationQueueCompleted();
          //var resAfter = MainPlayer.GetResourceSnapshot();
          var resDiff = pl.GetResDiff(resBefore, Res.WarResources);
          //var loss = resBefore.Zip(resAfter, (x, y) => new Res(x.Name, x.Num - y.Num)).ToList();
          MainPlayer.WarLoss = resDiff.Keys.Select(x => new Res(x, -resDiff[x])).Where(x => x.Num != 0).ToList();
          if (MainPlayer.IsBroke) break;
        }
        ChangesInResources.Clear(); ShowChangesInResources = false;

        if (pl != pls.Last()) { Message = "next player..."; }//await WaitForButtonClick(); }
      }
      Message = "war resolution done...click ok to continue"; //await WaitForButtonClick();
      LongMessage = "war resolution ending..."; await Task.Delay(longDelay); LongMessage = "war resolution phase over!";// Console.WriteLine("\t" + LongMessage);
      iplayer = 0;
    }

    public async Task FamineTask()
    {
      var card = Stats.EventCard;
      var xel = card.X;
      var famine = xel.aint("famine");
      if (famine == 0) return;
      //{
      //  Message = "no famine this time!"; await WaitForButtonClick(); return;
      //}
      Message = "famine is " + famine; //await WaitForButtonClick();
      iplayer = 0;
      while (iplayer < NumPlayers)
      {
        MainPlayer = Players[iplayer];

        await Task.Delay(shortDelay);
        await PayTask(MainPlayer, "wheat", famine);

        await WaitForAnimationQueueCompleted();

        iplayer++;
        //if (iplayer < NumPlayers) { Message = "next player..."; await WaitForButtonClick(); }
      }
      iplayer = 0;

    }

    public async Task EventResolutionTask()
    {
      LongMessage = "round " + iround + " event is being resolved..."; iphase = 2; Message = "Event Resolution..."; Debug.Assert(iplayer == 0, "event resolution phase not starting with ipl != 0!");

      var card = Stats.EventCard;
      var xel = card.X;
      var events = xel.Elements().ToList();
      var simpleEvents = events.Where(x => x.Name.ToString() == "e").ToList();
      var complexEvents = events.Where(x => x.Name.ToString() != "e").ToList();
      var pls = Players.ToList();

      if (complexEvents.Count > 0) { await Checker.HandleEventCard(card); return; } //jede karte mit complex event wird gleich als ganzes gehandled

      foreach (var ev in simpleEvents) 
      {
        await Checker.HandleSimpleEvent(ev, card);

        //if (ev != events.Last()) { Message = "next event..."; await WaitForButtonClick(); }
      }
    }

    //async Task HandleSimpleEvent(XElement ev)
    //{
    //  LongMessage = "event: " + ev.ToString() + "... start!"; await WaitForButtonClick();

    //  var plSelected = CalcPlayersAffected(ev);
    //  var resEffects = GetResourcesForRule(ev);
    //  var effectAction = GetSpecialEffect(ev);

    //  foreach (var pl in plSelected)
    //  {
    //    MainPlayer = pl; //switch ui to this player
    //    await Task.Delay(longDelay);

    //    effectAction?.Invoke(this, MainPlayer);

    //    foreach (var rtuple in resEffects) await ApplyResEffectTask(rtuple);

    //    await Task.Delay(shortDelay);
    //    await WaitForAnimationQueueCompleted();

    //    if (pl != plSelected.Last()) { Message = "next player..."; await WaitForButtonClick(); }
    //  }
    //}


    bool CheckResEffects(Player pl, IEnumerable<Res> resEffects) //true if this player can pay for cost effects
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
          if (res.Num > 0 && (!pl.HasWIC || !CalcCanAffordArchitect(pl))) return false;
          if (res.Num < 0 && (!pl.HasWIC || pl.WIC.NumDeployed < -res.Num)) return false;
        }
        else if (res.Num < 0 && pl.Res.n(res.Name) < -res.Num) return false;
      }
      return true;
    }


    #region gameloop event resolution schlechter code!!!!!
    public async Task EventResolutionTask_complex()
    {
      //Title = "Round " + (ird + 1) + ": Event"; 
      LongMessage = "round " + iround + " event is being resolved..."; iphase = 2; Message = "Event Resolution..."; Debug.Assert(iplayer == 0, "event resolution phase not starting with ipl != 0!");

      var xel = Stats.EventCard.X;
      var events = xel.Elements().ToList();
      var pls = Players.ToList();

      foreach (var ev in events)
      {
        if (ev.Name == "e") await EffectHandler_complex(ev, null, pls, false, false, true);
        else if (ev.Name == "foreach") await EffectHandler_complex(ev, null, pls, false, true, true);
        else if (ev.Name == "yesnoChoice") await EffectHandler_complex(ev, null, pls, true, false, true);
        else if (ev.Name == "yesnoInsideForeach") await EffectHandler_complex(ev, null, pls, true, true, true);
        else if (ev.Name == "task") await HandleTaskEvent_complex(ev);
        else if (ev.Name == "orChoice") await HandleOrChoiceEvent_complex(ev);

        //if (ev.Name == "e") await HandleSimpleEvent(ev);
        //else if (ev.Name == "foreach") await HandleForeachEvent(ev);
        //else if (ev.Name == "yesnoChoice") await HandleYesnoEvent(ev);
        //else if (ev.Name == "task") await HandleTaskEvent(ev);
        //else if (ev.Name == "orChoice") await HandleOrChoiceEvent(ev);

        if (ev != events.Last()) { Message = "next event..."; await WaitForButtonClick(); }
      }

      await FamineTask();

      //LongMessage = "event resolved..."; Console.WriteLine("\t" + LongMessage); await Task.Delay(longDelay);
    }
    public async Task EffectHandler_complex(XElement ev, object[] oarr = null, List<Player> basegroup = null, bool isYesNo = false, bool isCounter = false, bool waitForStartPress = false) // not for or choice events!!! >>in that case choice first
    {
      if (waitForStartPress) { Caption = "start"; LongMessage = "click start to start effect handler task for: " + ev.ToString() + "... start!"; await WaitForButtonClick(); Caption = "Ok"; }

      var plSelected = oarr == null ? CalcPlayersAffectedByPredOnPlayer_complex(ev, basegroup) : CalcPlayersAffectedByPredOnObjects_complex(ev, basegroup, oarr);
      var resEffects = Checker.GetResourcesForRule(ev);
      var effectAction = Checker.GetSpecialEffect_legacy(ev); // spaeter liste

      foreach (var pl in plSelected)
      {
        var cando = CheckResEffects(pl, resEffects) || effectAction != null; if (!cando) continue;

        var num = isCounter ? CalcCounter_complex(ev, pl) : 1; if (num == 0) continue;

        MainPlayer = pl; //switch ui to this player
        await Task.Delay(longDelay);

        if (isYesNo && !isCounter)
        {
          var answer = await YesNoChoiceTask(ev.astring("text"));
          if (!answer) continue;
        }

        for (int i = 0; i < num; i++)
        {
          cando = CheckResEffects(pl, resEffects) || effectAction != null; if (!cando) continue;

          if (isYesNo && isCounter)
          {
            var answer = await YesNoChoiceTask(ev.astring("text"));
            if (!answer) break;
          }

          if (oarr == null) { Message = "oarr is null in EffectHandler!"; }
          effectAction?.Invoke(oarr);
          foreach (var reff in resEffects) await Checker.ApplyResEffectTask(reff);
        }

        await Task.Delay(shortDelay);
        await WaitForAnimationQueueCompleted();

        if (pl != plSelected.Last()) { Message = "next player..."; await WaitForButtonClick(); }
      }
    }


    List<object> ParseParams_complex(XElement ev)
    {
      List<string> strResult = new List<string>();
      List<object> result = new List<object>();
      var par = ev.astring("param");
      if (string.IsNullOrEmpty(par)) return result;

      if (par.Contains("{")) // func="nbought" param="type{battle,war}"
      {
        var attrname = par.StringBefore("{");
        var attrvals = par.StringBetween("{", "}").Split(new char[] { ',' });
        strResult.Add(attrname);
        strResult.AddRange(attrvals);
      }
      else if (par.Contains(",")) // pred="greater" param="stability,2"
      {
        var strings = par.Split(new char[] { ',' });
        strResult.AddRange(strings);
      }
      else // pred="least" param="stability"
      {
        strResult.Add(par);
      }
      foreach (var obj in strResult) // converts strings to integers if possible
      {
        int i = 0;
        if (int.TryParse(obj, out i)) result.Add(i); else result.Add(obj);
      }
      return result;
    }
    async Task HandleOrChoiceEvent_complex(XElement ev, Field field = null)
    {
      LongMessage = "event: " + ev.ToString() + "... start!"; await WaitForButtonClick();

      var plSelected = CalcPlayersAffectedByPredOnPlayer_complex(ev);

      foreach (var pl in plSelected)
      {
        MainPlayer = pl;

        // check wieviele choices moeglich sind, wenn nur 1 oder keines, kein choice
        var choices = ev.Elements().ToList();
        var possibleChoices = new List<XElement>();
        var texts = new List<string>();
        foreach (var choice in choices)
        {
          var resEffects = Checker.GetResourcesForRule(choice);
          var effectAction = Checker.GetSpecialEffect_legacy(choice);
          var resEffectsPossible = CheckResEffects(pl, resEffects);
          if (!resEffectsPossible && effectAction == null) continue;

          possibleChoices.Add(choice); var text = choice.astring("text"); texts.Add(text);
        }

        if (possibleChoices.Count == 0) continue;
        else
        {
          var selectedNum = 0;
          if (possibleChoices.Count > 1)
          {
            var txt = await PickTextChoiceTask(texts, "event");
            for (int i = 0; i < texts.Count; i++) if (txt.Text == texts[i]) selectedNum = i;
          }
          var choice = possibleChoices[selectedNum];
          // just apply effects without choice
          var resEffects = Checker.GetResourcesForRule(choice);
          var effectAction = Checker.GetSpecialEffect_legacy(choice);
          var possible = CheckResEffects(pl, resEffects);
          var resEffectsPossible = CheckResEffects(pl, resEffects);
          effectAction?.Invoke(new object[] { MainPlayer, field });
          if (resEffectsPossible) foreach (var resEffect in resEffects) await Checker.ApplyResEffectTask(resEffect);
        }

        await Task.Delay(shortDelay);
        await WaitForAnimationQueueCompleted();

        if (pl != plSelected.Last()) { Message = "next player..."; await WaitForButtonClick(); }
      }
    }
    async Task HandleTaskEvent_complex(XElement ev, Field field = null)
    {
      // this option is for hardcoded events - these events would make the code just ugly, so they are hardcoded
      LongMessage = "event: " + ev.ToString() + "... start!"; await WaitForButtonClick();
      var plSelected = CalcPlayersAffectedByPredOnPlayer_complex(ev);
      var task = ev.astring("task");
      var resEffects = Checker.GetResourcesForRule(ev);
      var effectAction = Checker.GetSpecialEffect_legacy(ev);

      foreach (var pl in plSelected)
      {
        MainPlayer = pl; //switch ui to this player
        var resEffectsPossible = CheckResEffects(pl, resEffects);
        if (!resEffectsPossible && effectAction == null) continue;

        switch (task)
        {
          case "undeploy_military_each":
            // cond is have workers on military
            var n = pl.Cards.Where(x => x.mil()).Sum(x => x.NumDeployed);
            if (n <= 0) continue;

            var answer = await YesNoChoiceTask(ev.astring("text"));
            if (answer)
            {
              var fields = await CountClickWorkerTask("military");
              int times = 0;
              foreach (var f in fields) { MainPlayer.UndeployFrom(f, f.Counter); times += f.Counter; }

              for (int i = 0; i < times; i++)
              {
                effectAction?.Invoke(new object[] { MainPlayer, field });
                if (resEffectsPossible) foreach (var resEffect in resEffects) await Checker.ApplyResEffectTask(resEffect);
              }
              await Task.Delay(shortDelay);
              await WaitForAnimationQueueCompleted();

            }
            break;
        }


        if (pl != plSelected.Last()) { Message = "next player..."; await WaitForButtonClick(); }
      }
    }

    int CalcCounter_complex(XElement ev, Player pl)
    {
      var func = ev.astring("func");
      var olist = ParseParams_complex(ev);
      olist.Add(pl);
      Debug.Assert(dCountersII.ContainsKey(func), "CalcCounter: dCountersII does not contain key: " + func);
      int result = dCountersII[func](olist.ToArray());
      return result;
    }
    List<Player> CalcPredAnderOnObjects_complex(XElement ev, List<Player> basePlayerSet, object[] oarr)
    {
      // nimm alle <p> elements von ev: example <p pred="most" param="military" />
      var predlist = ev.Elements("p").ToList();
      var result = basePlayerSet;
      foreach (var predev in predlist) result = CalcPlayersAffectedByPredOnObjects_complex(predev, result, oarr);
      return result;
    }
    List<Player> CalcPredOrerOnObjects_complex(XElement ev, List<Player> basePlayerSet, object[] oarr)
    {
      // nimm alle <p> elements von ev: example <p pred="most" param="military" />
      var predlist = ev.Elements("p").ToList();
      var result = new List<Player>();
      foreach (var predev in predlist) result = result.Union(CalcPlayersAffectedByPredOnObjects_complex(predev, basePlayerSet, oarr)).ToList();
      return result;
    }
    List<Player> CalcPlayersAffectedByPredOnObjects_complex(XElement ev, List<Player> basePlayerSet = null, object[] oarr = null)
    {
      // select players affected by event
      var pred = ev.astring("pred");
      if (pred == "and") return CalcPredAnderOnObjects_complex(ev, basePlayerSet, oarr); else if (pred == "or") return CalcPredOrerOnObjects_complex(ev, basePlayerSet, oarr);
      var par = ev.astring("param");
      basePlayerSet = Checker.CalcAffects(ev, basePlayerSet);
      var plSelected = basePlayerSet ?? Players.ToList();//all players is default group
      if (dPredicates_old.ContainsKey(pred))
      {
        var result = new List<Player>();
        var args = new List<object>();
        if (oarr != null)
        {
          foreach (var obj in oarr) args.Add(obj);
        }
        args.Add(par);
        foreach (var pl in basePlayerSet) if (dPredicates_old[pred].Invoke(args.ToArray())) result.Add(pl);
        plSelected = result;
      }
      else if (dPlayerSelectors.ContainsKey(pred))
      {
        var predFunc = dPlayerSelectors[pred];
        plSelected = predFunc(par, plSelected).ToList();
      }
      return plSelected;
    }
    List<Player> CalcPredAnder_complex(XElement ev)
    {
      // nimm alle <p> elements von ev: example <p pred="most" param="military" />
      var predlist = ev.Elements("p").ToList();
      var result = Players.ToList();
      foreach (var predev in predlist) result = CalcPlayersAffectedByPredOnPlayer_complex(predev, result);
      return result;
    }
    List<Player> CalcPredOrer_complex(XElement ev)
    {
      // nimm alle <p> elements von ev: example <p pred="most" param="military" />
      var predlist = ev.Elements("p").ToList();
      var basePlayerSet = Players.ToList();
      var result = new List<Player>();
      foreach (var predev in predlist) result = result.Union(CalcPlayersAffectedByPredOnPlayer_complex(predev)).ToList();
      return result;
    }
    List<Player> CalcPlayersAffectedByPredOnPlayer_complex(XElement ev, List<Player> basePlayerSet = null)
    {
      // select players affected by event
      var pred = ev.astring("pred");
      if (pred == "and") return CalcPredAnder_complex(ev); else if (pred == "or") return CalcPredOrer_complex(ev);

      var par = ev.astring("param");

      basePlayerSet = Checker.CalcAffects(ev, basePlayerSet);

      var plSelected = basePlayerSet ?? Players.ToList();//all players is default group

      if (dPlayerSelectors.ContainsKey(pred))
      {
        var predFunc = dPlayerSelectors[pred];
        plSelected = predFunc(par, plSelected).ToList();
      }
      return plSelected;
    }


    #endregion
  }
}
