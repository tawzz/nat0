using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ations
{
  public partial class Game : DependencyObject, INotifyPropertyChanged
  {
    #region  settings for control flow: change to determine game ablauf(eg how many rounds...)    
    int rounds = 8;
    int defaultNumActions = 1;
    int longDelay = 500, shortDelay = 100, minAnimationDuration = 1000; // default wait times 
    int ipl, iph, ird, iact; // gameloop counters

    //unused:
    //int iturnround, moveCounter; bool gov;
    //int[] turnrounds = { 1, 1, 0 }; // not used for now! how many times players take turns, set to 100 when indeterminate (finish by passing)
    //int[] actPerMoveDefault = { 1, 1, 0 }; // how many actions does this player have each turn by default (eg sun tzu: 2 actions in first turn)
    #endregion

    #region gameloop parts

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
    void StartOfPlayerActions()
    {
      Title = "Round " + (ird + 1) + ": Player Actions"; LongMessage = "round " + ird + " is starting..."; Message = "click activated objects (cursor = hand) to perform actions"; iph = 1;

      foreach (var pl in Players)
      {
        pl.HasPassed = false;
        pl.Defaulted.Clear();
        pl.CalcMilitary();
        pl.CalcStability();
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
      Message = MainPlayer.Name + ", choose action";
      AnimationQueue.Clear();
      //SelectedAction = null;
      MarkAllPlayerChoices();
    }
    async Task<bool> ProcessActionTask() //hier kommen action specific checks rein oder in die Tasks, vielleicht noch besser
    {
      if (CancelClicked) { UnselectAll(); await Task.Delay(longDelay); return false; }
      else if (PassClicked) { MainPlayer.HasPassed = true; }
      else if (ArchitectSelected) { await TakeArchitectTask(); }
      else if (TurmoilSelected) { await TakeTurmoilTask(); }
      else if (SelectedProgressField != null && SelectedCivField != null) { await BuyProgressCardTask(); }
      else if (SelectedProgressField != null && IsOneStepBuy(SelectedProgressField)) { await BuyProgressCardTask(); }
      else if (WorkerSelected && SelectedCivField != null) { DeployAvailableWorker(); }
      else if (SelectedCivField != null && PreviousSelectedCivField != null) { DeployFromField(); }
      else return false;

      return true;
    }
    void EndOfAction()
    {
      iact++;
      UnselectAll();
      PassClicked = OkStartClicked = CancelClicked = false;//clear buttons
    }
    void EndOfPlayerTurn()
    {
      iact = 0;//reset action counter

      //select new player
      var plarr = Players.SkipWhile(x => x != MainPlayer).Skip(1).SkipWhile(x => x.HasPassed).ToArray();
      MainPlayer = plarr.FirstOrDefault() ?? Players.SkipWhile(x => x.HasPassed).FirstOrDefault() ?? Players[0];
    }
    void EndOfPlayerActions() { }
    async Task ProductionTask()
    {
      Title = "Round " + (ird + 1) + ": Production"; LongMessage = "round " + ird + " production is starting..."; iph = 2;
      Message = "Production...";
      await Task.Delay(longDelay);
    }

    #endregion

    public async void GameLoop()
    {
      try
      {
        IsOkStartEnabled = false; IsRunning = true; //gov = false; use later
        while (ird < rounds)
        {
          await RoundAgeProgressTask(); // do not comment  
          //await GrowthPhaseTask(); //comment to go directly to action phase
          NewEvent(); // do not comment

          StartOfPlayerActions();// do not comment
          while (!MainPlayer.HasPassed)
          {
            StartOfPlayerTurn();

            while (iact < MainPlayer.NumActions && !MainPlayer.HasPassed)
            {
              StartOfAction();

              var actionComplete = false;
              while (!actionComplete)
              {
                await WaitFor3ButtonClick();
                actionComplete = await ProcessActionTask();
              }

              await WaitForAnimationQueueCompleted();
              EndOfAction();
            }

            EndOfPlayerTurn();
          }
          EndOfPlayerActions();

          await ProductionTask();

          Message = "END OF ROUND " + (ird+1) +" - press ok to continue...";
          await WaitForButtonClick();
          ird++;
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
