using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml.Linq;
using ations;
namespace ations
{
  public partial class Game
  {
    public async Task PayOrRemoveChoice(Player pl, string cardtype, string resname, int resnum)
    {
      if (!pl.HasCardOfType(cardtype))
      {
        await PayTask(pl, resname, resnum);
      }
      else
      {
        MainPlayer = pl; await Task.Delay(500);
        var txt = await PickTextChoiceTask(new string[] { "pay " + resnum + " " + resname, "remove " + cardtype }, "event");
        if (txt.Text.StartsWith("pay")) await PayTask(pl, resname, resnum);
        else if (cardtype == "advisor") pl.RemoveAdvisor(); // ist there a case where player would remove just 1 advisor and another where he would remove all advisors?
        else
        {
          var fields = pl.Civ.Fields.Where(x => !x.IsEmpty && x.Card.Type == cardtype).ToList();
          var field = fields.Count > 1 ? await PickCivFieldOutOf(pl, fields, "removing") : fields.FirstOrDefault();
          Checker.RemoveCivCard(pl, field);
        }
      }
    }
    public async Task PayTask(Player pl, string resname, int num) // includes defaultpayment by picking resources
    {
      var debt = pl.Pay(num, resname);
      if (debt > 0)
      {
        // brauch ich einen multiresourcepicker fuer die payment resources von diesem player
        // er soll [debt] mal picken
        var respay = pl.Res.List.Where(x => x.CanPayDefaultWith && x.Num > 0).ToList();
        var canPayDebt = respay.Sum(x => x.Num) >= debt;
        if (canPayDebt)
        {
          var plMain = MainPlayer; MainPlayer = pl;
          var reslist = await PickMultiResourceNumTask(respay, debt, "debt");
          foreach (var rpay in reslist) pl.Pay(rpay.Num, rpay.Name);
          MainPlayer = plMain;
        }
        else
        {
          // this player is eliminated because cannot pay debt!!!
          pl.IsBroke = true;
          Message = "You, " + pl.Name + ", will be terminated for unability to pay your debt!!!!!!!!";
          await WaitForButtonClick();
        }

        //        await DefaultPaymentTask();
      }

    }

    //stats board
    public async Task<Card> HireArchitect(string archtype = "", bool countsAsHire = true)
    {
      var card = MainPlayer.WICField.Card; Debug.Assert(card != null, "TakeArchitect with empty wic!");
      card.NumDeployed++;
      if (card.NumDeployed >= NumArchitects(card))
      {
        await WaitForAnimationQueueCompleted();
        await WonderReadyTask();
      }
      Message = MainPlayer.Name + " hired " + archtype + " architect";
      await Checker.CheckHireArchitect(MainPlayer);
      return card;
    }
    public async Task TakeArchitectTask()
    {
      var card = await HireArchitect();
      var cost = CalcArchitectCost(card); MainPlayer.Pay(cost, "coal");
      Stats.Architects--;
    }
    public async Task AddPrivateArchitectTask()
    {
      var card = await HireArchitect("private");
      var cost = CalcArchitectCost(card); MainPlayer.Pay(cost, "coal");
    }
    public async Task AddFreeArchitectsTask(Player pl, int num)
    {
      var plMain = MainPlayer; MainPlayer = pl;
      await AddFreeArchitectsTask(num);
      MainPlayer = plMain;
    }
    public async Task AddFreeArchitectsTask(int num)
    {
      for (int i = 0; i < num; i++)
      {
        if (!MainPlayer.HasWIC) break;
        await HireArchitect("free", false);
      }
    }
    public async Task WonderReadyTask()
    {
      ContextInit(ctx.wready);
      UpdateUI();
      while (Step == null)
      {
        Message = "pick a wonder space";
        await WaitForButtonClick();
      }
      var civField = Step.Obj as Field;
      MainPlayer.WonderReady(civField);
      await Checker.CheckReady(civField);
      ContextEnd();
    }

