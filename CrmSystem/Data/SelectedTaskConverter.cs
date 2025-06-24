using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CrmSystem.Data
{
    public class SelectedTaskConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var selected = value != null && (bool)value;
            return selected ? new SolidColorBrush((Color)ColorConverter.ConvertFromString(parameter.ToString())) : new SolidColorBrush(Colors.White);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
