using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ations
{
  public static class Commands
  {
    public static readonly RoutedUICommand Buy =
      new RoutedUICommand("Buy", "Buy", typeof(Commands),
        new InputGestureCollection() { new KeyGesture(Key.F2, ModifierKeys.Alt) });
  }
}
