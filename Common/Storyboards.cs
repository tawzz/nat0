using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Windows.Foundation;

namespace ations
{
  public static class Storyboards
  {
    static EasingFunctionBase defaultEasing = new SineEase { EasingMode = EasingMode.EaseOut };
    public static Storyboard Scale(this FrameworkElement target, TimeSpan duration, Point from, Point to, EasingFunctionBase easing = null, bool autoreverse = false, int repeat = 1)
    {
      easing = easing ?? defaultEasing;
      var storyboard = new Storyboard();
      target.RenderTransformOrigin = new Point(.5, .5);
      target.RenderTransform = new ScaleTransform();
      storyboard.Children.Add(GetDoubleAnimation(target, "(UIElement.RenderTransform).(ScaleTransform.ScaleX)",
        from.X, to.X, duration, easing, autoreverse, repeat));
      storyboard.Children.Add(GetDoubleAnimation(target, "(UIElement.RenderTransform).(ScaleTransform.ScaleY)",
        from.Y, to.Y, duration, easing, autoreverse, repeat));
      return storyboard;
    }
    public static Storyboard MoveTo(this FrameworkElement target, Point to, TimeSpan duration, EasingFunctionBase easing)
    {
      easing = easing ?? defaultEasing;
      var storyboard = new Storyboard();
      target.RenderTransformOrigin = new Point(.5, .5);
      target.RenderTransform = new TranslateTransform();
      storyboard.Children.Add(GetDoubleAnimation(target, "(UIElement.RenderTransform).(TranslateTransform.X)",
        to.X, duration, easing, false, 1));
      storyboard.Children.Add(GetDoubleAnimation(target, "(UIElement.RenderTransform).(TranslateTransform.Y)",
        to.Y, duration, easing, false, 1));
      return storyboard;
    }
    public static Storyboard DoNothing(double msecs)
    {
      var storyboard = new Storyboard();
      storyboard.Duration = TimeSpan.FromMilliseconds(msecs);
      return storyboard;
    }
    public static Storyboard Appear(this FrameworkElement target, TimeSpan duration, EasingFunctionBase easing = null)
    {
      easing = easing ?? defaultEasing;
      var storyboard = new Storyboard();
      target.Opacity = 0;
      storyboard.Children.Add(GetDoubleAnimation(target, "Opacity", 0, 1, duration, easing, false, 1));
      return storyboard;
    }




















    public static Storyboard Move(this FrameworkElement target, Point from, Point to, TimeSpan duration, EasingFunctionBase easing)
    {
      easing = easing ?? defaultEasing;
      var storyboard = new Storyboard();
      target.RenderTransformOrigin = new Point(.5, .5);
      target.RenderTransform = new TranslateTransform();
      storyboard.Children.Add(GetDoubleAnimation(target, "(UIElement.RenderTransform).(TranslateTransform.X)",
        from.X, to.X, duration, easing, false, 1));
      storyboard.Children.Add(GetDoubleAnimation(target, "(UIElement.RenderTransform).(TranslateTransform.Y)",
        from.Y, to.Y, duration, easing, false, 1));
      storyboard.Completed += OnCompletedDefault;
      return storyboard;
    }
    public static Storyboard Scale(this FrameworkElement target, Action OnCompleted, TimeSpan duration, Point from, Point to, EasingFunctionBase easing = null, bool autoreverse = false, int repeat = 1)
    {
      easing = easing ?? defaultEasing;
      var storyboard = new Storyboard();
      target.RenderTransformOrigin = new Point(.5, .5);
      target.RenderTransform = new ScaleTransform();
      storyboard.Children.Add(GetDoubleAnimation(target, "(UIElement.RenderTransform).(ScaleTransform.ScaleX)",
        from.X, to.X, duration, easing, autoreverse, repeat));
      storyboard.Children.Add(GetDoubleAnimation(target, "(UIElement.RenderTransform).(ScaleTransform.ScaleY)",
        from.Y, to.Y, duration, easing, autoreverse, repeat));
      storyboard.Completed += new EventHandler((obj, eventargs) => { OnCompleted?.Invoke(); });
      return storyboard;
    }
    public static Storyboard Rotate(this FrameworkElement target, Action OnCompleted, TimeSpan duration, double from = 0, double to = 360, EasingFunctionBase easing = null, bool autoreverse = false, int repeat = 1)
    {
      easing = easing ?? defaultEasing;
      var storyboard = new Storyboard();
      target.RenderTransformOrigin = new Point(.5, .5);
      target.RenderTransform = new RotateTransform();
      storyboard.Children.Add(GetDoubleAnimation(target, "(UIElement.RenderTransform).(RotateTransform.Angle)",
        from, to, duration, easing, autoreverse, repeat));
      storyboard.Completed += new EventHandler((obj, eventargs) => { OnCompleted?.Invoke(); });
      return storyboard;
    }

