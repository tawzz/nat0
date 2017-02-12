using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

namespace ations
{
  public class Step: INotifyPropertyChanged
  {
    public cl Click { get; set; }
    public ctx Context { get; set; }
    public object Obj { get; set; }

    public Func<List<Step>, bool> IsValid { get; set; }
    public Action<List<Step>> Process { get; set; }
    public Action UndoProcess { get; set; }
    public string Message { get { return message; } set { message = value; NotifyPropertyChanged(); } }
    string message;

    public Game GameInst { get; set; }

    public Step(cl click, object obj = null, ctx context = ctx.none) { Click = click; Obj = obj; Context = context; }
    public Step(string txt, Action<List<Step>> processor = null, Func<List<Step>, bool> validator = null, Action undo = null)
    {
      Message = txt;
      IsValid = validator;
      Process = processor;
      UndoProcess = undo;
    }
    public event PropertyChangedEventHandler PropertyChanged; public void NotifyPropertyChanged([CallerMemberName] string propertyName = null) { this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
    public override string ToString()
    {
      return Click.ToString() + " " + Context.ToString() + " " + Obj.ToString() + " " + Message;
    }
  }

}
