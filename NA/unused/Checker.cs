using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ations;
using System.Collections.ObjectModel;
using System.IO;
using ations;
using System.Xml.Linq;

namespace ations
{
  public class Checker_old // not used!!!!!!!!!!!!!!!!!!!!!!!!!!!
  {
    public ObservableCollection<string> Report { get; set; }
    public Checker_old() { Report = new ObservableCollection<string>(); }
    public CheckerInfo ResInfo { get; set; }

    public async Task CheckPreAction(string timing, Card card = null, Res res = null, Field from = null, Field to = null)
    {
      ResInfo = new CheckerInfo(timing); ResInfo.PreAction();

      //foreach(var c in Game.Inst.MainPlayer.Cards) await StupidCheckerOfOwnCards(c, timing, card, res, from, to);
    }

    //public async Task StupidCheckerOfOwnCards(Card c, string timing, Card card = null, Res res = null, Field from = null, Field to = null)
    //{
    //  await Task.Delay(100);
    //  var game = Game.Inst;
    //  var pl = game.MainPlayer;
    //  switch (timing)
    //  {
    //    case "first_turn":
    //      switch (card.Name)
    //      {
    //        case "buddha": pl.NumActions = 0; break;
    //        case "sun_tsu": pl.NumActions = 2; break;
    //      }
    //      break;


    //  }
    //}


      //// for all players and all civcards identify relevant card info for this timing event
      //var game = Game.Inst;
      //var plMain = game.MainPlayer;
      //foreach(var c in plMain.Cards)
      //{
      //  switch (card.Name)
      //  {
      //    "archimedes":
      //  }
      //}
      //foreach (var owner in game.Players)
      //{
      //  foreach (var c in owner.Cards)
      //  {
      //    var clause = CheckIfCardRelevantFor(c, owner, timing, plMain);
      //    if (clause != null)
      //    {
      //      var pls = TestIfPreconditionFulFilled(clause, owner, card, res, from, to);
      //      //test precondition
      //      // if clause does not have a precondition the precondition is met
      //      // if clause has a precondition, test whether it is met
      //      // precond is met if 

    //      //execute if met
    //    }
    //  }
    //}

    //}
    //List<Player> TestIfPreconditionFulFilled(XElement clause, Player owner, Card card = null, Res res = null, Field from = null, Field to = null)
    //{
    //  var result = new List<Player>();
    //  var game = Game.Inst;
    //  var plMain = game.MainPlayer;

    //  // is there a predicate = precondition?
    //  var pred = clause.astring("pred");
    //  if (string.IsNullOrEmpty(pred)) return owner == plMain ? new List<Player> { plMain }:;

    //  // which group of players has to be tested for the precondition? >keyword group, default is owner
    //  var group = clause.astring("group");
    //  var pls = new List<Player>();
    //  if ((string.IsNullOrEmpty(group) && plMain==owner) || group == "self") pls.Add(owner);
    //  else if ((string.IsNullOrEmpty(group) && plMain != owner) || group == "all") pls.AddRange(game.Players);
    //  else if (group == "others") pls.AddRange(game.Players.Where(x => x != owner));

    //  // test condition for all players in pls
    //  // return all players for which the precond is true

    //  return result;
    //}
    //XElement CheckIfCardRelevantFor(Card card, Player owner, string timing, Player pl)//assume just one relevant clause for each timing!
    //{
    //  var clause = card.X.Elements(timing).FirstOrDefault() ??
    //    card.X.Elements().FirstOrDefault(x=>x.Attributes().Any(y=>y.Name.ToString() ==timing));
    //  if (clause == null) return null;
    //  var affects = clause.astring("affects");
    //  if (pl == owner && (string.IsNullOrEmpty(affects) || affects == "self")) return clause;
    //  if (pl != owner && (affects == "all" || affects =="others")) return clause;
    //  return null;
    //}


    public void CheckPostAction(string timing, Card card = null, Res res = null, Field from = null, Field to = null)
    {
      if (ResInfo == null) ResInfo = new CheckerInfo(timing);
      ResInfo.PostAction();

      Report.Add(ResInfo.Summary);
    }

  }
  public class CheckerInfo
  {
    public string Timing { get; set; }
    public string Summary { get; set; }
    public Dictionary<Player, Dictionary<string, int>> ResBefore = new Dictionary<Player, Dictionary<string, int>>();
    public Dictionary<Player, Dictionary<string, int>> ResAfter = new Dictionary<Player, Dictionary<string, int>>();
    public Dictionary<Player, Dictionary<string, int>> ResDiff = new Dictionary<Player, Dictionary<string, int>>();

    public CheckerInfo(string timing) { this.Timing = timing; }
    public void PreAction()
    {
      ResBefore.Clear();
      var game = Game.Inst;
      foreach (var pl in game.Players)
      {
        var resBefore = pl.GetResourceSnapshot();
        ResBefore.Add(pl, resBefore);
      }

    }
    public void PostAction()
    {
      var game = Game.Inst;
      ResAfter.Clear(); ResDiff.Clear();
      if (ResBefore == null) ResBefore = new Dictionary<Player, Dictionary<string, int>>();
      foreach (var pl in game.Players)
      {
        var resAfter = pl.GetResourceSnapshot();
        if (ResBefore[pl] == null) ResBefore[pl] = new Dictionary<string, int>();
        var resDiff = pl.GetResDiff(ResBefore[pl], ResAfter[pl]);
        ResAfter.Add(pl, resAfter);
        ResDiff.Add(pl, resDiff);
      }
      var mpl = game.MainPlayer;
      string summary = mpl.Name + "{";
      var dict = ResDiff[mpl];
      foreach (var k in dict.Keys.Where(x => dict[x] != 0)) summary += " " + k + ":" + dict[k];
      summary += "}";
      Summary = summary;
    }
  }
}
