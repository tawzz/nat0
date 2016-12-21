using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace ations
{
  public class AAni
  {
    public async static Task RunStoryboard(Storyboard Story, FrameworkElement item)
    {
      Story.Begin(item);
      while (Story.GetCurrentState() == ClockState.Active && Story.GetCurrentTime() < Story.Duration)
      {
        await Task.Delay(100);
      }
    }
  }
}
