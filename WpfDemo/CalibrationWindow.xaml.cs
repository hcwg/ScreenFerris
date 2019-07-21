using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace WpfDemo
{
    public enum CaliberationStep
    {
        FixLandscape,
        Fix90Degree,
        Finish
    }
    /// <summary>
    /// Interaction logic for CalibrationWindow.xaml
    /// </summary>
    public partial class CalibrationWindow : Window
    {
        private Sensors.TheSensor sensor;

        CaliberationStep step = CaliberationStep.FixLandscape;
        public CalibrationWindow(IBLEAccelerationSensor sensor)
        {
            InitializeComponent();
            this.sensor = sensor as Sensors.TheSensor;
            groupBoxCalibrationInfo.DataContext = sensor;
        }

        private void ButtonConfirm_Click(object sender, RoutedEventArgs e)
        {
            switch (step)
            {
                case CaliberationStep.FixLandscape:
                    {
                        sensor.Baseline = sensor.Acceleration;
                        textBlockButton.Text = "Step2: Rotate your monitor 90 degree clockwise and press this button.";
                        step = CaliberationStep.Fix90Degree;
                        break;
                    }
                case CaliberationStep.Fix90Degree:
                    {
                        sensor.Normal = Vector3.Cross(sensor.Baseline.Value, sensor.Acceleration.Value);
                        textBlockButton.Text = "Finish";
                        step = CaliberationStep.Finish;
                        break;
                    }
                case CaliberationStep.Finish:
                    {
                        this.Close();
                        break;
                    }

                default: break;


            }
        }
    }
}
