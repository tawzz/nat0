using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace ations
{
  public class ItemContainerToZIndexConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      var itemContainer = (DependencyObject)value;
      var itemsControl = itemContainer.FindAncestor<ItemsControl>();
      int index = itemsControl.ItemContainerGenerator.IndexFromContainer(itemContainer);
      return -index;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotSupportedException();
    }

  }

}
