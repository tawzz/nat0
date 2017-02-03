using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ations;

namespace ations
{
  public class Test
  {
    public bool isdyn,  isfake, sstart, sgro, sact, sprod,  sord,  swar,  sev;
    public Action scene;
    public Action testAction;
    public List<Action> testActions = new List<Action>();
    int actionIndex=0;
    public Func<bool> testVerification;
    public string name, description;
    public int nplayers, ncols, nrounds;
    public Dictionary<string,int> ResBefore { get; set; }
    public Dictionary<string, int> ResAfter { get; set; }
    public Dictionary<string, int> ResDiff { get; set; }

    public Test() { }
    public Test(string name, Action scene, List<Action> testActions, Func<bool> verify, string description, 
      int npls, int ncols, int nrounds, bool isdyn, bool isfake,
      bool sstart, bool sgro, bool sact, bool sprod, bool sord, bool swar, bool sev)
    {
      this.isdyn = isdyn; this.isfake = isfake; this.sstart = sstart;  this.sgro = sgro; this.sact = sact; this.sprod = sprod; this.sord = sord; this.sev = sev;
      this.scene = scene;
      this.testActions = testActions;
      this.testVerification = verify;
      this.name = name;
      this.description = description;
      this.nplayers = npls;
      this.ncols = ncols;
      this.nrounds = nrounds; 
    }

    public void Setup()
    {
      var game = Game.Inst;
      actionIndex = 0;
      //game.Title = name;
      game.Message = description;
      game.SwitchRoundAndAgeOn = sstart;
      game.SwitchGrowthOn = sgro;
      game.SwitchActionOn = sact;
      game.SwitchEventOn = sev;
      game.SwitchOrderOn = sord;
      game.SwitchProductionOn = sprod;
      game.SwitchWarOn = swar;
    }
    public void Scenario()
    {
      var game = Game.Inst;
      scene?.Invoke();
      ResBefore = game.MainPlayer.GetResourceSnapshot();
    }
    public void PerformAction()
    { if (actionIndex < testActions.Count)
      {
        testActions[actionIndex]?.Invoke();

        actionIndex++;
      }
      //else Game.Inst.IsAutoTesting = false;

    }
    public bool Verify()
    {
      var game = Game.Inst;
      ResAfter = game.MainPlayer.GetResourceSnapshot();
      ResDiff = game.MainPlayer.GetResDiff(ResBefore, ResAfter);
      var result = testVerification?.Invoke();
      return result??false;
    }
  }
}
