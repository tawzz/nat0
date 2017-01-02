using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ations.NA
{
  class _trash
  {
    #region Game

    //async Task DefaultPaymentTask()
    //{
    //  await Task.Delay(2000);
    //}

    #endregion

    #region Player   
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

    //public List<Tuple<string, int>> GetResourceTuples()
    //{
    //  List<string> exceptions = new List<string> { "vp", "raid", "military", "stability", "score", "name", "type", "age","private_architect","maxdeploy", "deploy", "res", "eff", "effect", "n", "cause", "milmin", "arch" };
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

  }
}
