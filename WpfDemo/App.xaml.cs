using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace WpfDemo
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public MainWindow mainWindow { get; private set; }
        public DiscoverSensorsWindow discoverSensorsWindow { get; private set; }

        public ObservableCollection<IBLEAccelerationSensor> Sensors
        {
            get { return bleSensors; }
        }
        private ObservableCollection<IBLEAccelerationSensor> bleSensors;

        private Dictionary<string, IBLEAccelerationSensor> bleSensorsDict;

        private SFConfigStore configStore;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            configStore = new SFConfigStore();

            // init sensors
            bleSensorsDict = new Dictionary<string, IBLEAccelerationSensor>();
            bleSensors = new ObservableCollection<IBLEAccelerationSensor>();
            foreach (BLEGravitySensorsConfig sensorRecord in configStore.settings.sensors)
            {
                var sensor = new Sensors.TheSensor(sensorRecord.Id, sensorRecord.Name);
                sensor.Baseline = sensorRecord.Baseline;
                sensor.Normal = sensorRecord.Normal;
                
                bleSensorsDict[sensorRecord.Id] = sensor;
                bleSensors.Add(sensor);
            }
            Sensors.CollectionChanged += sensorCollectionChanged;

            // init windows
            // mainWindow = new MainWindow(this);
            //discoverSensorsWindow = new DiscoverSensorsWindow(this);

            // mainWindow.Show();

            HomeWindow homeWindow = new HomeWindow();

            homeWindow.Show();

        }
        public IBLEAccelerationSensor GetSensorById(string deviceId)
        {
            return bleSensorsDict[deviceId];
        }

        private void sensorCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                configStore.settings.sensors.RemoveRange(e.OldStartingIndex, e.OldItems.Count);
            }
            else if (e.Action == NotifyCollectionChangedAction.Add)
            {
                List<BLEGravitySensorsConfig> newSensorConfigs = new List<BLEGravitySensorsConfig>();
                foreach (var item in e.NewItems)
                {
                    var sensor = item as Sensors.TheSensor;
                    var sensorConfig = new BLEGravitySensorsConfig()
                    {
                        Name = sensor.DeviceName,
                        Id = sensor.DeviceId,
                        Baseline = sensor.Baseline,
                        Normal = sensor.Normal,
                    };
                    newSensorConfigs.Add(sensorConfig);
                }
                configStore.settings.sensors.InsertRange(e.NewStartingIndex, newSensorConfigs);

            }
            else
            {
                throw new NotImplementedException();
            }
            //System.Diagnostics.Debugger.Break();

        }
        public void SaveSettings()
        {
            configStore.Save();
        }
    }
}