    public async Task TakeTurmoilTask()
    {
      var dyn = MainPlayer.Civ.Dynasties.ToList();
      if (dyn.Count == 0) { MainPlayer.UpdateResBy("gold", 2); await Task.Delay(longDelay); return; }

      List<Choice> list = new List<Choice>();
      foreach (var c in dyn) { var ch = new Choice(); ch.Tag = c; ch.Text = "upgrade"; list.Add(ch); }
      list.Add(new Choice { Text = "gold", Path = Helpers.GetImagePath("gold") });

      var chosen = await PickCardChoiceTask(list, "turmoil");

      if (chosen.Tag is Card) { var cdyn = chosen.Tag as Card; MainPlayer.UpgradeDynasty(cdyn); await Checker.CheckUpgradeDynasty(MainPlayer, cdyn); }
      else MainPlayer.UpdateResBy("gold", 2);

      MainPlayer.TurmoilsTaken++;
      Stats.Turmoils--;
      Message = MainPlayer.Name + " took a turmoil"; await Task.Delay(200);
    }
    public async Task<Card> UpgradeDynastyTask(Player pl)
    {
      var dyncard = await IndependentDynastyUpgradeTask(pl);
      await Checker.CheckUpgradeDynasty(pl, dyncard);
      return dyncard;
    }
    public async Task<Card> IndependentDynastyUpgradeTask(Player pl)
    {
      var dyn = pl.Civ.Dynasties.ToList();
      if (dyn.Count == 0) { return null; }
      if (dyn.Count == 1) { pl.UpgradeDynasty(dyn[0]); return dyn[0]; }

      if (MainPlayer != pl) { MainPlayer = pl; await Task.Delay(1000); }

      List<Choice> list = new List<Choice>();
      foreach (var c in dyn) { var ch = new Choice(); ch.Tag = c; ch.Text = "upgrade"; list.Add(ch); }

      var chosen = await PickCardChoiceTask(list, "dynasty change");
      var dyncard = chosen.Tag as Card;
      MainPlayer.UpgradeDynasty(dyncard);
      return dyncard;
    }

