using System;
using System.Globalization;
using System.Windows.Data;

namespace Miner_WPF.Models.Converters
{
    [ValueConversion(typeof(bool), typeof(bool?))]
    public class IsParamInputEnabledConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool isAlive = (bool)values[0];
            bool automatic = (bool)values[1];
            return !isAlive && !automatic;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
