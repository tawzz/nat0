using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ations
{
  public partial class Game
  {
    static Dictionary<cl[], Step[]> dStart = new Dictionary<cl[], Step[]>()
    {
      {new cl[] {cl.prog},
        new Step[] {new Step("buy $0?", validator: OneStepBuy) } },
      {new cl[] {cl.prog, cl.civ},
        new Step[] {new Step("select space"), new Step("buy $0?", validator: LastFitsFirst) } },
      {new cl[] {cl.prog, cl.civ, cl.civ},
        new Step[] {new Step("select space"), new Step("buy $0?", validator: LastFitsFirst), new Step("buy $0?", validator: LastFitsFirst, processor: Remove2ndLast), } },
      {new cl[] {cl.civ, cl.civ},
        new Step[] {new Step("select deploy source", validator: BuildingOrMilitary), new Step("deploy from $1?", validator: CanDeployFromLast)} },
      {new cl[] {cl.civ, cl.worker},
        new Step[] {new Step("select deploy source", validator: BuildingOrMilitary), new Step("deploy worker?")} },
      {new cl[] {cl.worker,cl.civ},
        new Step[] {new Step("select deploy target"), new Step("deploy worker?", validator: BuildingOrMilitary) } },
      {new cl[] {cl.arch},
        new Step[] {new Step("take architect") } },
      {new cl[] {cl.turm},
        new Step[] {new Step("take turmoil") } },
    };
    static Dictionary<cl[], Step[]> dWonderReady = new Dictionary<cl[], Step[]>()
    {
      {new cl[] {cl.civ},
        new Step[] {new Step("place wonder?", validator: AllowsWonder) } },
      {new cl[] {cl.civ, cl.civ},
        new Step[] {new Step("place wonder?", validator: AllowsWonder, processor:Remove2ndLast) } },
    };
    static Dictionary<ctx, string> contextStartMessage = new Dictionary<ctx, string>()
    {
      { ctx.none, "" },
      { ctx.special, "" }, // future
      { ctx.start, "$Player, choose action" },
      { ctx.wready, "wonder is ready!" },
    };
    static Dictionary<ctx, Dictionary<cl[], Step[]>> contextDictionaries = new Dictionary<ctx, Dictionary<cl[], Step[]>>()
    {
      { ctx.none, new Dictionary<cl[], Step[]>() },
      { ctx.special, new Dictionary<cl[], Step[]>() }, // future
      { ctx.start, dStart },
      { ctx.wready, dWonderReady },
    };

    public static bool AllowsWonder(List<Step> list) { return LastField(list).TypesAllowed.Contains("wic"); }
    public static bool BuildingOrMilitary(List<Step> list) { return LastField(list).Card.buildmil(); }
    public static bool LastFitsFirst(List<Step> list) { return list.Count > 1 && LastField(list).TypesAllowed.Contains(FirstField(list).Card.Type); }
    public static bool CanDeployFromLast(List<Step> list) { var card = LastField(list).Card; return card.buildmil() && card.NumDeployed > 0; }
    public static bool OneStepBuy(List<Step> list) { return list.Count == 1 && Game.instance.IsOneStepBuy(list[0].Obj as Field); }
    public static void Remove2ndLast(List<Step> list) { list.RemoveAt(list.Count - 2); }

    public static Step LastStep(List<Step> list) { Debug.Assert(list.LastOrDefault() != null, "NULL STEP!!!"); return list.Last(); }
    public static Step FirstStep(List<Step> list) { Debug.Assert(list.FirstOrDefault() != null, "NULL STEP!!!"); return list.First(); }
    public static Field LastField(List<Step> list) { Debug.Assert(LastStep(list).Obj is Field, "LAST NOT FIELD!!!"); return LastStep(list).Obj as Field; }
    public static Field FirstField(List<Step> list) { Debug.Assert(FirstStep(list).Obj is Field, "FIRST NOT FIELD!!!"); return FirstStep(list).Obj as Field; }
    public static ctx LastContext(List<Step> list) { return LastStep(list).Context; }
    public static ctx FirstContext(List<Step> list) { return FirstStep(list).Context; }
  }
}
