namespace WpfDemo
{
    using System.Numerics;
    using System.Windows;

    public enum CaliberationStep
    {
        FixLandscape,
        Fix90Degree,
        Finish
    }

    /// <summary>
    /// Interaction logic for CalibrationWindow.xaml.
    /// </summary>
    public partial class CalibrationWindow : Window
    {
        private IBLEAccelerationSensor sensor;

        private CaliberationStep step = CaliberationStep.FixLandscape;

        public CalibrationWindow(IBLEAccelerationSensor sensor)
        {
            this.InitializeComponent();
            this.sensor = sensor;
            this.groupBoxCalibrationInfo.DataContext = sensor;
        }

        private void ButtonConfirm_Click(object sender, RoutedEventArgs e)
        {
            switch (this.step)
            {
                case CaliberationStep.FixLandscape:
                    {
                        this.sensor.Baseline = this.sensor.Acceleration;
                        this.textBlockButton.Text = "Step2: Rotate your monitor 90 degree clockwise and press this button.";
                        this.step = CaliberationStep.Fix90Degree;
                        break;
                    }

                case CaliberationStep.Fix90Degree:
                    {
                        this.sensor.Normal = Vector3.Cross(this.sensor.Baseline.Value, this.sensor.Acceleration.Value);
                        this.textBlockButton.Text = "Finish";
                        this.step = CaliberationStep.Finish;
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