    #region dice storyboard
    public static Storyboard Dice(this FrameworkElement frame, FrameworkElement one, FrameworkElement two,
      FrameworkElement three, FrameworkElement four, FrameworkElement five, FrameworkElement six)
    //this FrameworkElement target, double from, double to, TimeSpan duration, EasingFunctionBase easing, bool autoreverse, int repeat)
    {
      var storyboard = new Storyboard();

      //frame.RenderTransformOrigin = new Point(.5, .5); // damit rotate around center!
      //frame.RenderTransform = new RotateTransform(); // { CenterX=.5, CenterY=.5 };
      //storyboard.Children.Add(GetDoubleAnimation(frame, "(UIElement.RenderTransform).(RotateTransform.Angle)",
      //  0, 360, TimeSpan.FromSeconds(.5), defaultEasing, false, 12));

      //make all faces transparent
      foreach (var fe in new FrameworkElement[] { one, two, three, four, five, six })
      {
        fe.Visibility = Visibility.Visible;
        fe.Opacity = 0;
      }

      var faces = new List<FrameworkElement>();
      var a = new List<FrameworkElement> { one, two, three, four, five, six };
      while (faces.Count < 4)
      {
        var r = Rand.N(0, a.Count - 1);
        faces.Add(a[r]);
        a.RemoveAt(r);
      }

      double i = 0.0;
      foreach (var f in faces)
      {
        AddAni(f, i, storyboard); i += .2;
      }
      AddAni(a.Last(), i, storyboard, false);

      storyboard.Completed += OnCompletedDefault;
      return storyboard;
    }
    #endregion

    static void AddAni(FrameworkElement fw, double beginsat, Storyboard sb, bool rev = true)
    {
      var ani = new DoubleAnimationUsingKeyFrames();
      ani.Duration = TimeSpan.FromSeconds(.5);
      var keyf = new DiscreteDoubleKeyFrame(); // LinearDoubleKeyFrame(); //new DiscreteDoubleKeyFrame();
      keyf.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(.4));
      keyf.Value = 1.0;
      ani.KeyFrames.Add(keyf);
      ani.BeginTime = TimeSpan.FromSeconds(beginsat);
      ani.AutoReverse = rev;
      Storyboard.SetTarget(ani, fw);
      Storyboard.SetTargetProperty(ani, new PropertyPath("Opacity"));
      sb.Children.Add(ani);

      //// Animate from 500 (the value of the previous key frame)  
      //// to 400 at 4 seconds using discrete interpolation. 
      //// Because the interpolation is discrete, the rectangle will appear 
      //// to "jump" from 500 to 400.
      //translationAnimation.KeyFrames.Add(
      //    new DiscreteDoubleKeyFrame(
      //        400, // Target value (KeyValue)
      //        KeyTime.FromTimeSpan(TimeSpan.FromSeconds(4))) // KeyTime
      //    );

    }

    public static DoubleAnimation GetDoubleAnimation(FrameworkElement target, string property, double to, TimeSpan duration, EasingFunctionBase easing, bool autoreverse, int repeat)
    {
      var ani = new DoubleAnimation
      {
        To = to,
        Duration = new Duration(duration),
        RepeatBehavior = new RepeatBehavior(repeat),// { Count = repeat },
        AutoReverse = autoreverse,
        EasingFunction = easing
      };
      Storyboard.SetTarget(ani, target);
      Storyboard.SetTargetProperty(ani, new PropertyPath(property));
      return ani;
    }
    public static DoubleAnimation GetDoubleAnimation(FrameworkElement target, string property, double from, double to, TimeSpan duration, EasingFunctionBase easing, bool autoreverse, int repeat)
    {
      var ani = new DoubleAnimation
      {
        From = from,
        To = to,
        Duration = new Duration(duration),
        RepeatBehavior = new RepeatBehavior(repeat),// { Count = repeat },
        AutoReverse = autoreverse,
        EasingFunction = easing
      };
      Storyboard.SetTarget(ani, target);
      Storyboard.SetTargetProperty(ani, new PropertyPath(property));
      return ani;
    }

    static void OnCompletedDefault(object sender, object e)
    {
      var s = sender as Storyboard;
      s.Completed -= OnCompletedDefault;
    }


  }
}
