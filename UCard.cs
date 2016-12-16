using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ations
{
  public class UCard : Control
  {
    public BitmapImage Image { get { return (BitmapImage)GetValue(ImageProperty); } set { SetValue(ImageProperty, value); } }
    public static readonly DependencyProperty ImageProperty = DependencyProperty.Register("Image", typeof(BitmapImage), typeof(UCard), null);

    static UCard()
    {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(UCard), new FrameworkPropertyMetadata(typeof(UCard)));
    }
  }
}
