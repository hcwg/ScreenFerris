using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
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
using WpfDemo.UserControls;

namespace WpfDemo
{

    /// <summary>
    /// Interaction logic for SensorsPage.xaml
    /// </summary>
    public partial class SensorsPage : Page
    {
        private App app;

        public ObservableCollection<IBLEAccelerationSensor> Sensors;
        public SensorsPage(App app)
        {
            this.app = app;
            Sensors = app.Sensors;

            InitializeComponent();

            sensorsListView.ItemsSource = Sensors;
        }

   

        private void BtnRemove_Click(object sender, RoutedEventArgs e)
        {
            int newIndex = Math.Min(Sensors.Count - 2, sensorsListView.SelectedIndex);
            Sensors.Remove(sensorsListView.SelectedItem as IBLEAccelerationSensor);
            if (newIndex >= 0)
            {
                Dispatcher.Invoke(() => { sensorsListView.SelectedIndex = newIndex; });
            }
        }


        private void SensorsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) { return; }
            var sensor = e.AddedItems[0] as IBLEAccelerationSensor;
            SensorControl sensorControl = new SensorControl(sensor);
            rightContent.Content = sensorControl;
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
