using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ations
{
  public class Step
  {
    public cl Click { get; set; }
    public ctx Context { get; set; }
    public object Obj { get; set; }
    public Info Info { get; set; }

    public Step(cl click, object obj = null, ctx context = ctx.none) { Click = click; Obj = obj; Context = context; }
    public override string ToString()
    {
      return Click.ToString() + " " + Context.ToString() + " " + Obj.ToString();
    }
  }

}
