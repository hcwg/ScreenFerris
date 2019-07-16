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
        }

        private void BtnQuit_Click(object sender, RoutedEventArgs e)
        {
            app.Shutdown();
        }
    }
}
