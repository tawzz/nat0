using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ations
{
  public class GameDesignTime
  {
    public Game Game { get; set; }
    public GameDesignTime()
    {
      Game = Game.GameInstance;
    }
  }
}