    // progress board
    public async Task BuyProgressCardTask()
    {
      var card = ProgressField.Card;
      var fieldBuy = ProgressField;
      var fieldPlace = CivBoardPlace;
      if (card.civ() && fieldPlace == null) { fieldPlace = MainPlayer.GetPossiblePlacesForType(card.Type).FirstOrDefault(); Debug.Assert(fieldPlace != null, "BuyProgressCard: so ein MIST!!!!"); }

      await Checker.CheckBuy(card, fieldPlace);

      Progress.Remove(fieldBuy);
      MainPlayer.CardsBoughtThisRound.Add(card);
      MainPlayer.Pay(card.Price);

      if (card.war()) { Stats.UpdateWarPosition(MainPlayer, card); }
      else if (card.golden()) { await BuyGoldenAgeTask(card); }
      else if (card.battle()) { await PickResourceAndGetItTask(new string[] { "wheat", "coal", "book" }, MainPlayer.RaidValue, "battle"); }
      else { Debug.Assert(card.civ(), "BuyProgressCard: not a civ card:" + card.Name); Checker.AddCivCard(MainPlayer, card, fieldPlace); }

      Message = MainPlayer.Name + " bought " + card.Name;
    }
    public async Task BuyGoldenAgeTask(Card card)
    {
      var res = card.GetResources().FirstOrDefault();
      var effect = card.GetEffect;
      Debug.Assert(res != null || !string.IsNullOrEmpty(card.GetEffect), "BuyGoldenAgeTask: no resource on golden age card AND no effect!");

      var goldenagebonus = MainPlayer.GoldenAgeBonus;
      if (res != null) res.Num += goldenagebonus;

      var costOfVP = Stats.Age - goldenagebonus;
      var resToPayForVP = MainPlayer.Res.List.Where(x => x.CanPayWith && x.Num > 0).ToList();
      var canaffordvp = resToPayForVP.Sum(x => x.Num) >= costOfVP;

      if (canaffordvp)// decide vp or resource/effect
      {
        if (res == null)
        {
          var choice = await PickTextChoiceTask(new string[] { "execute effect", "buy vp" }, "golden age");
          if (choice.Text.StartsWith("buy")) res = new Res("vp", 1);
        }
        else
        {
          var resPicked = await PickResourceTask(new string[] { res.Name, "vp" }, "golden age");
          if (resPicked.Name == "vp") { res = new Res("vp", 1); }
        }
      }

      if (res != null && res.Name == "vp") { await BuyVPForGoldenAgeTask(MainPlayer, costOfVP, resToPayForVP, card); }
      //{
      //  var reslist = await PickMultiResourceNumTask(resToPayForVP, costOfVP, "vp");
      //  foreach (var rpay in reslist) MainPlayer.Pay(rpay.Num, rpay.Name);
      //  MainPlayer.UpdateResBy("vp", 1);
      //  await Checker.CheckBuyGoldenAge(MainPlayer, card, res);
      //}
      else if (res == null || !string.IsNullOrEmpty(effect))      {        await Checker.CheckBuyGoldenAge(MainPlayer, card, res);      }
      else MainPlayer.UpdateResBy(res.Name, res.Num);
    }
    public async Task BuyVPForGoldenAgeTask(Player pl, int costOfVP, List<Res> resToPayForVP, Card card = null)
    {
      var plMain = MainPlayer; MainPlayer = pl;
      var reslist = await PickMultiResourceNumTask(resToPayForVP, costOfVP, "vp");
      foreach (var rpay in reslist) MainPlayer.Pay(rpay.Num, rpay.Name);
      MainPlayer.UpdateResBy("vp", 1);
      await Checker.CheckBuyGoldenAge(MainPlayer, card, new Res("vp"));
      MainPlayer = plMain;
    }
    public async Task PickWonderOrGoldenAgeTask()
    {
      ContextInit(ctx.start);
      DisableAndUnselectAll();
      MarkProgressCardsOfType("wonder");
      MarkProgressCardsOfType("golden");
      while (Step == null)
      {
        Message = "pick wonder or golden age for free";
        await WaitForButtonClick();
      }
      var field = Step.Obj as Field;
      var card = field.Card;
      card.Price = 0;
      await BuyProgressCardTask();
      ContextEnd();
    }
    public async Task SwapTwoProgressCardsTask()
    {
      ContextInit(ctx.swapprogress);
      UpdateUI();
      while (Step == null || Steps.Count < 2)
      {
        Message = "pick two progress cards";
        await WaitForButtonClick();
      }
      // progress cards are swapped
      var field1 = Steps.First().Obj as Field;
      var field2 = Steps.Last().Obj as Field;
      var card1 = field1.Card;
      var card2 = field2.Card;
      var cost = card1.BasicCost;
      card1.BasicCost = card2.BasicCost;
      card2.BasicCost = cost;
      field1.Card = card2;
      field2.Card = card1;
      ContextEnd();
    }
    public async Task<Field> PickProgressCardTask(Player pl)
    {
      ContextInit(ctx.pickProgress);
      UpdateUI();
      while (Step == null)
      {
        //Message = "pick two progress cards";
        await WaitForButtonClick();
      }
      // progress cards are swapped
      var result = Steps.First().Obj as Field;
      ContextEnd();
      return result;
    }

    // options
    public async Task OptionBuyVPTask(Player pl)
    {
      // first check if can afford vp
      var goldenagebonus = pl.GoldenAgeBonus;
      var costOfVP = Stats.Age - goldenagebonus;
      var resToPayForVP = pl.Res.List.Where(x => x.CanPayWith && x.Num > 0).ToList();
      var canaffordvp = resToPayForVP.Sum(x => x.Num) >= costOfVP;

      if (!canaffordvp) return;

      var plMain = MainPlayer; MainPlayer = pl;
      var answer = await YesNoChoiceTask("buy VP?");
      if (answer) { await BuyVPForGoldenAgeTask(pl, costOfVP, resToPayForVP); }

      MainPlayer = plMain;
    }

    // civ board
    public async Task<Field> PickFieldForType(Player pl, string type, string forwhat)
    {
      var fields = pl.GetPossiblePlacesForType(type);
      return await PickCivFieldOutOf(pl, fields, forwhat);
    }
    public async Task<Field> PickCivFieldOutOf(Player pl, IEnumerable<Field> fields, string forwhat)
    {
      if (fields == null || pl == null || !fields.Any()) return null;
      var n = fields.Count();
      if (n == 1) return fields.First();

      if (MainPlayer != pl) { MainPlayer = pl; }
      Debug.Assert(PreselectedFields != null && PreselectedFields.Count == 0, "PickCivFieldOutOf: PreselectedFields not cleared");
      foreach (var f in fields) PreselectedFields.Add(f);

      ContextInit(ctx.pickCivField, "pick civ card for " + forwhat);
      UpdateUI();
      while (Step == null)
      {
        await WaitForButtonClick();
      }
      var result = (Step.Obj as Field); PreselectedFields.Clear();
      await WaitForAnimationQueueCompleted();
      ContextEnd();
      return result;
    }

