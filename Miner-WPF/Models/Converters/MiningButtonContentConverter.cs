using System;
using System.Globalization;
using System.Windows.Data;

namespace Miner_WPF.Models.Converters
{
    [ValueConversion(typeof(bool), typeof(object))]
    class MiningButtonContentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                return "结束挖矿";
            }
            else
            {
                return "开始挖矿";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
