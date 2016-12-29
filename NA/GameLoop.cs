using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

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
    void StartOfPlayerActions()
    {
      Title = "Round " + (ird + 1) + ": Player Actions"; LongMessage = "round " + ird + " is starting..."; Message = "click activated objects (cursor = hand) to perform actions"; iph = 1;

      foreach (var pl in Players)
      {
        pl.HasPassed = false;
        pl.Defaulted.Clear();
        pl.UpdateStabAndMil();
      }

      ipl = 0;
      MainPlayer = Players[ipl];
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

    #region gameloop end of round

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
          MainPlayer.Pay(-res.Num, res.Name);
          NetProduction.Add(res);
          //make mods to production?
          //react to defaults
          await Task.Delay(shortDelay);
          await WaitForAnimationQueueCompleted();
        }

        ipl++;

        //falls defaults da sind, muss multichoicepicker aktivieren um abzubezahlen
        Message = "next player...";
        await WaitForButtonClick();
        NetProduction.Clear();
      }
      LongMessage = "production phase ending..."; await Task.Delay(longDelay); LongMessage = "production phase over!"; Console.WriteLine("\t" + LongMessage);
      ShowProduction = false;

      await Task.Delay(longDelay);
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
          //await GrowthPhaseTask(); //comment to skip
          NewEvent();
          StartOfPlayerActions();

          //inject TestScenarios here
          TestScenario1();

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

          await ProductionTask();

          Message = "round end! press ok to continue...";
          await WaitForButtonClick();
          ird++; ipl = 0; iph = 0;
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
