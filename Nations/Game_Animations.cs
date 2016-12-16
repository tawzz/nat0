using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Reflection;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media.Animation;

namespace ations
{
  public partial class Game
  {
    //public void EnableUI=true { if (NextButton != null) NextButton.IsEnabled = true; }
    //public void DisableUI() { if (NextButton != null) NextButton.IsEnabled = false; }
    void GlobalCompleted(object s, object targetOrAction)
    {
      var sb = s as Storyboard;
      sb.Completed -= GlobalCompleted;
      var action = targetOrAction as Action;
      if (action != null) action(); else Phase.AniCompletedAction?.Invoke(sb,targetOrAction);
      EnableUI=true;
    }
    //animations:
    public void AniRoundMarker(FrameworkElement target)
    {
      //var ani = Storyboards.Appear(target, TimeSpan.FromSeconds(1));
      var ani = Storyboards.MoveTo(target, RoundMarkerPosition, TimeSpan.FromSeconds(1), null);
      ani.Completed += (s,_)=>GlobalCompleted(ani, target);
      ani.Begin();
    }
    // not used
    public void AniProgressCards(FrameworkElement target)
    {
      var ani = Storyboards.DoNothing(1000);
      ani.Completed += (s, _)=>GlobalCompleted(ani, target);
      ani.Begin();
    }
    public void AniResourceUpdate(FrameworkElement target)
    {
      var ani = Storyboards.Scale(target, TimeSpan.FromSeconds(.3), new Point(1, 1), new Point(5, 2), null, true);
      ani.Completed += (s, _) => GlobalCompleted(ani, target);
      ani.Begin();
    }
    public void AniPlayerAction(FrameworkElement target) { }//?!?brauch nicht
















  }
}
