namespace WpfDemo
{
    using System;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using WpfDemo.UserControls;

    /// <summary>
    /// Interaction logic for SensorsPage.xaml.
    /// </summary>
    public partial class SensorsPage : Page
    {
        public ObservableCollection<IBLEAccelerationSensor> Sensors;

        private App app;

        public SensorsPage(App app)
        {
            this.app = app;
            this.Sensors = app.Sensors;

            this.InitializeComponent();

            this.sensorsListView.ItemsSource = this.Sensors;
            if (this.Sensors.Count > 0)
            {
                this.sensorsListView.SelectedIndex = 0;
            }
        }

        private void BtnRemove_Click(object sender, RoutedEventArgs e)
        {
            int newIndex = Math.Min(this.Sensors.Count - 2, this.sensorsListView.SelectedIndex);
            this.Sensors.Remove(this.sensorsListView.SelectedItem as IBLEAccelerationSensor);
            if (newIndex >= 0)
            {
                this.Dispatcher.Invoke(() => { this.sensorsListView.SelectedIndex = newIndex; });
            }
        }

        private void SensorsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) { return; }
            var sensor = e.AddedItems[0] as IBLEAccelerationSensor;
            SensorControl sensorControl = new SensorControl(sensor);
            this.rightContent.Content = sensorControl;
        }

    }

    public class SelectedToActiveConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
