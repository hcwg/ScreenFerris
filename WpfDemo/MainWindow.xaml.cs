using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using WpfDemo.Models;

namespace WpfDemo
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private App app;
        private SFConfigStore configStore;
        public IBLEAccelerationSensor theSensor = new Sensors.TheSensor("BluetoothLE#BluetoothLE00:28:f8:fd:94:99-df:63:ad:c1:08:97", "The sensor");

        private ObservableCollection<SensorDisplay> SensorsCollection { get; set; }
        public MainWindow(App app, SFConfigStore configStore)
        {
            this.configStore = configStore;
            this.app = app;


            SensorsCollection = new ObservableCollection<SensorDisplay>();

            InitializeComponent();

            sensorsListView.ItemsSource = SensorsCollection;

            foreach (BLEGravitySensors sensor in configStore.settings.sensors)
            {
                SensorsCollection.Add(new SensorDisplay() {
                    DeviceID = sensor.Id,
                    DeviceName = sensor.Name,
                });

            }
            theSensor.AutoConnect = true;
            theSensor.PropertyChanged += (sender, e) =>
            {
                if (theSensor.Acceleration.HasValue)
                {
                    Dispatcher.Invoke(() =>
                    {
                        label.Content = theSensor.Acceleration.ToString();
                    });
                }
                
               
            };
        }

        private void BtnQuit_Click(object sender, RoutedEventArgs e)
        {
            app.Shutdown();
        }
    }
}
