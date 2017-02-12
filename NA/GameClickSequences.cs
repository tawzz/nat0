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
        new Step[] {new Step("select deploy source", validator: BuildingOrMilitaryUnderMaxDeploy), new Step("deploy from $1?", validator: CanDeployFromLast)} },
      {new cl[] {cl.civ},
        new Step[] {new Step("perform action?", validator: ActionPossible, processor: SetActionHandlingTrue) } }, 
      {new cl[] {cl.civ, cl.worker},
        new Step[] {new Step("select deploy source", validator: BuildingOrMilitaryUnderMaxDeploy), new Step("deploy worker?")} },
      {new cl[] {cl.worker,cl.civ},
        new Step[] {new Step("select deploy target"), new Step("deploy worker?", validator: BuildingOrMilitaryUnderMaxDeploy) } },
      {new cl[] {cl.arch},
        new Step[] {new Step("take architect?") } },
      {new cl[] {cl.turm},
        new Step[] {new Step("take turmoil?") } },
      {new cl[] {cl.cspecial},
        new Step[] {new Step("perform action?", validator: SpecialOptionPossible, processor: SetSpecialOptionHandlingTrue) } },
    };
    static Dictionary<cl[], Step[]> dSwapCards = new Dictionary<cl[], Step[]>()
    {
      {new cl[] {cl.prog},
        new Step[] {new Step("put cross here?", validator: (list) => { return LastGame(list).Context.Id == ctx.pickProgress; }) } },
      {new cl[] {cl.prog, cl.prog},
        new Step[] {new Step("select another progress card"), new Step("swap?")} },
    };
    static Dictionary<cl[], Step[]> dPickCivField = new Dictionary<cl[], Step[]>()
    {
      {new cl[] {cl.civ},
        new Step[] {new Step("click ok to confirm choice") } },
      {new cl[] {cl.civ, cl.civ},
        new Step[] {new Step("click ok to confirm choice", processor:Remove2ndLast) } },
    };
    static Dictionary<cl[], Step[]> dWonderReady = new Dictionary<cl[], Step[]>()//weg
    {
      {new cl[] {cl.civ},
        new Step[] {new Step("place wonder?", validator: AllowsWonder) } },
      {new cl[] {cl.civ, cl.civ},
        new Step[] {new Step("place wonder?", validator: AllowsWonder, processor:Remove2ndLast) } },
    };
    static Dictionary<cl[], Step[]> dBMChooser = new Dictionary<cl[], Step[]>()//weg
    {
      {new cl[] {cl.civ},
        new Step[] {new Step("undeploy?", validator: HasWorkerDeployedContextUndeploy) } },
      {new cl[] {cl.civ, cl.civ},
        new Step[] {new Step("undeploy?", validator: HasWorkerDeployedContextUndeploy, processor:Remove2ndLast) } },
      {new cl[] {cl.civ},
        new Step[] {new Step("deploy?", validator:(list)=>{ return LastGame(list).Context.Id == ctx.deployWorker; }) } },
      {new cl[] {cl.civ, cl.civ},
        new Step[] {new Step("deploy?", validator: (list) => { return LastGame(list).Context.Id == ctx.deployWorker; }, processor:Remove2ndLast) } },
    };
    static Dictionary<cl[], Step[]> dMChooser = new Dictionary<cl[], Step[]>()//weg
    {
      {new cl[] {cl.civ},
        new Step[] {new Step("undeploy?", validator: HasMilitaryWorkerDeployed) } },
      {new cl[] {cl.civ, cl.civ},
        new Step[] {new Step("updeploy?", validator: HasMilitaryWorkerDeployed, processor:Remove2ndLast) } },
    };
    static Dictionary<ctx, string> contextStartMessage = new Dictionary<ctx, string>()
    {
      { ctx.none, "" },
      { ctx.special, "" }, // future//weg
      { ctx.start, "$Player, choose action" },
      { ctx.pickCivField, "$Player, select civ card" },
      { ctx.wready, "wonder is ready!" },//weg
      { ctx.swapprogress, "swap two progress cards!" },
      { ctx.pickProgress, "pick progress card!" },
      { ctx.removeWorker, "undeploy worker?" },//weg
      { ctx.deployWorker, "deploy worker?" },//weg
      { ctx.removeMilitaryWorker, "undeploy worker?" },//weg
    };
    static Dictionary<ctx, Dictionary<cl[], Step[]>> contextDictionaries = new Dictionary<ctx, Dictionary<cl[], Step[]>>()
    {
      { ctx.none, new Dictionary<cl[], Step[]>() },
      { ctx.special, new Dictionary<cl[], Step[]>() }, // future//weg
      { ctx.start, dStart },
      { ctx.pickCivField, dPickCivField },
      { ctx.wready, dWonderReady },//weg
      { ctx.swapprogress, dSwapCards },
      { ctx.pickProgress, dSwapCards },
      { ctx.removeWorker, dBMChooser },//weg
      { ctx.deployWorker, dBMChooser },//weg
      { ctx.removeMilitaryWorker, dMChooser },//weg
    };

    //public static bool IsCTX(ctx context) { return GameInst.Context.Id == context; }
    public static bool AllowsWonder(List<Step> list) { return LastField(list).TypesAllowed.Contains("wic"); }
    public static bool BuildingOrMilitaryUnderMaxDeploy(List<Step> list) { var last = LastField(list); var result = last.Card.CanDeployOn; return result; }// var card = last.Card; var maxdeploy = card.X.aint("maxdeploy", 100); var res = card.buildmil() && card.NumDeployed < maxdeploy; return res;}
    public static bool HasWorkerDeployedContextUndeploy(List<Step> list) { var last = LastField(list); var result = last.Card.buildmil() && last.Card.NumDeployed > 0; return LastGame(list).Context.Id == ctx.removeWorker && result; }// LastField(list).Card.buildmil(); }
    public static bool HasMilitaryWorkerDeployed(List<Step> list) { var last = LastField(list); var result = last.Card.mil() && last.Card.NumDeployed > 0; return result; }// LastField(list).Card.buildmil(); }
    public static bool LastFitsFirst(List<Step> list) { return list.Count > 1 && LastField(list).TypesAllowed.Contains(FirstField(list).Card.Type); }
    public static bool CanDeployFromLast(List<Step> list) { var card = LastField(list).Card; return card.buildmil() && card.NumDeployed > 0; }
    public static bool OneStepBuy(List<Step> list) { return list.Count == 1 && LastGame(list).IsOneStepBuy(list[0].Obj as Field); }
    public static void Remove2ndLast(List<Step> list) { list.RemoveAt(list.Count - 2); }
    public static bool ActionPossible(List<Step> list) { var last = LastField(list); var result = !last.Card.buildmil() && LastGame(list).Checker.CheckActionPossible(LastGame(list).MainPlayer, last.Card); return result; }
    public static bool SpecialOptionPossible(List<Step> list) { var last = LastChoice(list); var result = LastGame(list).Checker.CheckActionPossible(LastGame(list).MainPlayer, last.Tag as Card); return result; }
    public static void SetActionHandlingTrue(List<Step> list) { LastGame(list).IsCardActivation = true; }
    public static void SetSpecialOptionHandlingTrue(List<Step> list) { LastGame(list).IsSpecialOptionActivation = true; }

    public static Game LastGame(List<Step> list) { return LastStep(list).GameInst; }
    public static Step LastStep(List<Step> list) { Debug.Assert(list.LastOrDefault() != null, "NULL STEP!!!"); return list.Last(); }
    public static Step FirstStep(List<Step> list) { Debug.Assert(list.FirstOrDefault() != null, "NULL STEP!!!"); return list.First(); }
    public static Field LastField(List<Step> list) { Debug.Assert(LastStep(list).Obj is Field, "LAST NOT FIELD!!!"); return LastStep(list).Obj as Field; }
    public static Choice LastChoice(List<Step> list) { Debug.Assert(LastStep(list).Obj is Choice, "LAST NOT Choice!!!"); return LastStep(list).Obj as Choice; }
    public static Field FirstField(List<Step> list) { Debug.Assert(FirstStep(list).Obj is Field, "FIRST NOT FIELD!!!"); return FirstStep(list).Obj as Field; }
    public static ctx LastContext(List<Step> list) { return LastStep(list).Context; }
    public static ctx FirstContext(List<Step> list) { return FirstStep(list).Context; }
  }
}
