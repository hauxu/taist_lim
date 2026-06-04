using System;
using System.Windows.Data;
using Core.Models.DurationLimit;
namespace UI.Converters
{
    public class LockActionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is LockAction action)
            {
                switch (action)
                {
                    case LockAction.LockScreen: return "锁屏提醒";
                    case LockAction.CloseProcess: return "关闭进程";
                    case LockAction.BlockLaunch: return "今日禁止启动";
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