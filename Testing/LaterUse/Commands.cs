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
    //example of command (for later)
    public static readonly RoutedUICommand BuyExample =
      new RoutedUICommand("BuyExample", "BuyExample", typeof(Commands),
        new InputGestureCollection() { new KeyGesture(Key.F2, ModifierKeys.Alt) });
  }
}
