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
    //public async void GameLoop_old()
    //{
    //  try
    //  {
    //    IsOkStartEnabled = false; IsRunning = true;
    //    while (iround < rounds)
    //    {
    //      Title = IsTesting ? CurrentKey : "Round " + (iround + 1); iphase = 0;

    //      if (SwitchRoundAndAgeOn) { Stats.UpdateRound(); Stats.UpdateAge(); await WaitForRoundMarkerAnimationCompleted(); }

    //      if (SwitchProgressOn) Progress.Deal();

    //      if (SwitchGrowthOn) await GrowthPhaseTask();

    //      if (SwitchNewEventOn) Stats.PickEventCard();

    //      BeforePlayerActions(); // updates stab and mil, resets defaults, hasPassed, ...

    //      if (IsTesting) CurrentTest.Scenario(); // inject TestScenarios here!!!!!!!!!!!!!!!!!!!!!

    //      if (SwitchActionOn) await PlayerActionTask();

    //      if (SwitchProductionOn) await ProductionTask();

    //      if (SwitchOrderOn) PlayerOrder();

    //      if (SwitchWarOn) await WarResolutionTask();

    //      if (SwitchEventOn) await EventResolutionTask();

    //      if (IsTesting) { TestEnd(); await WaitForButtonClick(); }

    //      Message = "press ok to continue..."; await WaitForButtonClick(); iround++; iplayer = 0; iphase = 0; Stats.UpdateWarPosition(); //cleanup
    //    }
    //    LongMessage = Message = "GAME OVER!";
    //  }
    //  catch (Exception e)
    //  {
    //    CanInitialize = true;
    //    LongMessage = "interrupt!!!: " + e.Message;

    //  }
    //}

    #region growth


    #endregion

    #region player actions



    #endregion

    #region gameloop production, player order, war resolution


    //void PlayerOrder()
    //{
    //  var plarr = Players.OrderBy(x => x.Military).Reverse().ToArray();
    //  Players.Clear();
    //  foreach (var pl in plarr) Players.Add(pl);
    //  for (int i = 0; i < Players.Count; i++) Players[i].Index = i;
    //  MainPlayer = Players[0];

    //  //RandomPlayerOrder();
    //}

    //async Task WarResolutionTask()
    //{
    //  if (!Stats.IsWar) return;

    //  //Title = "Round " + (ird + 1) + ": War";
    //  LongMessage = "round " + iround + " war is being resolved..."; iphase = 2; Message = "War Resolution...";
    //  Debug.Assert(iplayer == 0, "war resolution phase not starting with ipl != 0!");

    //  var warcard = Stats.WarCard;
    //  var xel = warcard.X;
    //  var reslist = warcard.GetResources().ToList();

    //  var pls = Players.Where(x => x.Military < Stats.WarLevel).ToArray();

    //  ChangesInResources.Clear();
    //  foreach (var pl in pls)
    //  {
    //    MainPlayer = pl;
    //    Message = MainPlayer.Name + ", you lost in war! click to pay...";
    //    var resBefore = MainPlayer.GetResourceSnapshot();

    //    //first show production
    //    ShowChangesInResources = true;
    //    ChangesInResources.Add(new Res("vp", -1));
    //    foreach (var res in reslist) { var cost = res.Num - MainPlayer.Stability; if (cost > 0) { ChangesInResources.Add(new Res(res.Name, -cost)); } }
    //    await WaitForButtonClick();

    //    ChangesInResources.Clear(); ShowChangesInResources = false;
    //    await PayTask(MainPlayer, "vp", 1);
    //    foreach (var res in reslist)
    //    {
    //      var cost = res.Num - MainPlayer.Stability;
    //      if (cost > 0) { await PayTask(MainPlayer, res.Name, cost); }
    //      await Task.Delay(shortDelay);
    //      await WaitForAnimationQueueCompleted();
    //      var resAfter = MainPlayer.GetResourceSnapshot();
    //      var resDiff = MainPlayer.GetResDiff(resBefore, resAfter, Res.WarResources);
    //      //var loss = resBefore.Zip(resAfter, (x, y) => new Res(x.Name, x.Num - y.Num)).ToList();
    //      MainPlayer.WarLoss = resDiff.Keys.Select(x => new Res(x, resDiff[x])).Where(x => x.Num != 0).ToList();
    //      if (MainPlayer.IsBroke) break;
    //    }

    //    if (pl != pls.Last()) { Message = "next player..."; await WaitForButtonClick(); }
    //  }
    //  Message = "war resolution done...click ok to continue"; await WaitForButtonClick();
    //  LongMessage = "war resolution ending..."; await Task.Delay(longDelay); LongMessage = "war resolution phase over!";// Console.WriteLine("\t" + LongMessage);
    //  iplayer = 0;
    //}



    #endregion

    #region checks
    async Task Checks_Prod()
    {
      foreach (var field in MainPlayer.Civ.Fields)
      {
        if (!field.IsEmpty)
        {
          var checks = field.Card.X.Elements("prod").ToArray();
          foreach (var chk in checks)
          {
            await EffectHandler(chk, new object[] { field }, new List<Player> { MainPlayer }, false, false, true);
          }
        }
      }
    }
    #endregion


  }
}
