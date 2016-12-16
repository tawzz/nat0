using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace ations
{
  public class AAnimations
  {
    public static FrameworkElement RoundMarkerUI { get; set; }
    public static Action<object> AfterAnimation { get; set; } // so far param is not needed

    public static void AniRoundMarker(Point goalPosition, Action<object> afterAni)
    {
      AfterAnimation = afterAni;
      if (RoundMarkerUI == null) AfterAnimation?.Invoke(null);
      else
      {
        //var ani = Storyboards.Appear(target, TimeSpan.FromSeconds(1));
        var ani = Storyboards.MoveTo(RoundMarkerUI, goalPosition, TimeSpan.FromSeconds(1), null);
        ani.Completed += (s, _) => GlobalCompleted(ani);
        ani.Begin();
      }
    }
    public static void AniResourceUpdate(FrameworkElement target)
    {
      var ani = Storyboards.Scale(target, TimeSpan.FromSeconds(.3), new Point(1, 1), new Point(5, 2), null, true);
      ani.Completed += (s, _) => GlobalCompleted(ani);
      ani.Begin();
    }

    static void GlobalCompleted(object sb, object param = null)
    {
      (sb as Storyboard).Completed -= GlobalCompleted;
      AfterAnimation?.Invoke(param);
    }


  }
}
