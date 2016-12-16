using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Reflection;

namespace ations
{
  public partial class Game
  {
    public static Tuple<Player[], Player[], Player[]> Split(string res, int bound, IEnumerable<Player> pl)
    {
      // returns players with smaller,exactly,greater resnum(res)
      return new Tuple<Player[], Player[], Player[]>(
        pl.Where(x => x.resnum(res) < bound).ToArray(),
        pl.Where(x => x.resnum(res) == bound).ToArray(),
        pl.Where(x => x.resnum(res) > bound).ToArray());
    }
    //selectors
    static Dictionary<string, Func<string, IEnumerable<Player>, Player[]>> sel4 = new Dictionary<string, Func<string, IEnumerable<Player>, Player[]>>()
    {
      {"least", new Func<string, IEnumerable<Player>, Player[]>((s,pl) => { return Split(s, pl.Min(x => x.resnum(s)), pl).Item2; }) },//Least(s,pl); })},
      {"most", new Func<string, IEnumerable<Player>, Player[]>((s,pl) => { return Split(s, pl.Min(x => x.resnum(s)), pl).Item3; }) },//Most(s,pl); })},
      {"notleast", new Func<string, IEnumerable<Player>, Player[]>((s,pl) => { return Split(s, pl.Max(x => x.resnum(s)), pl).Item2; }) },//NotLeast(s,pl); })},
      {"notmost", new Func<string, IEnumerable<Player>, Player[]>((s,pl) => { return Split(s, pl.Max(x => x.resnum(s)), pl).Item1; }) },//NotLeast(s,pl); })},
      {"exleast", new Func<string, IEnumerable<Player>, Player[]>((s,pl) => { return Split(s, pl.Max(x => x.resnum(s)), pl).Item2; }) },//NotLeast(s,pl); })},
      {"exmost", new Func<string, IEnumerable<Player>, Player[]>((s,pl) => { return Split(s, pl.Max(x => x.resnum(s)), pl).Item1; }) },//NotLeast(s,pl); })},
    };
    //actions
    static Dictionary<string, Action<IEnumerable<Player>, string, int>> act4 = new Dictionary<string, Action<IEnumerable<Player>, string, int>>()
    {
      {":res",new Action<IEnumerable<Player>, string,int>((pl,s,i)=> { foreach (var p in pl) p.res(s).Num += i; }) },
    };

    public void eval()
    {//           op      res    :action  inc res

      var rule = "|least military :res -3 book";
      var ws = rule.Split(null);
      var dictkey = ws[0][0];//leads to type of rule, eg: @time  |pred param :action 

      var op = ws[0].a1();
      // object ld = dict[dictkey][op];

      var res = ws[1];
      if (sel4.ContainsKey(op))
      {
        var pl = sel4[op](res, Players);
        //foreach of these players, do the action

      }

    }
















    Dictionary<string, Action> selector = new Dictionary<string, Action>()
    {
      { "least", new Action(() => { var b=4; }) }
    };
    Dictionary<string, Action<string>> sel1 = new Dictionary<string, Action<string>>()
    {
      { "least", new Action<string>((s) => { var b=4; }) }
    };
    Dictionary<string, Func<string, Player[]>> sel2 = new Dictionary<string, Func<string, Player[]>>()
    {
      { "least", new Func<string,Player[]>((s) => { var m=s; return new Player[1]; }) }
    };
    Dictionary<string, Func<string, Player[]>> sel3 = new Dictionary<string, Func<string, Player[]>>();
    //selectors
    //public static Player[] Least(string res, IEnumerable<Player> pl) { return Split(res, pl.Min(x => x.resnum(res)), pl).Item2; }
    //public static Player[] NotLeast(string res, IEnumerable<Player> pl) { return Split(res, pl.Min(x => x.resnum(res)), pl).Item3; }
    //public static Player[] Most(string res, IEnumerable<Player> pl) { return Split(res, pl.Max(x => x.resnum(res)), pl).Item2; }
    //public static Player[] NotMost(string res, IEnumerable<Player> pl) { return Split(res, pl.Max(x => x.resnum(res)), pl).Item1; }
    //actions
    //in act4    public static void ModifyResNum(IEnumerable<Player> pl, string res, int n) { foreach (var p in pl) p.res(res).Num += n; }

    //too complex
    //static Dictionary<char, object> dict = new Dictionary<char, object>()
    //{
    //  { '|',sel4 },
    //  { ':',act4 },
    //};



  }
}
