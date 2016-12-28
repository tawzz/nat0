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
    Dictionary<cl[], Info[]> dStart = new Dictionary<cl[], Info[]>()
    {
      {new cl[] {cl.prog},
        new Info[] {new Info("buy $0?", validator: OneStepBuy) } },
      {new cl[] {cl.prog, cl.cciv},
        new Info[] {new Info("select space"), new Info("buy $0?", validator: Fits) } },
      {new cl[] {cl.prog, cl.cciv, cl.cciv},
        new Info[] {new Info("select space"), new Info("buy $0?", validator: Fits), new Info("buy $0?", Remove2ndLast, Fits), } },
      {new cl[] {cl.cciv, cl.cciv},
        new Info[] {new Info("select deploy source", validator: BuildingOrMilitary), new Info("deploy from $1?", validator: CanDeployFromLast)} },
      {new cl[] {cl.cciv, cl.worker},
        new Info[] {new Info("select deploy source", validator: BuildingOrMilitary), new Info("deploy worker?")} },
      {new cl[] {cl.worker,cl.cciv},
        new Info[] {new Info("select deploy target"), new Info("deploy worker?", validator: BuildingOrMilitary) } },
      {new cl[] {cl.arch},
        new Info[] {new Info("take architect") } },
      {new cl[] {cl.arch, cl.cciv},
        new Info[] { new Info("select wonder space"), new Info("place wonder?",UpdateSelectedField, NewWonderPlacement,ResetSelectedField) } },
      {new cl[] {cl.turm},
        new Info[] {new Info("take turmoil") } },

    };

    public static bool NewWonderPlacement(List<Step> list) { return FirstContext(list) == ctx.wready && FirstField(list).TypesAllowed.Contains("wonder"); }
    public static bool BuildingOrMilitary(List<Step> list) { return LastField(list).Card.buildmil(); }
    public static bool Fits(List<Step> list) { return list.Count > 1 && LastField(list).TypesAllowed.Contains(FirstField(list).Card.Type); }
    public static bool CanDeployFromLast(List<Step> list) { var card = LastField(list).Card; return card.buildmil() && card.NumDeployed > 0; }
    public static bool OneStepBuy(List<Step> list) { return list.Count == 1 && Game.instance.IsOneStepBuy(list[0].Obj as Field); }

    public static void Remove2ndLast(List<Step> list) { list.RemoveAt(list.Count - 2); }
    public static void UpdateSelectedField(List<Step> list) { Game.instance.SelectedField = LastField(list); }
    public static void ResetSelectedField() { Game.instance.SelectedField = null; }


    public static Step LastStep(List<Step> list) { Debug.Assert(list.LastOrDefault() != null, "NULL STEP!!!"); return list.Last(); }
    public static Step FirstStep(List<Step> list) { Debug.Assert(list.FirstOrDefault() != null, "NULL STEP!!!"); return list.First(); }
    public static Field LastField(List<Step> list) { Debug.Assert(LastStep(list).Obj is Field,"LAST NOT FIELD!!!"); return LastStep(list).Obj as Field; }
    public static Field FirstField(List<Step> list) { Debug.Assert(FirstStep(list).Obj is Field, "FIRST NOT FIELD!!!"); return FirstStep(list).Obj as Field; }
    public static ctx LastContext(List<Step> list) { return LastStep(list).Context; }
    public static ctx FirstContext(List<Step> list) { return FirstStep(list).Context; }
  }
}
