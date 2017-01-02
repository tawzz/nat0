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
  public partial class Game : DependencyObject, INotifyPropertyChanged
  {
    #region  settings for gameloop (eg how many rounds...)    
    int rounds = 4;
    int defaultNumActions = 1;
    int longDelay = 400, shortDelay = 100, minAnimationDuration = 1000; // default wait times 
    int ipl, iph, ird, iact; // gameloop counters

    //unused:
    //int iturnround, moveCounter; bool gov;
    //int[] turnrounds = { 1, 1, 0 }; // not used for now! how many times players take turns, set to 100 when indeterminate (finish by passing)
    //int[] actPerMoveDefault = { 1, 1, 0 }; // how many actions does this player have each turn by default (eg sun tzu: 2 actions in first turn)
    #endregion

    #region gameloop start of round

    async Task RoundAgeProgressTask()
    {
      Title = "Round " + (ird + 1) + ": Growth"; LongMessage = "round " + ird + " is starting..."; iph = 0;

      Stats.UpdateRound();
      Stats.UpdateAge();
      Stats.IsWar = false; Stats.WarLevel = 0;
      await WaitForRoundMarkerAnimationCompleted();

      Progress.Deal();
    }
    async Task GrowthPhaseTask()
    {
      //growth phase --------------------------
      Debug.Assert(IsOkStartEnabled == false, "Start growth phase with button enabled!!!");
      Title = "Round " + (ird + 1); LongMessage = "growth phase is starting...";
      Title += ": Growth";

      Debug.Assert(ipl == 0, "growth phase starting with ipl != 0!");
      while (ipl < NumPlayers)
      {
        MainPlayer = Players[ipl];

        // just 1 growth pick action for now

        var reslist = new List<string> { "wheat", "coal", "gold" };
        if (MainPlayer.ExtraWorkers.Count(x => !x.IsCheckedOut) > 0) reslist.Add("worker");
        var num = LevelGrowth[MainPlayer.Level];

        if (Interrupt) throw (new Exception("STOPPED BY PLAYER!"));

        await WaitForPickResourceCompleted(reslist, num, "growth");

        await WaitForAnimationQueueCompleted(1);

        ipl++;
      }
      LongMessage = "growth phase ending..."; await Task.Delay(longDelay); LongMessage = "growth phase over!"; Console.WriteLine("\t" + LongMessage);
    }
    void NewEvent()
    {
      Stats.PickEventCard();

    }

    #endregion

    #region gameloop player actions

    void BeforePlayerActions()
    {
      Title = "Round " + (ird + 1) + ": Player Actions"; LongMessage = "round " + ird + " is starting..."; Message = "click activated objects (cursor = hand) to perform actions"; iph = 1;

      foreach (var pl in Players)
      {
        pl.HasPassed = false;
        pl.Defaulted.Clear();
        pl.UpdateStabAndMil();
        pl.CardsBoughtThisRound.Clear();
      }

      ipl = 0;
      MainPlayer = Players[ipl];
    }

    async Task PlayerActionTask()
    {

      while (!MainPlayer.HasPassed)
      {
        StartOfPlayerTurn();
        while (iact < MainPlayer.NumActions && !MainPlayer.HasPassed)
        {
          // action starts here *******************************************************
          StartOfAction();
          await ActionLoop();
          await WaitForAnimationQueueCompleted();
          EndOfAction();
          // action ends here *******************************************************
        }
        EndOfPlayerTurn();
      }
      EndOfPlayerActions();
    }

    void StartOfPlayerTurn()
    {
      MainPlayer.NumActions = defaultNumActions;
    }
    void StartOfAction()
    {
      Caption = "Ok";
      AnimationQueue.Clear();
      ContextStack.Clear();
      ContextInit(ctx.start, MainPlayer.Name + ", choose action");
    }
    async Task ActionLoop()
    {
      int testCounter = 0;
      ActionComplete = false;
      while (!ActionComplete)
      {
        UpdateUI();
        await WaitForThreeButtonClick();

        if (CancelClicked) { Message = "Action Canceled!"; ClearSteps(); ActionComplete = false; await Task.Delay(longDelay); }
        else if (PassClicked) { Message = MainPlayer.Name + " Passed"; DisableAndUnselectAll(); await Task.Delay(longDelay); MainPlayer.HasPassed = true; ActionComplete = true; }
        else if (ActionComplete)
        {  //achtung: reihenfolge wichtig bei folgenden clauses
          if (ArchitectSelected) { await TakeArchitectTask(); }
          else if (TurmoilSelected) { await TakeTurmoilTask(); }
          else if (ProgressField != null) { await BuyProgressCardTask(); }
          else if (WorkerSelected && DeployTarget != null) { DeployAvailableWorker(); }
          else if (DeploySource != null && DeployTarget != null) { DeployFromField(); }
        }
        else LongMessage = "action incomplete... please complete your action " + (++testCounter);
      }
    }
    void EndOfAction()
    {
      iact++;
      DisableAndUnselectAll();// UnselectAll();
      PassClicked = OkStartClicked = CancelClicked = false;//clear buttons
    }
    void EndOfPlayerTurn()
    {
      iact = 0;//reset action counter

      //select new player
      var plarr = Players.SkipWhile(x => x != MainPlayer).Skip(1).SkipWhile(x => x.HasPassed).ToArray();
      MainPlayer = plarr.FirstOrDefault() ?? Players.SkipWhile(x => x.HasPassed).FirstOrDefault() ?? Players[0];
    }
    void EndOfPlayerActions() { ipl = 0; }

    #endregion

    #region gameloop production, player order, war resolution

    async Task ProductionTask()
    {
      Title = "Round " + (ird + 1) + ": Production"; LongMessage = "round " + ird + " production is starting..."; iph = 2;
      Message = "Production...";
      Debug.Assert(ipl == 0, "production phase not starting with ipl != 0!");
      NetProduction.Clear();
      ShowProduction = true;
      while (ipl < NumPlayers)
      {
        MainPlayer = Players[ipl];

        await Task.Delay(shortDelay);

        Res[] netProduction = MainPlayer.CalcNetBasicProduction();
        foreach (var res in netProduction)
        {
          await PayTask(MainPlayer, res.Name, -res.Num);
          NetProduction.Add(res);
          //make mods to production?
          //react to defaults
          await Task.Delay(shortDelay);
          await WaitForAnimationQueueCompleted();
          if (MainPlayer.IsBroke) break;
        }

        ipl++;

        //falls defaults da sind, muss multichoicepicker aktivieren um abzubezahlen
        if (ipl < NumPlayers) { Message = "next player..."; await WaitForButtonClick(); }
        NetProduction.Clear();
      }
      ipl = 0;
      LongMessage = "production phase ending..."; await Task.Delay(longDelay); LongMessage = "production phase over!"; Console.WriteLine("\t" + LongMessage);
      ShowProduction = false;

      await Task.Delay(longDelay);
    }

    void PlayerOrder()
    {
      var plarr = Players.OrderBy(x => x.Military).Reverse().ToArray();
      Players.Clear();
      foreach (var pl in plarr) Players.Add(pl);
      for (int i = 0; i < Players.Count; i++) Players[i].Index = i;
      MainPlayer = Players[0];

      //RandomPlayerOrder();
    }

    async Task WarResolutionTask()
    {
      if (!Stats.IsWar) return;

      Title = "Round " + (ird + 1) + ": War"; LongMessage = "round " + ird + " war is being resolved..."; iph = 2;
      Message = "War Resolution...";
      Debug.Assert(ipl == 0, "war resolution phase not starting with ipl != 0!");

      var warcard = Stats.WarCard;
      var xel = warcard.X;
      var reslist = warcard.GetResources().ToList();

      var pls = Players.Where(x => x.Military < Stats.WarLevel).ToArray();

      NetProduction.Clear();
      foreach (var pl in pls)
      {
        MainPlayer = pl;
        Message = MainPlayer.Name + ", you lost in war! click to pay...";
        var resBefore = MainPlayer.GetResourceSnapshot();

        //first show production
        ShowProduction = true;
        NetProduction.Add(new Res("vp", -1));
        foreach (var res in reslist) { var cost = res.Num - MainPlayer.Stability; if (cost > 0) { NetProduction.Add(new Res(res.Name, -cost)); } }
        await WaitForButtonClick();

        NetProduction.Clear(); ShowProduction = false;
        await PayTask(MainPlayer, "vp", 1);
        foreach (var res in reslist)
        {
          var cost = res.Num - MainPlayer.Stability;
          if (cost > 0) { await PayTask(MainPlayer, res.Name, cost); }
          await Task.Delay(shortDelay);
          await WaitForAnimationQueueCompleted();
          var resAfter = MainPlayer.GetResourceSnapshot();
          var loss = resBefore.Zip(resAfter, (x, y) => new Res(x.Name, x.Num - y.Num)).ToList();
          MainPlayer.WarLoss = loss;
          if (MainPlayer.IsBroke) break;
        }

        if (pl != pls.Last()) { Message = "next player..."; await WaitForButtonClick(); }
      }
      Message = "war resolution done...click ok to continue"; await WaitForButtonClick();
      LongMessage = "war resolution ending..."; await Task.Delay(longDelay); LongMessage = "war resolution phase over!"; Console.WriteLine("\t" + LongMessage);
      ipl = 0;
    }



    #endregion

    #region gameloop event resolution
    async Task HandleSimpleEvent(XElement ev)
    {
      LongMessage = "event: " + ev.ToString() + "... start!"; await WaitForButtonClick();

      var plSelected = CalcPlayersAffected(ev);
      var resEffects = GetResourcesForRule(ev);
      var effectAction = GetSpecialEffect(ev);

      foreach (var pl in plSelected)
      {
        MainPlayer = pl; //switch ui to this player
        await Task.Delay(longDelay);

        effectAction?.Invoke(this, MainPlayer);

        foreach (var rtuple in resEffects) await ApplyResEffectTask(rtuple);

        await Task.Delay(shortDelay);
        await WaitForAnimationQueueCompleted();

        if (pl != plSelected.Last()) { Message = "next player..."; await WaitForButtonClick(); }
      }
    }
    async Task HandleForeachEvent(XElement ev)
    {
      LongMessage = "event: " + ev.ToString() + "... start!"; await WaitForButtonClick();

      var plSelected = CalcPlayersAffected(ev);
      var resEffects = GetResourcesForRule(ev);
      var effectAction = GetSpecialEffect(ev);

      foreach (var pl in plSelected)
      {
        MainPlayer = pl; //switch ui to this player
        await Task.Delay(longDelay);

        var num = CalcCounter(ev, pl);
        for (int i = 0; i < num; i++)
        {
          effectAction?.Invoke(this, MainPlayer);
          foreach (var rtuple in resEffects) await ApplyResEffectTask(rtuple);
        }

        await Task.Delay(shortDelay);
        await WaitForAnimationQueueCompleted();

        if (pl != plSelected.Last()) { Message = "next player..."; await WaitForButtonClick(); }
      }
    }
    bool CheckResEffects(Player pl, IEnumerable<Res> resEffects) //true if this player can pay for cost effects
    {
      foreach (var res in resEffects)
      {
        if (res.Num < 0 && pl.Res.n(res.Name) < -res.Num) return false;
      }
      return true;
    }
    async Task HandleYesnoEvent(XElement ev)
    {
      LongMessage = "event: " + ev.ToString() + "... start!"; await WaitForButtonClick();

      var plSelected = CalcPlayersAffected(ev);
      var resEffects = GetResourcesForRule(ev);
      var effectAction = GetSpecialEffect(ev);

      foreach (var pl in plSelected)
      {
        MainPlayer = pl; //switch ui to this player

        var possible = CheckResEffects(pl, resEffects);
        if (!possible) continue;

        var answer = await YesNoChoiceTask(ev.astring("text"));

        if (answer)
        {
          effectAction?.Invoke(this, MainPlayer);

          foreach (var rtuple in resEffects) await ApplyResEffectTask(rtuple);

        }
        await Task.Delay(shortDelay);
        await WaitForAnimationQueueCompleted();

        if (pl != plSelected.Last()) { Message = "next player..."; await WaitForButtonClick(); }
      }
    }
    int CalcCounter(XElement ev, Player pl)
    {
      // select players affected by event
      var func = ev.astring("func");
      var par = ev.astring("param");
      int result = 0;
      if (dCounters.ContainsKey(func))
      {
        var predFunc = dCounters[func];
        result = predFunc(par, pl);
      }
      return result;
    }
    IEnumerable<Player> CalcPlayersAffected(XElement ev)
    {
      // select players affected by event
      var pred = ev.astring("pred");
      var par = ev.astring("param");
      var plSelected = Players.ToArray();//all players is default group
      if (dPred.ContainsKey(pred))
      {
        var predFunc = dPred[pred];
        plSelected = predFunc(par, plSelected).ToArray();
      }
      return plSelected;
    }
    async Task ApplyResEffectTask(Res resEffect)
    {
      var resname = resEffect.Name;
      var resnum = resEffect.Num;
      if (resname == "worker" && resnum < 0) // needs to return a worker!
      {
        var avail = MainPlayer.Res.n("worker") > 0;
        if (!avail) { await PickWorkerToUndeployTask(); }

        Debug.Assert(MainPlayer.Res.n("worker") > 0, "no worker available after undeploy!!!");

        var workerVar = MainPlayer.ExtraWorkers.Where(x => x.IsCheckedOut).Select(x => x.CostRes).Distinct().ToList();
        var num = workerVar.Count;
        var costresname = "";
        if (num > 1) { var worker = await WaitForPickExtraWorkerCompleted(); costresname = worker.CostRes; }
        else if (num == 1) costresname = workerVar.First();

        MainPlayer.ReturnWorker(costresname);
      }
      else if (resname == "worker" && resnum > 0)
      {
        var worker = await WaitForPickExtraWorkerCompleted();
        Message = MainPlayer.Name + " picked " + worker.CostRes.ToCapital() + " worker";
        MainPlayer.CheckOutWorker(worker);
      }
      else
      {
        await PayTask(MainPlayer, resEffect.Name, -resEffect.Num);

      }

    }
    async Task FamineTask(XElement xel)
    {
      var famine = xel.aint("famine");
      if (famine == 0)
      {
        Message = "no famine this time!"; await WaitForButtonClick(); return;
      }
      Message = "famine is " + famine; await WaitForButtonClick();
      ipl = 0;
      while (ipl < NumPlayers)
      {
        MainPlayer = Players[ipl];

        await Task.Delay(shortDelay);
        await PayTask(MainPlayer, "wheat", famine);

        await WaitForAnimationQueueCompleted();

        ipl++;
        if (ipl < NumPlayers) { Message = "next player..."; await WaitForButtonClick(); }
      }
      ipl = 0;

    }

    async Task EventResolutionTask()
    {
      Title = "Round " + (ird + 1) + ": Event"; LongMessage = "round " + ird + " event is being resolved..."; iph = 2; Message = "Event Resolution..."; Debug.Assert(ipl == 0, "event resolution phase not starting with ipl != 0!");

      var xel = Stats.EventCard.X;
      var events = xel.Elements().ToList();
      
      foreach (var ev in events)
      {
        if (ev.Name == "e") await HandleSimpleEvent(ev);
        else if (ev.Name == "foreach") await HandleForeachEvent(ev);
        else if (ev.Name == "yesnoChoice") await HandleYesnoEvent(ev);
        if (ev != events.Last()) { Message = "next event..."; await WaitForButtonClick(); }
      }

      await FamineTask(xel);

      LongMessage = "event resolved..."; Console.WriteLine("\t" + LongMessage); await Task.Delay(longDelay);
    }

    #endregion

    public async void GameLoop()
    {
      try
      {
        IsOkStartEnabled = false; IsRunning = true;
        while (ird < rounds)
        {
          await RoundAgeProgressTask();

          await GrowthPhaseTask(); //comment to skip

          NewEvent();

          BeforePlayerActions(); //updates stab and mil, resets defaults and hasPassed

          //TestScenario9();//inject TestScenarios here!!!!!!!!!!!!!!!!!!!!!

          await PlayerActionTask();

          await ProductionTask();

          PlayerOrder();

          await WarResolutionTask();

          await EventResolutionTask();

          Message = "round end! press ok to continue..."; await WaitForButtonClick(); ird++; ipl = 0; iph = 0;
        }
        LongMessage = Message = "GAME OVER!";
      }
      catch (Exception e)
      {
        LongMessage = "interrupt!!!: " + e.Message;

      }
    }

  }
}
