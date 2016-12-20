//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Runtime.CompilerServices;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows.Threading;

//namespace ations
//{
//  public class DispatchToUIThread : INotifyCompletion
//  {
//    private readonly Dispatcher dispatcher;

//    public static DispatchToUIThread Awaiter { get; private set; }

//    private DispatchToUIThread(Dispatcher dispatcher)
//    {
//      this.dispatcher = dispatcher;
//    }

//    [CLSCompliant(false)]
//    public static void Initialize(Dispatcher dispatcher)
//    {
//      if (dispatcher == null) throw new ArgumentNullException("dispatcher");
//      Awaiter = new DispatchToUIThread(dispatcher);
//    }

//    public DispatchToUIThread GetAwaiter()
//    {
//      return this;
//    }

//    public bool IsCompleted
//    {
//      get { return this.dispatcher.HasThreadAccess; }
//    }

//    public async void OnCompleted(Action continuation)
//    {
//      if (continuation == null) throw new ArgumentNullException("continuation");
//      await this.dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => continuation());
//    }

//    public void GetResult() { }
//  }
  
