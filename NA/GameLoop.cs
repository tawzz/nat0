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
    int rounds = 1;
    int[] turnrounds = { 1, 1, 0 }; // not used for now! how many times players take turns, set to 100 when indeterminate (finish by passing)
    int[] actPerMoveDefault = { 1, 3, 0 }; // how many actions does this player have each turn (eg sun tzu: 2 actions in first turn)
    int longDelay = 500, shortDelay = 100, minAnimationDuration = 1000; // default wait times 
    int ipl, iph, ird, iturnround, iact, moveCounter; bool gov; // gameloop counters
    #endregion

    #region gameloop parts
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
    async Task RoundAgeProgressTask()
    {
      Debug.Assert(iph == 0, "iph != 0 at beginning of round!!!");

      iph = 0; Title = "Round " + (ird + 1) + ", Phase " + iph + ": Growth"; LongMessage = "round " + ird + " is starting...";

      Stats.UpdateRound();
      Stats.UpdateAge();
      await WaitForRoundMarkerAnimationCompleted();

      Progress.Deal();
    }
    void EventAndBeforePlayerActions()
    {
      Stats.PickEventCard();

      //player action phase --------------------------
      iph = 1; Title = "Round " + (ird + 1) + ", Phase " + iph + ": Player Actions"; LongMessage = "round " + ird + " is starting..."; Message = "click activated objects (cursor = hand) to perform actions";
      iturnround = 0;
      foreach (var pl in Players) pl.HasPassed = false;
      ipl = 0;
      MainPlayer = Players[ipl];
    }
    void StartOfPlayer()
    {
      MainPlayer.NumActions = actPerMoveDefault[iph]; //testing //ignore sun tzu
    }
    void StartOfAction()
    {
      Caption = "Ok";
      Message = MainPlayer.Name + ", choose action";
      AnimationQueue.Clear();
      SelectedAction = null;
      MarkAllPlayerChoices();
    }
    #endregion

    public async void GameLoop()
    {
      try
      {
        IsOkStartEnabled = false; IsRunning = true; gov = false;
        while (ird < rounds)
        {
          await RoundAgeProgressTask(); // do not comment  

          //await GrowthPhaseComplete(); //comment to go directly to action phase

          EventAndBeforePlayerActions(); // do not comment

          while (MainPlayer != null)
          {
            StartOfPlayer();

            while (iact < MainPlayer.NumActions && !MainPlayer.HasPassed)
            {
              StartOfAction();

              await WaitFor3ButtonClick();

              if (PassClicked) { MainPlayer.HasPassed = true; UnselectAll(); } // next player turn
              else if (CancelClicked) { UnselectAll(); await Task.Delay(longDelay); }

              //************ buy progress card action ****************************************
              else if (SelectedAction == BuyProgressCard)
              {
                await BuyProgressCardTask();
                await WaitForAnimationQueueCompleted();
                iact++;
                UnselectAll();
              }
              else if (SelectedAction == PartialBuy) // progresscard has been selected, need select civ place
              {
                while (!CancelClicked && !PassClicked && SelectedCivField == null)
                {
                  //                  await Task.Delay(100);
                  Message = "select place for card (PartialBuy)";
                  await WaitFor3ButtonClick();
                }
                if (CancelClicked) UnselectAll();
                else if (PassClicked) { MainPlayer.HasPassed = true; UnselectAll(); }
                else if (SelectedAction == BuyProgressCard)
                {
                  await BuyProgressCardTask();
                  await WaitForAnimationQueueCompleted();
                  iact++;
                  UnselectAll();
                }
                else if (SelectedAction != null)
                {
                  SelectedAction();
                  await WaitForAnimationQueueCompleted();
                  iact++;
                  UnselectAll(); // action completed
                }
              }
              //************ end buy progress card action ****************************************

              else if (SelectedAction == PartialDeploy) // partial information, need more selections
              {
                while (!CancelClicked && !PassClicked && !WorkerSelected && PreviousSelectedCivField == null)
                {
                  await Task.Delay(200);
                  await WaitFor3ButtonClick();
                }
                if (CancelClicked) UnselectAll();
                else if (PassClicked) { MainPlayer.HasPassed = true; UnselectAll(); }
                else if (SelectedAction != null)
                {
                  SelectedAction();
                  await WaitForAnimationQueueCompleted();
                  iact++;
                  UnselectAll(); // action completed
                }
              }
              else if (SelectedAction != null)
              {
                SelectedAction();
                //await RunSelectedAction(); // SelectedAction();
                await WaitForAnimationQueueCompleted();
                iact++;
                UnselectAll(); // action completed
              }


              //clear buttons
              PassClicked = OkStartClicked = CancelClicked = false;


              //end action
              UnselectAll();
            }
            //end player action
            iact = 0;
            ipl = (ipl + 1) % NumPlayers;
            MainPlayer = Players.First(x => !x.HasPassed && x.Index >= ipl);
          }

          await WaitForButtonClick();

          //await DoStuffAtEndOfRound();
          ird++;
        }
        LongMessage = "game is over";
      }
      catch (Exception e)
      {
        LongMessage = "interrupt!!!: " + e.Message;
      }
    }


    //public void BuyProgressCard()
    //{
    //  var card = SelectedProgressField.Card;
    //  var fieldBuy = SelectedProgressField;
    //  var fieldPlace = SelectedCivField;
    //  card.CanBuy = false;

    //  if (card.civ())
    //  {
    //    if (fieldPlace == null) fieldPlace = GetPossiblePlacesForCard(fieldBuy).First();
    //    MainPlayer.Civ.Add(card, fieldPlace);
    //    Progress.Remove(fieldBuy);
    //    MainPlayer.Pay(card.Cost);
    //  }
    //  else
    //  {
    //    Progress.Remove(fieldBuy);
    //    MainPlayer.Pay(card.Cost);
    //    if (card.war()) { Stats.UpdateWarPosition(MainPlayer, card); }
    //    else if (card.golden()) BuyGoldenAge(card);
    //    else if (card.battle())
    //    {
    //      //LongMessage = "TODO!!!! resource picker invokation";
    //      WaitForPickResourceCompletedSync(new string[] { "wheat", "coal", "book" }, MainPlayer.RaidValue, "battle");

    //      //AAnimations.AfterAnimation = null; // (x) => NextPlayer(); // ani in ui ausgeloest onResourceUpdate
    //      //PrepareResourcePicker(new string[] { "wheat", "coal", "book" }, MainPlayer.RaidValue, "battle");
    //    }
    //  }
    //  Message = MainPlayer.Name + " bought " + card.Name;

    //}


  }
}