    // workers
    public async Task DeployWorkerForFreeTask(Player pl, IEnumerable<string> types) // worker tasks kann man noch viel besser streamlinen, in refactoring phase
    {
      var plMain = MainPlayer; MainPlayer = pl;
      if (pl.Res.n("worker") == 0) { await PickWorkerToUndeployTask(); }
      var fields = pl.Civ.Fields.Where(x=>!x.IsEmpty && types.Contains(x.Card.Type)).ToList();
      var field = await PickCivFieldOutOf(pl, fields, "free deployment");
      pl.DeployWorker(1);
      field.Card.NumDeployed++;
      await WaitForAnimationQueueCompleted();
      MainPlayer = plMain;
    }
    public async Task DeployAvailableWorkerTask(Player pl, IEnumerable<string> types)
    {
      if (MainPlayer != pl) { MainPlayer = pl; }
      ContextInit(ctx.deployWorker, "pick civ card to deploy worker to");
      UpdateUI();
      while (Step == null)
      {
        await WaitForButtonClick();
      }
      pl.DeployWorker(1);
      (Step.Obj as Field).Card.NumDeployed++;
      await WaitForAnimationQueueCompleted();
      ContextEnd();
    }
    public async Task DeployAvailableWorkerTask(Player pl)
    {
      if (MainPlayer != pl) { MainPlayer = pl; }
      ContextInit(ctx.deployWorker, "pick civ card to deploy worker to");
      UpdateUI();
      while (Step == null)
      {
        await WaitForButtonClick();
      }
      pl.DeployWorker(1);
      (Step.Obj as Field).Card.NumDeployed++;
      await WaitForAnimationQueueCompleted();
      ContextEnd();
    }
    public async Task<Worker> PickExtraWorkerTask()
    {
      var workers = MainPlayer.ExtraWorkers;
      var wfree1 = workers.FirstOrDefault(x => !x.IsCheckedOut);
      if (wfree1 == null) return null;

      var wfree2 = workers.LastOrDefault(x => !x.IsCheckedOut);
      if (wfree1.CostRes != wfree2.CostRes)
      {
        Debug.Assert(ResChoices != null && ResChoices.Count == 0, "ResChoices not cleared for worker pick");
        Debug.Assert(SelectedResource == null, "start worker pick with resource already selected!");

        var resarr = MainPlayer.ExtraWorkers.Where(x => !x.IsCheckedOut).Select(x => x.CostRes).Distinct().ToArray();

        var res = await PickResourceTask(resarr, "worker type");
        //foreach (var rname in resarr) ResChoices.Add(new Res(rname));
        //ShowResChoices = true; Message = "pick worker type";

        //var res = await MakeSureUserPicksAResource();
        //ShowResChoices = false; ResChoices.Clear(); SelectedResource = null;

        if (res.Name == wfree2.CostRes) wfree1 = wfree2;
      }
      return wfree1;
    }
    public async Task<int> PickUndeployForEachTask(Player pl, IEnumerable<string> cardtype, string resname, int nEach)
    {
      var times = await PickWorkerToUndeployMultipleTask(pl, cardtype);
      if (times == 0) return 0;
      pl.UpdateResBy(resname, nEach * times);
      await WaitForAnimationQueueCompleted();
      return times;
    }
    public async Task<int> PickWorkerToUndeployMultipleTask(Player pl, IEnumerable<string> cardtype)
    {
      var fields = await CountClickWorkerTask(pl, cardtype);
      if (fields.Count == 0) return 0;
      int times = 0;
      foreach (var f in fields) { pl.UndeployFrom(f, f.Counter); times += f.Counter; }
      return times;
    }
    public async Task PickWorkerToUndeployTask()
    {
      var fields = MainPlayer.Civ.Fields.Where(x => !x.IsEmpty && x.Card.buildmil() && x.Card.NumDeployed > 0).ToList();
      //Debug.Assert(fields.Count > 0, "PickWorkerToUndeployTask: no workers deployed!!!");
      var n = fields.Count();
      if (n == 0) return;

      Field field = null;
      if (n == 1) field = fields.First();
      else
      {
        ContextInit(ctx.removeWorker, "pick civ card to undeploy worker from"); UpdateUI();
        while (Step == null) { await WaitForButtonClick(); }
        field = Step.Obj as Field;
        ContextEnd();
      }
      MainPlayer.UndeployFrom(field);
      await WaitForAnimationQueueCompleted();
    }
    public async Task<List<Field>> CountClickWorkerTask(Player pl, IEnumerable<string> cardtype)
    {
      var fields = pl.Civ.Fields.Where(x => !x.IsEmpty && x.Card.NumDeployed > 0 && cardtype.Contains(x.Card.Type)).ToList();
      if (fields.Count == 0) return new List<Field>();
      Caption = "ok";
      Message = "click on cards to undeploy a number of workers";
      var plMain = MainPlayer; MainPlayer = pl;
      foreach (var f in fields) { f.IsCounterEnabled = true; f.CounterText = "undeploy:"; f.Counter = 0; }
      await WaitForButtonClick();
      foreach (var f in fields) { f.IsCounterEnabled = false; f.CounterText = ""; }
      MainPlayer = plMain;
      return fields;
    }
    public async Task<List<Field>> CountClickWorkerTask(string cardtype)
    {
      var fields = MainPlayer.Civ.Fields.Where(x => !x.IsEmpty && x.Card.NumDeployed > 0 && x.Card.Type == cardtype).ToList();
      if (fields.Count == 0) return new List<Field>();
      Caption = "ok";
      Message = "click on cards to undeploy a number of workers";
      foreach (var f in fields) { f.IsCounterEnabled = true; f.CounterText = "undeploy:"; f.Counter = 0; }
      await WaitForButtonClick();
      foreach (var f in fields) { f.IsCounterEnabled = false; f.CounterText = ""; }
      return fields;
    }
    public async Task<Worker> CheckoutExtraWorkerTask(Player pl)
    {
      var plMain = MainPlayer; MainPlayer = pl;
      var worker = await PickExtraWorkerTask();
      if (worker != null)
      {
        Message = MainPlayer.Name + " picked " + worker.CostRes.ToCapital() + " worker";
        MainPlayer.CheckOutWorker(worker);
        Checker.CheckCheckOutExtraWorker(pl, worker);
      }
      MainPlayer = plMain;
      return worker;
    }
    public async Task<string> ReturnWorkerTask(Player pl)
    {
      var plMain = MainPlayer;
      var avail = pl.Res.n("worker") > 0;
      if (!avail) { MainPlayer = pl; await PickWorkerToUndeployTask(); }

      Debug.Assert(pl.Res.n("worker") > 0, "no worker available after undeploy!!!");

      var workerVar = pl.ExtraWorkers.Where(x => x.IsCheckedOut).Select(x => x.CostRes).Distinct().ToList();
      var num = workerVar.Count;
      var costresname = "";
      if (num > 1) { MainPlayer = pl; var worker = await PickExtraWorkerTask(); costresname = worker.CostRes; }
      else if (num == 1) costresname = workerVar.First();

      pl.ReturnWorker(costresname);
      MainPlayer = plMain;
      return costresname;

    }
    public void DeployFromField()
    {
      var sourceCard = DeploySource.Card;
      var targetCard = DeployTarget.Card;
      sourceCard.NumDeployed--;

      DeployTo(targetCard);

      Message = MainPlayer.Name + " deployed from " + sourceCard.Name + " to " + targetCard.Name;
    }
    public void DeployAvailableWorker()
    {
      var card = DeployTarget.Card;
      MainPlayer.DeployWorker();
      DeployTo(card);
      Message = MainPlayer.Name + " deployed to " + card.Name;
    }
    public void DeployTo(Card card)
    {
      card.NumDeployed++;
      var depcost = Checker.CalcDeployCost(MainPlayer, card);
      if (depcost > 0) MainPlayer.Pay(depcost, "coal");
      ////MainPlayer.CalculateSpecialResources();
      ////if (card.mil()) MainPlayer.CalcRaid(); //recheck special cards
    }

