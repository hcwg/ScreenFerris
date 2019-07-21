using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WpfDemo.ValueConverters
{
    public class Vector3ToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is Vector3)) { return "No value"; }
            Vector3? v = value as Vector3?;
            if (v.HasValue)
            {
                return String.Format("x = {0}, y = {0}, z = {0}", v.Value.X, v.Value.Y, v.Value.Z);
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class RadToDegConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) { return ""; }
            if (value is float)
            {
                float? rad = value as float?;
                return String.Format("{0:0.000}", rad.Value / 2 / Math.PI * 360);
            }
            else if (value is double)
            {
                double? rad = value as double?;
                return String.Format("{0:0.000}", rad.Value / 2 / Math.PI * 360);
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
