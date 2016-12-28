using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace ations
{
  public class NumberToStringConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      var twoFormatString = ((string)parameter).Split('/');
      var num = (int)value;
      var format = twoFormatString.Length > 1 && num < 0? twoFormatString[1]:twoFormatString[0];
      return string.Format(format, num>=0?num:-num);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
