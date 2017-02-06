using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace ations
{
  public class NonEmptyToVisibleConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is ObservableCollection<Choice> && (value as ObservableCollection<Choice>).Count > 0
        ||
        value is ObservableCollection<Res> && (value as ObservableCollection<Res>).Count > 0)
        return Visibility.Visible;
      else return Visibility.Collapsed;
      //var specialOptions = value as ObservableCollection<Choice>;
      //var reslist = value as ObservableCollection<Res>;
      //return specialOptions != null && specialOptions.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
