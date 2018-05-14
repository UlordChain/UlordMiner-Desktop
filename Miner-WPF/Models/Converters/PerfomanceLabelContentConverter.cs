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
            return Binary.GetBinaryString((double)value,1);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
