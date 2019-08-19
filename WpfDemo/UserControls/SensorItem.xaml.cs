namespace WpfDemo.UserControls
{
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for SensorItem.xaml.
    /// </summary>
    public partial class SensorItem : UserControl
    {
        public static DependencyProperty SensorProperty = DependencyProperty.Register("Sensor", typeof(IBLEAccelerationSensor), typeof(SensorItem));

        public SensorItem()
        {
            this.InitializeComponent();
        }
    }
}
