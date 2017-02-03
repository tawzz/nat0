using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ations
{
  public partial class App : Application
  {
    private void OnException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
      var msg = e.Exception.InnerException.Message;
      int nttttttttttttttttttt = 4;
    }
  }
}
