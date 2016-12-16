using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ations
{
  public class Engine//performs a specific step of the game
  {
    protected Game game;
    protected Object data;
    public Engine(Game game, object data) { this.game = game; this.data = data; }
    public virtual void start() { }
    public void prompt() { }
    public void processInput() { }
    public void end() { }
  }
  public class MaintenanceEngine : Engine
  {
    public MaintenanceEngine(Game g, object d) : base(g, d)
    {
      g.ablauf.Add("roundMarker", new RoundMarkerEngine(g, d));
      g.ablauf.Add("progressCards", new ProgressCardsEngine(g, d));
      g.ablauf.Add("growth", new GrowthEngine(g, d));
      g.ablauf.Add("newEvents", new NewEventsEngine(g, d));
      g.ablauf.Add("architectsEngine", new ArchitectsEngine(g, d));
    }
    public override void start()
    {
      base.start();
      game.Round++;
    }
  }
  public class RoundMarkerEngine : Engine
  {
    public RoundMarkerEngine(Game g, object d) : base(g, d) { }
  }
  public class ProgressCardsEngine : Engine
  {
    public ProgressCardsEngine(Game g, object d) : base(g, d) { }
  }
  public class GrowthEngine : Engine
  {
    public GrowthEngine(Game g, object d) : base(g, d) { }
  }
  public class NewEventsEngine : Engine
  {
    public NewEventsEngine(Game g, object d) : base(g, d) { }
  }
  public class ArchitectsEngine : Engine
  {
    public ArchitectsEngine(Game g, object d) : base(g, d) { }
  }
  public class ActionEngine : Engine
  {
    public ActionEngine(Game g, object d) : base(g, d) { }
  }
  public class ResolutionEngine : Engine
  {
    public ResolutionEngine(Game g, object d) : base(g, d) { }
  }

}
