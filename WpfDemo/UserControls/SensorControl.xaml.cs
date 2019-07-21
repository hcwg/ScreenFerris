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

        private void ButtonCalibrate_Click(object sender, RoutedEventArgs e)
        {
            CalibrationWindow calibrationWindow = new CalibrationWindow(sensor);
            calibrationWindow.Show();
        }
    }

   
}
