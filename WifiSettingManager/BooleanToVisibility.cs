using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace WifiSettingManager
{
    public static class BooleanToVisibility
    {
        public static IValueConverter HiddenWhenFalse { get; private set; } = new VisibilityConverter
        {
            Visible = true,
            Hidden = false
        };

        public static IValueConverter CollapsedWhenFalse { get; private set; } = new VisibilityConverter
        {
            Visible = true,
            Collapsed = false
        };

        public static IValueConverter HiddenWhenTrue { get; private set; } = new VisibilityConverter
        {
            Visible = false,
            Hidden = true
        };

        public static IValueConverter CollapsedWhenTrue { get; private set; } = new VisibilityConverter
        {
            Visible = false,
            Collapsed = true
        };

        public static IValueConverter VisibleWhenNull { get; private set; } = new VisibilityConverter
        {
            DefaultVisibility = Visibility.Collapsed,
            Visible = null
        };
        public static IValueConverter CollapsedWhenNull { get; private set; } = new VisibilityConverter
        {
            DefaultVisibility = Visibility.Visible,
            Collapsed = null
        };
    }
    public class VisibilityConverter : IValueConverter
    {
        /// <summary>
        /// 构造一个 <see cref="T:System.Object" /> to <see cref="T:System.Windows.Visibility" /> 的通用转换器对象。
        /// </summary>
        public VisibilityConverter()
        {
            this.DefaultVisibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 获取或设置 <see cref="F:System.Windows.Visibility.Visible" /> 所对应的值。
        /// </summary>
        public object Visible { get; set; }

        /// <summary>
        /// 获取或设置 <see cref="F:System.Windows.Visibility.Hidden" /> 所对应的值。
        /// </summary>
        public object Hidden { get; set; }

        /// <summary>
        /// 获取或设置 <see cref="F:System.Windows.Visibility.Collapsed" /> 所对应的值。
        /// </summary>
        public object Collapsed { get; set; }

        /// <summary>
        /// 设置默认的 <see cref="T:System.Windows.Visibility" /> 值。
        /// </summary>
        public Visibility DefaultVisibility { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (object.Equals(value, this.Visible))
                return (object)Visibility.Visible;
            if (object.Equals(value, this.Hidden))
                return (object)Visibility.Hidden;
            if (object.Equals(value, this.Collapsed))
                return (object)Visibility.Collapsed;
            return (object)this.DefaultVisibility;
        }

        /// <exception cref="T:System.ArgumentNullException">传入参数为null时触发</exception>
        /// <exception cref="T:System.InvalidCastException">传入参数为非<see cref="T:System.Windows.Visibility" />类型和非整数时触发</exception>
        /// <exception cref="T:System.ArgumentException">传入参数为0，1，2的整数时</exception>
        public object ConvertBack(
          object value,
          Type targetType,
          object parameter,
          CultureInfo culture)
        {
            if (value == null)
                throw new ArgumentNullException();
            switch ((Visibility)value)
            {
                case Visibility.Visible:
                    return this.Visible;
                case Visibility.Hidden:
                    return this.Hidden;
                case Visibility.Collapsed:
                    return this.Collapsed;
                default:
                    throw new ArgumentException(string.Format("不支持指定值 {0} 的转换。", value));
            }
        }
    }
}
