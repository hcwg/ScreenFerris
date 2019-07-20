using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfDemo.UserControls
{
    /// <summary>
    /// Interaction logic for SensorControl.xaml
    /// </summary>
    public partial class SensorControl : UserControl
    {
        IBLEAccelerationSensor sensor;
        public SensorControl(IBLEAccelerationSensor sensor)
        {
            InitializeComponent();
            this.sensor = sensor;
            grid.DataContext = sensor;
        }
    }

    public class Vector3ToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
           if (!(value is Vector3)) { return ""; }
            Vector3? v = value as Vector3?;
            if (v.HasValue) {
                return String.Format("x = {0}, y = {0}, z = {0}", v.Value.X, v.Value.Y, v.Value.Z);
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