    // rounds/messaging/user interaction board
    public async Task<Card> PickACard(Player pl, IEnumerable<string> names, string forWhat) { return await PickACard(pl, names.Select(x => Card.MakeCard(x)).ToList(), forWhat); }
    public async Task<Card> PickACard(Player pl, IEnumerable<Card> cards, string forWhat)
    {
      var choices = cards.Select(x => new Choice() { Tag = x, Text = "" }).ToList();

      if (MainPlayer != pl) { MainPlayer = pl; await Task.Delay(1000); }

      var chosen = await PickCardChoiceTask(choices, forWhat);
      var result = chosen.Tag as Card;
      return result;
    }
    public async Task<Res> PickResourceTask(IEnumerable<string> resnames, string forWhat)
    {
      Debug.Assert(ResChoices != null && ResChoices.Count == 0, "ResChoices not cleared for growth resource pick");
      Debug.Assert(SelectedResource == null, "start resource pick with resource already selected!");

      foreach (var rname in resnames) ResChoices.Add(new Res(rname));
      ShowResChoices = true; var capt = Caption; Caption = "Pick"; Message = MainPlayer.Name + ", pick " + forWhat + " resource";

      var res = await MakeSureUserPicksAResource();

      ShowResChoices = false; ResChoices.Clear(); SelectedResource = null; Message = MainPlayer.Name + " picked " + res.Name.ToCapital(); Caption = capt;
      return res;
    }
    public async Task<Res> PickResourceAndGetItTask(IEnumerable<string> resnames, int num, string forWhat)
    {
      var res = await PickResourceTask(resnames, forWhat);
      //Debug.Assert(ResChoices != null && ResChoices.Count == 0, "ResChoices not cleared for growth resource pick");
      //Debug.Assert(SelectedResource == null, "start resource pick with resource already selected!");

      //foreach (var rname in resnames) ResChoices.Add(new Res(rname));
      //Number = num;
      //ShowResChoices = true; var capt = Caption; Caption = "Pick"; Message = MainPlayer.Name + ", pick " + forWhat + " resource";

      //var res = await MakeSureUserPicksAResource();

      //ShowResChoices = false; ResChoices.Clear(); SelectedResource = null;
      //Message = MainPlayer.Name + " picked " + res.Name.ToCapital();

      if (res.Name == "vp") Number = 1; else Number = num;

      if (res.Name == "worker")
      {
        var worker = await CheckoutExtraWorkerTask(MainPlayer);
        //var worker = await PickExtraWorkerTask();
        //Message = MainPlayer.Name + " picked " + worker.CostRes.ToCapital() + " worker";
        //MainPlayer.CheckOutWorker(worker);//weg
      }
      else
      {
        MainPlayer.UpdateResBy(res.Name, Number);
      }
      return res;
    }
    public async Task<List<Res>> PickMultiResourceNumTask(IEnumerable<Res> reslist, int total, string forWhat)
    {//returns list of resources with number selected by player
      //reslist is copied so can pass player resource list as param
      Debug.Assert(ResChoices != null && ResChoices.Count == 0, "ResChoices not cleared for growth resource pick");
      //Debug.Assert(SelectedResource == null, "start resource pick with resource already selected!");

      // can pay at most MainPlayer.Res.n(resname) of each resource
      ResChoices.Clear();
      foreach (var res in reslist) ResChoices.Add(new Res(res.Name, 0));
      ShowMultiResChoices = true; var capt = Caption; Caption = "Pick"; Message = MainPlayer.Name + ", pick " + total + " resources for " + forWhat;

      await MakeSureUserPicksExactlyTotalOfResources(total);

      ShowMultiResChoices = false;
      var result = ResChoices.ToList(); ResChoices.Clear();
      Message = MainPlayer.Name + " picked " + total + " resource"; Caption = capt;

      return result;
    }
    public async Task<List<Res>> PickMultiResourceWithinRangeTask(IEnumerable<Res> reslist, int min, int max, string forWhat)
    {//returns list of resources with number selected by player
      //reslist is copied so can pass player resource list as param
      Debug.Assert(ResChoices != null && ResChoices.Count == 0, "ResChoices not cleared for growth resource pick");
      //Debug.Assert(SelectedResource == null, "start resource pick with resource already selected!");

      // can pay at most MainPlayer.Res.n(resname) of each resource
      ResChoices.Clear();
      foreach (var res in reslist) ResChoices.Add(new Res(res.Name, 0));
      ShowMultiResChoices = true; var capt = Caption; Caption = "Pick"; Message = MainPlayer.Name + ", pick between " + min + " and " + max + " resources for " + forWhat;

      await MakeSureUserPicksBetween(min,max);

      ShowMultiResChoices = false;
      var result = ResChoices.ToList(); ResChoices.Clear();
      Message = MainPlayer.Name + " picked " + result.Sum(x=>x.Num) + " resources"; Caption = capt;

      return result;
    }
    public async Task<Choice> PickTextChoiceTask(IEnumerable<string> choiceTexts, string forWhat)
    {
      Debug.Assert(Choices != null && Choices.Count == 0, "Choices null or not cleared for choice picker");
      Debug.Assert(SelectedChoice == null, "start choice picker with choice already selected!");

      foreach (var text in choiceTexts) Choices.Add(new Choice { Text = text });
      ShowChoices = true; var capt = Caption; Caption = "Pick"; Message = MainPlayer.Name + ", pick " + forWhat + " choice";

      var choice = await MakeSureUserPicksAChoice();

      ShowChoices = false; Choices.Clear(); SelectedChoice = null;
      Message = MainPlayer.Name + " picked " + choice.Text.ToCapital(); Caption = capt;

      return choice;
    }
    public async Task<Choice> PickCardChoiceTask(List<Choice> choices, string forWhat)
    {
      Debug.Assert(Choices != null && Choices.Count == 0, "Choices null or not cleared for choice picker");
      Debug.Assert(SelectedChoice == null, "start choice picker with choice already selected!");

      //Choices.Clear();
      foreach (var ch in choices) Choices.Add(ch);
      ShowCardChoices = true; var capt = Caption; Caption = "Pick"; Message = MainPlayer.Name + ", pick " + forWhat + " choice";

      var choice = await MakeSureUserPicksAChoice();

      ShowCardChoices = false; Choices.Clear(); SelectedChoice = null;
      Message = MainPlayer.Name + " picked " + choice.Text.ToCapital(); Caption = capt;

      return choice;
    }
    public async Task<bool> YesNoChoiceTask(string msg)
    {
      var okCaption = Caption; var passCaption = RightCaption; Caption = "Yes"; RightCaption = "No"; var prevMsg = Message; Message = msg;
      IsOkStartEnabled = IsPassEnabled = true; OkStartClicked = PassClicked = false;

      while (!OkStartClicked && !PassClicked)
      {
        if (Interrupt) throw (new Exception("STOPPED BY PLAYER!"));
        if (IsTesting) { await Task.Delay(longDelay); ConsumeTestInput(); await Task.Delay(longDelay); }
        await Task.Delay(100);
      }
      var result = OkStartClicked;
      IsOkStartEnabled = IsPassEnabled = false; Caption = okCaption; RightCaption = passCaption; Message = prevMsg;
      OkStartClicked = PassClicked = false;
      return result;
    }

