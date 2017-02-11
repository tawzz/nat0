using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ations.NA
{
  class _trash
  {
    #region Gameloop

    //async Task Checks_Ready(Field civField, Card card)
    //{
    //  foreach (var field in MainPlayer.Civ.Fields)
    //  {
    //    if (!field.IsEmpty)
    //    {
    //      var checks = field.Card.X.Elements("new_wonder_ready").ToArray();
    //      foreach (var chk in checks)
    //      {
    //        await EffectHandler(chk, new object[] { card, civField, field }, new List<Player> { MainPlayer }, false, false, true);
    //      }
    //    }
    //  }
    //  var checksNewWonder = card.X.Elements("ready").ToArray();
    //  foreach (var chk in checksNewWonder)
    //  {
    //    await EffectHandler(chk, new object[] { card, civField, civField }, new List<Player> { MainPlayer }, false, false, true);
    //  }

    //}
    //async Task Checks_Pre_Action()
    //{
    //  foreach (var field in MainPlayer.Civ.Fields)
    //  {
    //    if (!field.IsEmpty)
    //    {
    //      var checks = field.Card.X.Elements("post_action").ToArray();
    //      foreach (var chk in checks)
    //      {
    //        // kann auch preconds on civField, Card haben eg., buy battle...
    //        // koennte aber sehr wohl foreach oder yesno sein, ignore for now!
    //        await EffectHandler(chk, new object[] { field }, new List<Player> { MainPlayer }, false, false, true);
    //      }
    //    }
    //  }
    //}
    //async Task Checks_Post_Action()
    //{



    //  foreach (var field in MainPlayer.Civ.Fields)
    //  {
    //    if (!field.IsEmpty)
    //    {
    //      var checks = field.Card.X.Elements("post_action").ToArray();
    //      foreach (var chk in checks)
    //      {
    //        // kann auch preconds on civField, Card haben eg., buy battle...
    //        // koennte aber sehr wohl foreach oder yesno sein, ignore for now!
    //        await EffectHandler(chk, new object[] { field }, new List<Player> { MainPlayer }, false, false, true);
    //      }
    //    }
    //  }
    //}

    //async Task Checks_Pass() { }
    //async Task Checks_Pre_Buy(Field progField, Card card, Field civField)
    //{
    //  // called after buying card from progField and placing it on civField
    //  // also check if the new card has some effect when_being_bought
    //  foreach (var field in MainPlayer.Civ.Fields)
    //  {
    //    if (!field.IsEmpty)
    //    {
    //      var checks = field.Card.X.Elements("buy").ToArray();
    //      foreach (var chk in checks)
    //      {
    //        await EffectHandler(chk, new object[] { progField, card, civField, field }, new List<Player> { MainPlayer }, false, false, true);
    //      }
    //    }
    //  }
    //}
    //async Task Checks_Buy(Field progField, Card card, Field civField)
    //{
    //  // called after buying card from progField and placing it on civField
    //  // also check if the new card has some effect when_being_bought
    //  foreach (var field in MainPlayer.Civ.Fields)
    //  {
    //    if (!field.IsEmpty)
    //    {
    //      var checks = field.Card.X.Elements("buy").ToArray();
    //      foreach (var chk in checks)
    //      {
    //        await EffectHandler(chk, new object[] { progField, card, civField, field }, new List<Player> { MainPlayer }, false, false, true);
    //      }
    //    }
    //  }
    //}


    //async Task Check_First_Turn()
    //{
    //  // also hav to check the cards of others whether they affect first_turn w/ affects="others"
    //  foreach (var field in MainPlayer.Civ.Fields)// check own cards if they affect my first turn
    //  {
    //    if (!field.IsEmpty)
    //    {
    //      var checks = field.Card.X.Elements("first_turn").ToArray();
    //      foreach (var chk in checks)
    //      {
    //        // koennte aber sehr wohl foreach oder yesno sein, ignore for now!
    //        await EffectHandler(chk, new object[] { field }, new List<Player> { MainPlayer }, false, false, true);
    //      }
    //    }
    //  }
    //}
    //async Task<int> Check_Natural_Wonder_Ready(int defaultNA)
    //{
    //  int result;
    //  if (MainPlayer.RoundsToWaitForNaturalWonder > 0)
    //  {
    //    MainPlayer.RoundsToWaitForNaturalWonder--;
    //    result = 0;
    //    if (MainPlayer.RoundsToWaitForNaturalWonder == 0)
    //    {
    //      await WonderReadyTask();
    //    }
    //  }
    //  else result = defaultNA;
    //  return result;
    //}

    //List<Player> PlayersByPredicate(XElement ev, List<Player> baseset, object[] oarr)
    //{
    //  return baseset.Where(x=>x.)
    //}

    #region obsolete code
    //async Task EffectHandler_old(XElement ev, Field field = null, List<Player> basegroup = null, bool isYesNo = false, bool isCounter = false, bool waitForStartPress = false) // not for or choice events!!! >>in that case choice first
    //{
    //  if (waitForStartPress) { Caption = "start"; LongMessage = "click start to start effect handler task for: " + ev.ToString() + "... start!"; await WaitForButtonClick(); Caption = "Ok"; }

    //  var plSelected = CalcPlayersAffectedByPredOnPlayer(ev, basegroup);
    //  var resEffects = GetResourcesForRule(ev);
    //  var effectAction = GetSpecialEffect(ev); // spaeter liste

    //  foreach (var pl in plSelected)
    //  {
    //    var cando = CheckResEffects(pl, resEffects) || effectAction != null; if (!cando) continue;

    //    var num = isCounter ? CalcCounter(ev, pl) : 1; if (num == 0) continue;

    //    MainPlayer = pl; //switch ui to this player
    //    await Task.Delay(longDelay);

    //    if (isYesNo && !isCounter)
    //    {
    //      var answer = await YesNoChoiceTask(ev.astring("text"));
    //      if (!answer) continue;
    //    }

    //    for (int i = 0; i < num; i++)
    //    {
    //      cando = CheckResEffects(pl, resEffects) || effectAction != null; if (!cando) continue;

    //      if (isYesNo && isCounter)
    //      {
    //        var answer = await YesNoChoiceTask(ev.astring("text"));
    //        if (!answer) break;
    //      }

    //      effectAction?.Invoke(new object[] { MainPlayer, field });
    //      foreach (var rtuple in resEffects) await ApplyResEffectTask(rtuple);
    //    }

    //    await Task.Delay(shortDelay);
    //    await WaitForAnimationQueueCompleted();

    //    if (pl != plSelected.Last()) { Message = "next player..."; await WaitForButtonClick(); }
    //  }
    //}

    #endregion

    #region  settings for gameloop (eg how many rounds...)    

    //unused:
    //int iturnround, moveCounter; bool gov;
    //int[] turnrounds = { 1, 1, 0 }; // not used for now! how many times players take turns, set to 100 when indeterminate (finish by passing)
    //int[] actPerMoveDefault = { 1, 1, 0 }; // how many actions does this player have each turn by default (eg sun tzu: 2 actions in first turn)
    #endregion

    //async Task RoundAgeProgressTask()
    //{
    //  Stats.UpdateRound();
    //  Stats.UpdateAge();
    //  Stats.IsWar = false; Stats.WarLevel = 0;
    //  await WaitForRoundMarkerAnimationCompleted();

    //  Progress.Deal();
    //}


    //void CalculateDyamicResourcesForAllPlayers()
    //{
    //  // update special resources for every player
    //  // 5 players*20cards*5 players=500 ops*1000 25million ops persec sollt sich ausgehen
    //  // komplett ohne optimierung: check alle card.X
    //  // optimierung: flags wenn civ card gelegt wird die any of spec resources affecten kann
    //  // optimierung: flag fuer may_affect self and/or others
    //  // optimierungen erst einfuehren wenn too slow!!!!!
    //  foreach (var pl in Players) CalculateDynamicResources(pl);
    //}
    //void CalculateDynamicResources(Player pl)
    //{
    //  Checker.CalcStabAndMil(pl);
    //}

    #endregion

    #region Game
    //public void InitPlayers(int num, IEnumerable<string> civs)
    //{
    //  Debug.Assert(Players != null, "InitPlayers with Players==null");
    //  NumPlayers = num;
    //  Players.Clear();
    //  for (int i = 0; i < NumPlayers; i++)
    //    Players.Add(PlayerType[i] ? new AI(PlayerNames[i], PlayerCivs[i], PlayerBrushes[i], Levels.Chieftain, i) : new Player(PlayerNames[i], PlayerCivs[i], PlayerBrushes[i], Levels.Chieftain, i));
    //  MainPlayer = Players[0];
    //  NotifyPropertyChanged("Players");
    //  NotifyPropertyChanged("NumPlayers");
    //  NotifyPropertyChanged("MainPlayer");

    //}


    //public async Task<Card> IndependentDynastyUpgradeTask(Player pl)
    //{
    //  var dyn = pl.Civ.Dynasties.ToList();
    //  if (dyn.Count == 0) { return null; }
    //  if (dyn.Count == 1) { pl.UpgradeDynasty(dyn[0]); return dyn[0]; }

    //  if (MainPlayer != pl) { MainPlayer = pl; await Task.Delay(1000); }

    //  List<Choice> list = new List<Choice>();
    //  foreach (var c in dyn) { var ch = new Choice(); ch.Tag = c; ch.Text = "upgrade"; list.Add(ch); }

    //  var chosen = await PickCardChoiceTask(list, "dynasty change");
    //  var dyncard = chosen.Tag as Card;
    //  MainPlayer.UpgradeDynasty(dyncard);
    //  return dyncard;
    //}


    //public bool CycleCountingEnabled { get; set; }//(((
    //public void MarkAllPlayerChoices()
    //{
    //  MarkPossibleProgressCards();
    //  MarkCiv(); //TODO: check which cards can be activated
    //  MarkArchitects();
    //  MarkTurmoils();
    //  MarkWorkers();
    //}

    //public async Task MainPlayerClicksOkCancelOrPass()//***
    //{
    //  if (IsAutoTesting) { await Task.Delay(longDelay); CurrentTest.PerformAction(); await Task.Delay(longDelay); }
    //  else { await WaitForThreeButtonClick(); }
    //}
    #region old
    //async Task EffectHandler1(XElement ev, List<Player> basegroup = null, bool isYesNo = false, bool isCounter = false, bool waitForStartPress = false) // not for or choice events!!! >>in that case choice first
    //{
    //  if (waitForStartPress) { Caption = "start"; LongMessage = "click start to start effect handler task for: " + ev.ToString() + "... start!"; await WaitForButtonClick(); Caption = "Ok"; }

    //  var plSelected = CalcPlayersAffected(ev, basegroup);
    //  var resEffects = GetResourcesForRule(ev);
    //  var effectAction = GetSpecialEffect(ev); // spaeter liste

    //  foreach (var pl in plSelected)
    //  {
    //    MainPlayer = pl; //switch ui to this player
    //    await Task.Delay(longDelay);

    //    var num = isCounter ? CalcCounter(ev, pl) : 1;

    //    for (int i = 0; i < num; i++)
    //    {
    //      var resEffectsPossible = CheckResEffects(pl, resEffects);
    //      if (!resEffectsPossible && effectAction == null) break;

    //      if (isYesNo)
    //      {
    //        var answer = await YesNoChoiceTask(ev.astring("text"));
    //        if (!answer) break;
    //      }

    //      effectAction?.Invoke(this, MainPlayer);
    //      foreach (var rtuple in resEffects) await ApplyResEffectTask(rtuple);
    //    }

    //    await Task.Delay(shortDelay);
    //    await WaitForAnimationQueueCompleted();

    //    if (pl != plSelected.Last()) { Message = "next player..."; await WaitForButtonClick(); }
    //  }
    //}
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
    //async Task HandleForeachEvent(XElement ev)
    //{
    //  LongMessage = "event: " + ev.ToString() + "... start!"; await WaitForButtonClick();

    //  var plSelected = CalcPlayersAffected(ev);
    //  var resEffects = GetResourcesForRule(ev);
    //  var effectAction = GetSpecialEffect(ev);

    //  foreach (var pl in plSelected)
    //  {
    //    MainPlayer = pl; //switch ui to this player
    //    await Task.Delay(longDelay);

    //    var num = CalcCounter(ev, pl);
    //    for (int i = 0; i < num; i++)
    //    {
    //      effectAction?.Invoke(this, MainPlayer);
    //      foreach (var rtuple in resEffects) await ApplyResEffectTask(rtuple);
    //    }

    //    await Task.Delay(shortDelay);
    //    await WaitForAnimationQueueCompleted();

    //    if (pl != plSelected.Last()) { Message = "next player..."; await WaitForButtonClick(); }
    //  }
    //}
    //async Task HandleYesnoEvent(XElement ev)
    //{
    //  LongMessage = "event: " + ev.ToString() + "... start!"; await WaitForButtonClick();

    //  var plSelected = CalcPlayersAffected(ev);
    //  var resEffects = GetResourcesForRule(ev);
    //  var effectAction = GetSpecialEffect(ev);

    //  foreach (var pl in plSelected)
    //  {
    //    MainPlayer = pl; //switch ui to this player

    //    var resEffectsPossible = CheckResEffects(pl, resEffects);
    //    if (!resEffectsPossible && effectAction == null) continue;

    //    var answer = await YesNoChoiceTask(ev.astring("text"));

    //    if (answer)
    //    {
    //      effectAction?.Invoke(this, MainPlayer);

    //      if (resEffectsPossible) foreach (var rtuple in resEffects) await ApplyResEffectTask(rtuple);

    //    }
    //    await Task.Delay(shortDelay);
    //    await WaitForAnimationQueueCompleted();

    //    if (pl != plSelected.Last()) { Message = "next player..."; await WaitForButtonClick(); }
    //  }
    //}
    #endregion
    //public int CalcDeployCost(Player pl, Card card)
    //{
    //  var discount = card.mil() ? pl.MilitaryDeployBonus : pl.BuildingDeployBonus;
    //  var deploymentCost = card.GetDeployCost(Stats.Age) - discount;
    //  return deploymentCost >= 0 ? deploymentCost : 0;
    //}

    //static Dictionary<string, Func<object[], bool>> dPredicates = new Dictionary<string, Func<object[], bool>>()
    //{
    //  {"hasWIC", new Func<object[], bool>((oarr) => {var game = Game.GameInstance; var pl = oarr[0] as Player; return pl.HasWIC;}) },

    //};
    //static Dictionary<string, string[]> dActions = new Dictionary<string, string[]>() // cando,do
    //{
    //  {"take_private_architect",new string[]{"hasWIC","take_architect_task"} },
    //};
    //isleast("military",pl,basegroup);

    //{"ndeploy", new Func<string, Player, int>((s,pl) =>
    //{
    //  var n= pl.Cards.Where(x=>x.Type == s).Sum(x=>x.NumDeployed);
    //  return n;
    //}) },

    //async Task DefaultPaymentTask()
    //{
    //  await Task.Delay(2000);
    //}

    #endregion

    #region Player   
    //public void AddCivCard(Card card, Field field)
    //{
    //RemoveCivCard(field);
    //if (card.natural())
    //{
    //  RoundsToWaitForNaturalWonder = card.GetTurns;
    //}
    //Civ.AddCard(card, field);
    //}
    //public void RemoveCivCard(Field field)
    //{
    //  if (!field.IsEmpty)
    //  {
    //    if (field.Card.buildmil()) UndeployWorker(field.Card.NumDeployed);
    //    Civ.RemoveCard(field);
    //  }
    //}
    //public IEnumerable<Res> GetResourceSnapshot() { return Res.List.Where(x => x.CanPayDefaultWith || x.Name == "vp").Select(x => new Res(x.Name, x.Num)).ToList(); }
    //public int CalcRoundEffect(string resname) { var re = RoundEffects.FirstOrDefault(x => x.Name == resname); return re != null ? re.Num : 0; }
    //public int CalcRoundEffectsOnPrice(Card card)
    //{
    //  // there could be a round effect saying that for this player, cards of type t cost more or less, or cards of cost n cost more or less
    //  return CalcRoundEffect("cost" + card.BasicCost) + CalcRoundEffect(card.Type);
    //}
    //public int CalcCardEffect(string resname)
    //{
    //  var re = CardEffects.Where(x => x.Item2.Name == resname).ToList();
    //  var result = re.Count == 0 ? 0 : re.Sum(x => x.Item2.Num);
    //  return result;
    //}
    //public int CalcCardEffectsOnPrice(Card card)
    //{
    //  // there could be a round effect saying that for this player, cards of type t cost more or less, or cards of cost n cost more or less
    //  return CalcCardEffect("cost" + card.BasicCost) + CalcCardEffect(card.Type);
    //}
    //public void CalcCardPrices()
    //{
    //  foreach (var card in Cards) card.Price = card.BasicCost + CalcRoundEffectsOnPrice(card) + CalcCardEffectsOnPrice(card);
    //}
    //public void CalcBasicBonuses()
    //{
    //  // bonuses expressed as positive==good, and they add up
    //  MilminBonus = GoldenAgeBonus = MilitaryDeployBonus = BuildingDeployBonus = 0;
    //  foreach (var card in Cards)
    //  {
    //    MilminBonus += card.X.aint("milminbonus");
    //    GoldenAgeBonus += card.X.aint("goldenagebonus");
    //    MilitaryDeployBonus += card.X.aint("militarydeploybonus");
    //    BuildingDeployBonus += card.X.aint("buildingdeploybonus");
    //  }
    //  MilminBonus += CalcRoundEffect("milminbonus") + CalcCardEffect("milminbonus");
    //  GoldenAgeBonus += CalcRoundEffect("goldenagebonus") + CalcCardEffect("goldenagebonus");
    //  MilitaryDeployBonus += CalcRoundEffect("militarydeploybonus") + CalcCardEffect("militarydeploybonus");
    //  BuildingDeployBonus += CalcRoundEffect("buildingdeploybonus") + CalcCardEffect("buildingdeploybonus");
    //}
    //public int CalcBasicMilitary()
    //{
    //  var mil = 0;
    //  foreach (var c in Cards.Where(x => x != WIC)) { var factor = c.buildmil() ? c.NumDeployed : 1; mil += c.GetMilitary * factor; }
    //  foreach (var w in ExtraWorkers.Where(x => x.IsCheckedOut)) { if (w.CostRes == "military") { mil -= w.Cost; } }
    //  mil += CalcRoundEffect("military") + CalcCardEffect("military");
    //  return mil; //add dyn or other special rules
    //}
    //public int CalcBasicStability()
    //{
    //  var stab = 0;
    //  foreach (var c in Cards.Where(x => x != WIC)) { var factor = c.buildmil() ? c.NumDeployed : 1; stab += c.GetStability * factor; }
    //  foreach (var w in ExtraWorkers.Where(x => x.IsCheckedOut)) { if (w.CostRes == "stability") { stab -= w.Cost; } }
    //  stab += CalcRoundEffect("stability") + CalcCardEffect("stability");
    //  return stab; //add dyn or other special rules
    //}

    //public Dictionary<string, string> BuyBonus { get; set; }
    //public async Task BasicProduction(bool defaultPenalty = true)
    //{
    //  foreach (var c in Cards.Where(x => x != WIC))
    //  {
    //    List<Tuple<string, int>> tuples = c.GetResourceTuples();
    //    var factor = c.buildmil() ? c.NumDeployed : 1;
    //    if (factor == 0) continue;
    //    foreach (var res in tuples)
    //    {
    //      var n = res.Item2;
    //      var resname = res.Item1;
    //      Console.WriteLine("production: " + Name + " card: " + c.Name + ", res: " + resname + " by " + n);
    //      if (n < 0 && defaultPenalty) Pay(-n * factor, resname); else UpdateResBy(resname, n * factor);
    //      await Task.Delay(100);
    //    }
    //  }
    //  foreach (var w in ExtraWorkers.Where(x => x.IsCheckedOut))
    //  {
    //    if (w.CostRes != "military" && w.CostRes != "stability") { Pay(w.Cost, w.CostRes); }
    //  }
    //}
    //public bool MoreThanOneExtraWorkerType()
    //{
    //  var wfree1 = ExtraWorkers.FirstOrDefault(x => !x.IsCheckedOut);
    //  var wfree2 = ExtraWorkers.LastOrDefault(x => !x.IsCheckedOut);
    //  return (wfree1.CostRes != wfree2.CostRes);
    //}


    #endregion

    #region Card
    //public static Card MakeCivCard(Field field, string civ)
    //{
    //  var name = field.X.astring("name");
    //  if (string.IsNullOrEmpty(name)) return MakeEmptyCard(field, field.Type);
    //  else
    //  {
    //    var xcard = Helpers.GetCardX(name, 1);
    //    var card = MakeCard(xcard, true);
    //    field.Card = card;
    //    return card;
    //  }
    //}
    //public static Card MakeCard_old(XElement xcard, bool isCivCard = false)
    //{
    //  var name = xcard.astring("name");
    //  var type = xcard.astring("type");
    //  int age = xcard.aint("age", 1);
    //  Card card = null;
    //  Debug.Assert(CardType.typeColors.ContainsKey(type), "MakeCard: typeColors does not contains key " + type);
    //  card = new Card
    //  {
    //    Type = type,
    //    Brush = CardType.typeColors[type],
    //    Name = name,
    //    Age = age,
    //    X = xcard,
    //    Image = Helpers.GetCardImage(xcard, isCivCard),
    //  };
    //  card.ImageDeployObject = card.buildmil() ? Helpers.GetMiscImage("worker") :
    //      card.wonder() ? Helpers.GetMiscImage("architect") : Helpers.GetMiscImage("cross");

    //  return card;
    //}
    //public static Card MakeCard_old(string name = "archimedes", int age = 1) { var xcard = Helpers.GetCardX(name, age); return MakeCard(xcard); }
    //public static Card MakeEmptyCard(Field field)
    //{
    //  var card = new Card();
    //  var name = field.X.astring("");
    //  card.Type = field.Type;
    //  card.Brush = CardType.typeColors[card.Type];
    //  card.Age = 1;
    //  card.Image = Helpers.GetEmptyCardImage(field.Type);
    //  card.X = field.X;
    //  return card;
    //}

    //public List<Tuple<string, int>> GetResourceTuples()
    //{
    //  List<string> exceptions = new List<string> { "vp", "raid", "military", "stability", "score", "name", "type", "age","private_arch","maxdeploy", "deploy", "res", "eff", "effect", "n", "cause", "milmin", "arch" };
    //  List<Tuple<string, int>> result = new List<Tuple<string, int>>();
    //  var reslist = X.Attributes().Where(x => !exceptions.Contains(x.Name.ToString())).ToList();
    //  foreach (var attr in reslist)
    //  {
    //    var name = attr.Name.ToString();
    //    int n = 0;
    //    var ok = int.TryParse(attr.Value, out n);
    //    Debug.Assert(ok, "GetResourceTuples: Unparsable attribute value for resource " + name+" for card " + this.Name);
    //    result.Add(new Tuple<string, int>(name,n));
    //  }
    //  return result;
    //}
    //public static List<Tuple<string, int>> GetResourceTuplesForRule(XElement xel)
    //{
    //  List<string> exceptions = new List<string> { "pred", "param", "text", "min", "effect", "foreach" };
    //  List<Tuple<string, int>> result = new List<Tuple<string, int>>();
    //  var reslist = xel.Attributes().Where(x => !exceptions.Contains(x.Name.ToString())).ToList();
    //  foreach (var attr in reslist)
    //  {
    //    var name = attr.Name.ToString();
    //    int n = 0;
    //    var ok = int.TryParse(attr.Value, out n);
    //    Debug.Assert(ok, "GetResourceTuplesForRule: Unparsable attribute value for resource " + name);
    //    result.Add(new Tuple<string, int>(name, n));
    //  }
    //  return result;
    //}

    #endregion

    #region helpers
    //public static XElement GetCommonCardX(string cardname)
    //{
    //  var xarr = GetX("cards/xml/_commoncards.xml");
    //  return xarr != null ? xarr.Elements().FirstOrDefault(x => x.astring("name") == cardname) : null;
    //}
    //public static XElement GetCivCardX(string civ, string cardname)
    //{
    //  var xarr = GetX("cards/xml/civcards/" + civ + "cards.xml");
    //  return xarr != null ? xarr.Elements().FirstOrDefault(x => x.astring("name") == cardname) : null;
    //}
    //public static BitmapImage GetCardImageExtNo(string cardname, int age = 1)
    //{
    //  string[] pathnames = { "cards/age" + age + "/age" + age + "_" + cardname + ".jpg",
    //    "cards/civcards/age"+age+ "_" + cardname + ".jpg",
    //    "dyn/age"+age+"/age"+age+ "_" + cardname + ".jpg",
    //    "dyn/dynasties/"+cardname+".jpg",
    //    "cards/civcards/"+cardname+".png",
    //    "cards/civcards/"+cardname+".jpg",
    //  };
    //  foreach (var path in pathnames)
    //  {
    //    Uri uriResult;
    //    bool Uriresult = Uri.TryCreate(URISTART + path, UriKind.Absolute, out uriResult); //geht!
    //    //var ok = CheckUrlExists(uri.AbsolutePath);
    //    //if (!uri.IsFile) continue;
    //    if (!Uriresult) continue;
    //    var result =GetImage(path);
    //    if (result != null) return result;
    //  }
    //  throw new Exception("card with name: " + cardname + " NOT found!!!!");
    //}


    // all die folgenden in 1

    //public static BitmapImage GetEmptyCardImage(string type) { return GetImage("civs/cards/" + type + ".png"); }
    //public static BitmapImage GetCardImage_old(string cardname, int age) { return GetImage("cards/age" + age + "/age" + age + "_" + cardname + ".jpg"); }
    //public static BitmapImage GetEmptyCardImage(string type) { return GetImage("civs/cards/" + type + ".png"); }
    //public static BitmapImage GetCivCardImage(string cardname) { return GetImage("civs/cards/age1_" + cardname + ".jpg"); }
    //public static BitmapImage GetDynCardImage(string civ) { return GetImage("civs/cards/" + civ + "_dyn1.jpg"); }

    #endregion
  }
}
