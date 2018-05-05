using Microsoft.Expression.Media;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Miner_WPF.Models.Converters
{
    [ValueConversion(typeof(bool), typeof(ArrowOrientation))]
    public class PerformanceSignOrientationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? ArrowOrientation.Up : ArrowOrientation.Down;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