    public async Task WaitForButtonClick()
    {
      IsOkStartEnabled = true; OkStartClicked = false;

      while (!OkStartClicked)
      {
        if (Interrupt) throw (new Exception("STOPPED BY PLAYER!"));

        if (IsTesting) { await Task.Delay(longDelay); ConsumeTestInput(); await Task.Delay(longDelay); }

        await Task.Delay(100);
      }
      OkStartClicked = false; IsOkStartEnabled = false;
    }
    public async Task WaitForThreeButtonClick()
    {
      IsOkStartEnabled = IsCancelEnabled = IsPassEnabled = true;
      OkStartClicked = CancelClicked = PassClicked = false;
      while (!OkStartClicked && !CancelClicked && !PassClicked)
      {
        if (Interrupt) throw (new Exception("STOPPED BY PLAYER!"));

        if (IsTesting) { await Task.Delay(longDelay); ConsumeTestInput(); await Task.Delay(longDelay); }//***

        await Task.Delay(100);
      }
      IsOkStartEnabled = IsCancelEnabled = IsPassEnabled = false;
    }

    // helpers
    public async Task<Choice> MakeSureUserPicksAChoice()
    {
      while (SelectedChoice == null)
      {
        if (Interrupt) throw (new Exception("STOPPED BY PLAYER!"));
        await Task.Delay(shortDelay);
        await WaitForButtonClick();

        var choice = SelectedChoice;
        if (choice == null) { Message = "YOU DID NOT PICK ANYTHING!"; }
      }
      return SelectedChoice;
    }
    public async Task<Res> MakeSureUserPicksAResource()
    {
      while (SelectedResource == null)
      {
        await Task.Delay(shortDelay);
        await WaitForButtonClick();

        var res = SelectedResource;
        if (res == null) { Message = "YOU DID NOT PICK ANYTHING!"; }
      }
      return SelectedResource;
    }
    public async Task MakeSureUserPicksExactlyTotalOfResources(int total)
    {
      int picked = 0;
      while (picked != total)
      {
        await Task.Delay(shortDelay);
        await WaitForButtonClick();

        // how do I bind to a list of resources two way?!?!?!?!?!?!?!?!?!!!!!!!!
        var reslist = ResChoices;
        picked = reslist.Sum(x => x.Num);
        if (picked != total) { Message = "PICK THE AMOUNT SPECIFIED: " + total + "!"; foreach (var r in ResChoices) r.Num = 0; }
      }
    }
    public async Task MakeSureUserPicksBetween(int min, int max)
    {
      int picked = 0;
      while (picked < min || picked > max)
      {
        await Task.Delay(shortDelay);
        await WaitForButtonClick();

        // how do I bind to a list of resources two way?!?!?!?!?!?!?!?!?!!!!!!!!
        var reslist = ResChoices;
        picked = reslist.Sum(x => x.Num);
        if (picked <min || picked > max) { Message = "pick between " + min + " and " + max + " resources!"; foreach (var r in ResChoices) r.Num = 0; }
      }
    }
    public async Task WaitForAnimationQueueCompleted(int minAnimations = 0)
    {
      while (AnimationQueue.Count < minAnimations)
      {
        await Task.Delay(200);//give ui time to trigger resourceUpdated event
        if (Interrupt) throw (new Exception("STOPPED BY PLAYER!"));
      }
      //Console.WriteLine("Animation Queue ready -  starting animations...");
      if (AnimationQueue.Count == 0) { await Task.Delay(500); }
      while (AnimationQueue.Count > 0)
      {
        var sb = AnimationQueue[0];
        AnimationQueue.RemoveAt(0);
        sb.Begin();
        if (Interrupt) throw (new Exception("STOPPED BY PLAYER!"));
        await Task.Delay(minAnimationDuration);
        while (sb.GetCurrentState() == ClockState.Active && sb.GetCurrentTime() < sb.Duration)
        {
          if (Interrupt) throw (new Exception("STOPPED BY PLAYER!"));

          await Task.Delay(100);
        }
      }
      Debug.Assert(AnimationQueue.Count == 0, "WaitForAnimationQueueCompleted: nicht alle animations geloescht!");
      //Console.WriteLine("Animation Queue abgearbeitet!");
    }
    public async Task WaitForRoundMarkerAnimationCompleted()
    {
      var sb = Storyboards.MoveTo(UIRoundmarker, Stats.RoundMarkerPosition, TimeSpan.FromSeconds(1), null);
      //var sb = Storyboards.Scale(testui, TimeSpan.FromSeconds(.3), new Point(1, 1), new Point(5, 2), null, true);
      //sb.Completed += (s, _) => testcompleted(sb, testui); // brauche garkein completed in wirklichkeit! nur testing!
      sb.Begin();
      while (sb.GetCurrentState() == ClockState.Active && sb.GetCurrentTime() < sb.Duration)
      {
        if (Interrupt) throw (new Exception("STOPPED BY PLAYER!"));
        await Task.Delay(200);
      }
    }
    public async Task WaitSeconds(double secs) { int delay = (int)(secs * 1000); await Task.Delay(delay); }
  }
}
