using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ations;
using ations.MA;
namespace ations
{
  public partial class App : Application
  {
    public VM VM { get { if (_vm == null) _vm = new VM(); return _vm; } }
    private VM _vm = null;

    private void OnException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
      var msg = e.Exception.InnerException.Message;
    }

    private void OnStartup(object sender, StartupEventArgs e)
    {
    }
  }
}
