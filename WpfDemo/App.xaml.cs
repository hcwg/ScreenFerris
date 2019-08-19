namespace WpfDemo
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Windows;

    /// <summary>
    /// Interaction logic for App.xaml.
    /// </summary>
    public partial class App : Application, INotifyPropertyChanged
    {

        public SensorsPage mainWindow { get; private set; }

        public AddSensorPage discoverSensorsWindow { get; private set; }

        public bool ConfigModified { get => this.configStore.Modified; }

        public ObservableCollection<IBLEAccelerationSensor> Sensors
        {
            get { return this.bleSensors; }
        }

        private ObservableCollection<IBLEAccelerationSensor> bleSensors;

        private Dictionary<string, IBLEAccelerationSensor> bleSensorsDict;

        private SFConfigStore configStore;

        private System.Windows.Forms.NotifyIcon notifyIcon;
        private HomeWindow homeWindow;

        public event PropertyChangedEventHandler PropertyChanged;

        public App()
        {
            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            this.configStore = new SFConfigStore();
            this.configStore.PropertyChanged += this.ConfigStorePropertyChanged;

            // init sensors
            this.bleSensorsDict = new Dictionary<string, IBLEAccelerationSensor>();
            this.bleSensors = new ObservableCollection<IBLEAccelerationSensor>();
            foreach (BLEGravitySensorConfig sensorConfig in this.configStore.settings.sensors)
            {
                var sensor = new Sensors.TheSensor(sensorConfig.Id, sensorConfig.Name)
                {
                    Baseline = sensorConfig.Baseline,
                    Normal = sensorConfig.Normal,
                    MACAddress = sensorConfig.MACAddress,
                };
                sensor.AutoConnect = sensorConfig.AutoConnect;
                sensor.Binding.MonitorDeviceName = sensorConfig.BindedMonitor;
                sensor.PropertyChanged += (object s, PropertyChangedEventArgs ev) =>
                {
                    this.SyncSensorProperty(sensor, sensorConfig, ev.PropertyName);
                };

                this.bleSensorsDict[sensorConfig.Id] = sensor;
                this.bleSensors.Add(sensor);
            }

            this.Sensors.CollectionChanged += this.sensorCollectionChanged;

            // init tray icon
            this.notifyIcon = new System.Windows.Forms.NotifyIcon();
            this.notifyIcon.Icon = WpfDemo.Properties.Resources.ScreenFerrisIcon;
            this.notifyIcon.Visible = true;
            this.notifyIcon.DoubleClick += (sender, args) =>
            {
                this.ShowHomeWindow();
            };
            this.notifyIcon.ContextMenu = this.GetContextMenu();

            // init windows
            HomeWindow homeWindow = new HomeWindow(this);
            homeWindow.Show();

        }

        protected override void OnExit(ExitEventArgs e)
        {
            this.notifyIcon.Visible = false;
        }

        public IBLEAccelerationSensor GetSensorById(string deviceId)
        {
            return this.bleSensorsDict[deviceId];
        }

        private void sensorCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in e.OldItems)
                {
                    this.bleSensorsDict.Remove((item as IBLEAccelerationSensor).DeviceId);
                }

                this.configStore.settings.sensors.RemoveRange(e.OldStartingIndex, e.OldItems.Count);
            }
            else if (e.Action == NotifyCollectionChangedAction.Add)
            {
                List<BLEGravitySensorConfig> newSensorConfigs = new List<BLEGravitySensorConfig>();
                foreach (var item in e.NewItems)
                {
                    var sensor = item as Sensors.TheSensor;
                    this.bleSensorsDict[sensor.DeviceId] = sensor;
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
                        this.SyncSensorProperty(sensor, sensorConfig, ev.PropertyName);
                    };

                }

                this.configStore.settings.sensors.InsertRange(e.NewStartingIndex, newSensorConfigs);

            }
            else
            {
                throw new NotImplementedException();
            }

            this.configStore.Modified = true;

        }

        private void SensorPropertyChangedEventHandler(object sender, PropertyChangedEventArgs e)
        {

        }

        private void ConfigStorePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Modified")
            {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ConfigModified"));
            }
        }

        private void SyncSensorProperty(IBLEAccelerationSensor sensor, BLEGravitySensorConfig sensorConfig, string propertyName)
        {
            bool isConfigProperty = true;
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

                case "AutoConnect":
                    {
                        sensorConfig.AutoConnect = sensor.AutoConnect;
                        break;
                    }

                case "BindMonitor":
                    {
                        sensorConfig.BindedMonitor = sensor.Binding.MonitorDeviceName;
                        break;
                    }

                default:
                    {
                        isConfigProperty = false;
                        break;
                    }

            }

            if (isConfigProperty)
            {
                this.configStore.Modified = true;
            }
        }

        public void SaveSettings()
        {
            this.configStore.Save();
        }

        public void ShowHomeWindow()
        {
            if (this.homeWindow == null)
            {
                this.homeWindow = new HomeWindow(this);
            }

            this.homeWindow.Show();
        }

        public void SetHomeWindowNull()
        {
            this.homeWindow = null;
        }

        private System.Windows.Forms.ContextMenu GetContextMenu()
        {
            System.Windows.Forms.ContextMenu menu = new System.Windows.Forms.ContextMenu();
            var itemExit = new System.Windows.Forms.MenuItem("Exit", (object sender, EventArgs e) => { this.Shutdown(); });
            menu.MenuItems.Add(itemExit);
            return menu;
        }
    }
}
