using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Windows.Phone.ApplicationModel;

namespace WpfDemo
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        public SensorsPage mainWindow { get; private set; }
        public AddSensorPage discoverSensorsWindow { get; private set; }

        public ObservableCollection<IBLEAccelerationSensor> Sensors
        {
            get { return bleSensors; }
        }
        private ObservableCollection<IBLEAccelerationSensor> bleSensors;

        private Dictionary<string, IBLEAccelerationSensor> bleSensorsDict;

        private SFConfigStore configStore;

        private System.Windows.Forms.NotifyIcon notifyIcon;
        private HomeWindow homeWindow;

        public App()
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            configStore = new SFConfigStore();

            // init sensors
            bleSensorsDict = new Dictionary<string, IBLEAccelerationSensor>();
            bleSensors = new ObservableCollection<IBLEAccelerationSensor>();
            foreach (BLEGravitySensorConfig sensorConfig in configStore.settings.sensors)
            {
                var sensor = new Sensors.TheSensor(sensorConfig.Id, sensorConfig.Name)
                {
                    Baseline = sensorConfig.Baseline,
                    Normal = sensorConfig.Normal,
                    MACAddress = sensorConfig.MACAddress,
                };
                sensor.PropertyChanged += (object s, PropertyChangedEventArgs ev) =>
                {
                    SyncSensorProperty(sensor, sensorConfig, ev.PropertyName);
                };


                bleSensorsDict[sensorConfig.Id] = sensor;
                bleSensors.Add(sensor);
            }
            Sensors.CollectionChanged += sensorCollectionChanged;
            //init tray icon

            notifyIcon = new System.Windows.Forms.NotifyIcon();
            notifyIcon.Icon = WpfDemo.Properties.Resources.ScreenFerrisIcon;
            notifyIcon.Visible = true;
            notifyIcon.DoubleClick += delegate (object sender, EventArgs args)
                {
                    ShowHomeWindow();
                };
            notifyIcon.ContextMenu = GetContextMenu();

            // init windows
            HomeWindow homeWindow = new HomeWindow(this);
            homeWindow.Show();

        }
        protected override void OnExit(ExitEventArgs e)
        {
            notifyIcon.Visible = false;
        }

        public IBLEAccelerationSensor GetSensorById(string deviceId)
        {
            return bleSensorsDict[deviceId];
        }

        private void sensorCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in e.OldItems)
                {
                    bleSensorsDict.Remove((item as IBLEAccelerationSensor).DeviceId);
                }
                configStore.settings.sensors.RemoveRange(e.OldStartingIndex, e.OldItems.Count);
            }
            else if (e.Action == NotifyCollectionChangedAction.Add)
            {
                List<BLEGravitySensorConfig> newSensorConfigs = new List<BLEGravitySensorConfig>();
                foreach (var item in e.NewItems)
                {
                    var sensor = item as Sensors.TheSensor;
                    bleSensorsDict[sensor.DeviceId] = sensor;
                    var sensorConfig = new BLEGravitySensorConfig()
                    {
                        Name = sensor.DeviceName,
                        Id = sensor.DeviceId,
                        Baseline = sensor.Baseline,
                        Normal = sensor.Normal,
                        MACAddress = sensor.MACAddress
                    };
                    newSensorConfigs.Add(sensorConfig);
                    sensor.PropertyChanged += (object s, PropertyChangedEventArgs ev) =>
                    {
                        SyncSensorProperty(sensor, sensorConfig, ev.PropertyName);
                    };

                }
                configStore.settings.sensors.InsertRange(e.NewStartingIndex, newSensorConfigs);

            }
            else
            {
                throw new NotImplementedException();
            }
            //System.Diagnostics.Debugger.Break();

        }
        private void SensorPropertyChangedEventHandler(object sender, PropertyChangedEventArgs e)
        {
        }
        private void SyncSensorProperty(IBLEAccelerationSensor sensor, BLEGravitySensorConfig sensorConfig, string propertyName)
        {
            switch (propertyName)
            {
                case "DeviceName":
                    {
                        sensorConfig.Name = sensor.DeviceName;
                        break;
                    }
                case "Baseline":
                    {
                        sensorConfig.Baseline = sensor.Baseline;
                        break;
                    }
                case "Normal":
                    {
                        sensorConfig.Normal = sensor.Normal;
                        break;
                    }
            }
        }
        public void SaveSettings()
        {
            configStore.Save();
        }
        public void ShowHomeWindow()
        {
            if (homeWindow == null)
            {
                homeWindow = new HomeWindow(this);
            }
            homeWindow.Show();
        }
        public void SetHomeWindowNull()
        {
            homeWindow = null;
        }
        private System.Windows.Forms.ContextMenu GetContextMenu()
        {
            System.Windows.Forms.ContextMenu menu = new System.Windows.Forms.ContextMenu();
            var itemExit = new System.Windows.Forms.MenuItem("Exit", (object sender, EventArgs e) => { Shutdown(); });
            menu.MenuItems.Add(itemExit);
            return menu;
        }
    }
}
