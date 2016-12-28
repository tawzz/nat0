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
  public class Info
  {
    public Func<List<Step>, bool> IsValid { get; set; }
    public Action<List<Step>> Process { get; set; }
    public Action UndoProcess { get; set; }
    public string Text { get; set; }

    public Info(string txt, Action<List<Step>> processor = null, Func<List<Step>, bool> validator = null, Action undo = null)
    {
      Text = txt;
      IsValid = validator;
      Process = processor;
      UndoProcess = undo;
    }
  }

}
