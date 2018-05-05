using System;
using System.Globalization;
using System.Windows.Data;

namespace Miner_WPF.Models.Converters
{
    [ValueConversion(typeof(double), typeof(object))]
    class PerfomanceLabelContentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int digits = 1;
            if (parameter != null)
            {
                digits = System.Convert.ToInt32(parameter);
            }
            return Binary.GetBinaryString((double)value,digits);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
