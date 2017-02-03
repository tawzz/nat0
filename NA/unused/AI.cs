using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ations
{
  public class AI : Player
  {
    public new bool IsAI { get { return true; } }

    public AI(string name, string civ, Brush brush, int level, int index) : base(name, civ, brush, level, index) { }



  }
}
