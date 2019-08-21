namespace WpfDemo
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Windows;
    using WpfDemo.Sensors;

    /// <summary>
    /// Interaction logic for App.xaml.
    /// </summary>
    public partial class App : Application, INotifyPropertyChanged
    {
        private Dictionary<string, IBLEAccelerationSensor> bleSensorsDict;

        private SFConfigStore configStore;

        private System.Windows.Forms.NotifyIcon notifyIcon;
        private HomeWindow homeWindow;

        public App()
        {
            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<IBLEAccelerationSensor> Sensors { get; private set; }

        public bool ConfigModified { get => this.configStore.Modified; }

        public IBLEAccelerationSensor GetSensorById(string deviceId)
        {
            return this.bleSensorsDict[deviceId];
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

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            this.configStore = new SFConfigStore();
            this.configStore.PropertyChanged += this.ConfigStorePropertyChanged;

            // init sensors
            this.bleSensorsDict = new Dictionary<string, IBLEAccelerationSensor>();
            this.Sensors = new ObservableCollection<IBLEAccelerationSensor>();
            foreach (BLEGravitySensorConfig sensorConfig in this.configStore.Settings.sensors)
            {
                var sensor = SensorFactory.GetNewSensor(sensorConfig.Id, sensorConfig.Name, sensorConfig.ModelName);
                sensor.Baseline = sensorConfig.Baseline;
                sensor.Normal = sensorConfig.Normal;
                sensor.MACAddress = sensorConfig.MACAddress;
                sensor.AutoConnect = sensorConfig.AutoConnect;
                sensor.Binding.MonitorDeviceName = sensorConfig.BindedMonitor;
                sensor.PropertyChanged += (object s, PropertyChangedEventArgs ev) =>
                {
                    this.SyncSensorProperty(sensor, sensorConfig, ev.PropertyName);
                };

                this.bleSensorsDict[sensorConfig.Id] = sensor;
                this.Sensors.Add(sensor);
            }

            this.Sensors.CollectionChanged += this.SensorCollectionChanged;

            // init tray icon
            this.notifyIcon = new System.Windows.Forms.NotifyIcon
            {
                Icon = WpfDemo.Properties.Resources.ScreenFerrisIcon,
                Visible = true,
            };
            this.notifyIcon.DoubleClick += (sender, args) =>
            {
                this.ShowHomeWindow();
            };
            this.notifyIcon.ContextMenu = this.GetContextMenu();

            // init windows
            this.ShowHomeWindow();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            this.notifyIcon.Visible = false;
        }

        private void SensorCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in e.OldItems)
                {
                    this.bleSensorsDict.Remove((item as IBLEAccelerationSensor).DeviceId);
                }

                this.configStore.Settings.sensors.RemoveRange(e.OldStartingIndex, e.OldItems.Count);
            }
            else if (e.Action == NotifyCollectionChangedAction.Add)
            {
                List<BLEGravitySensorConfig> newSensorConfigs = new List<BLEGravitySensorConfig>();
                foreach (var item in e.NewItems)
                {
                    var sensor = item as IBLEAccelerationSensor;
                    this.bleSensorsDict[sensor.DeviceId] = sensor;
                    var sensorConfig = new BLEGravitySensorConfig()
                    {
                        Name = sensor.DeviceName,
                        Id = sensor.DeviceId,
                        Baseline = sensor.Baseline,
                        Normal = sensor.Normal,
                        MACAddress = sensor.MACAddress,
                        ModelName = sensor.ModelName,
                    };
                    newSensorConfigs.Add(sensorConfig);
                    sensor.PropertyChanged += (object s, PropertyChangedEventArgs ev) =>
                    {
                        this.SyncSensorProperty(sensor, sensorConfig, ev.PropertyName);
                    };
                }

                this.configStore.Settings.sensors.InsertRange(e.NewStartingIndex, newSensorConfigs);
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

        private System.Windows.Forms.ContextMenu GetContextMenu()
        {
            System.Windows.Forms.ContextMenu menu = new System.Windows.Forms.ContextMenu();
            var itemExit = new System.Windows.Forms.MenuItem("Exit", (object sender, EventArgs e) => { this.Shutdown(); });
            menu.MenuItems.Add(itemExit);
            return menu;
        }
    }
}
