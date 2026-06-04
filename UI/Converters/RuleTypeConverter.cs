using System;
using System.Windows.Data;
using Core.Models.DurationLimit;
namespace UI.Converters
{
    public class RuleTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is RuleType type)
            {
                switch (type)
                {
                    case RuleType.Software: return "软件";
                    case RuleType.Website: return "网站";
                    case RuleType.Browser: return "浏览器";
                    default: return "未知";
                }
            }
            return value;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}