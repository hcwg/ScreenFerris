namespace WpfDemo
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;
    using Windows.Devices.Bluetooth;
    using Windows.Devices.Enumeration;
    using WpfDemo.Sensors;

    public class ScanDeviceInfo : INotifyPropertyChanged
    {
        public ScanDeviceInfo(DeviceInformation deviceInfoIn)
        {
            this.DeviceInformation = deviceInfoIn;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public DeviceInformation DeviceInformation { get; private set; }

        public string DeviceId => this.DeviceInformation.Id;

        public string DeviceName => this.DeviceInformation.Name;

        public string DeviceMAC => (string)this.DeviceInformation.Properties["System.Devices.Aep.DeviceAddress"];

        public void Update(DeviceInformationUpdate deviceInfoUpdate)
        {
            Debug.WriteLine(string.Format("Before update: {0}", this.DeviceInformation.Name));
            this.DeviceInformation.Update(deviceInfoUpdate);
            Debug.WriteLine(string.Format("After update: {0}", this.DeviceInformation.Name));

            this.OnPropertyChanged("Name");
            this.OnPropertyChanged("DeviceInformation");
        }

        protected void OnPropertyChanged(string name)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    /// <summary>
    /// Interaction logic for SensorsPage.xaml.
    /// </summary>
    public partial class AddSensorPage : Page
    {
        private DeviceWatcher deviceWatcher;
        private App app;
        private Window window;

        public AddSensorPage(App app, Window window)
        {
            this.app = app;
            this.window = window;
            this.DeviceCollection = new ObservableCollection<ScanDeviceInfo>();
            this.InitializeComponent();
        }

        public ObservableCollection<ScanDeviceInfo> DeviceCollection { get; set; }

        public void StartScan()
        {
            if (this.deviceWatcher == null)
            {
                string[] requestedProperties = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsConnected", "System.Devices.Aep.Bluetooth.Le.IsConnectable" };

                // BT_Code: Example showing paired and non-paired in a single query.
                string aqsAllBluetoothLEDevices = "(System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\")";

                BluetoothLEDevice.GetDeviceSelectorFromPairingState(false);

                this.deviceWatcher =
                        DeviceInformation.CreateWatcher(
                            aqsAllBluetoothLEDevices,
                            requestedProperties,
                            DeviceInformationKind.AssociationEndpoint);
                this.deviceWatcher.Added += this.DeviceWatcher_Added;
                this.deviceWatcher.Updated += this.DeviceWatcher_Updated;
                this.deviceWatcher.Removed += this.DeviceWatcher_Removed;
                this.deviceWatcher.EnumerationCompleted += this.DeviceWatcher_EnumerationCompleted;
                this.deviceWatcher.Stopped += this.DeviceWatcher_Stopped;
                this.deviceWatcher.Start();

            }
        }

        public void StopScan()
        {
            if (this.deviceWatcher != null)
            {
                // Unregister the event handlers.
                this.deviceWatcher.Added -= this.DeviceWatcher_Added;
                this.deviceWatcher.Updated -= this.DeviceWatcher_Updated;
                this.deviceWatcher.Removed -= this.DeviceWatcher_Removed;
                this.deviceWatcher.EnumerationCompleted -= this.DeviceWatcher_EnumerationCompleted;
                this.deviceWatcher.Stopped -= this.DeviceWatcher_Stopped;

                // Stop the watcher.
                this.deviceWatcher.Stop();
                this.deviceWatcher = null;
            }
        }

        private void DeviceWatcher_Added(DeviceWatcher sender, DeviceInformation deviceInfo)
        {
            Debug.WriteLine(string.Format("Added {0}{1}", deviceInfo.Id, deviceInfo.Name));
            if (sender != this.deviceWatcher) { return; }
            this.window.Dispatcher.Invoke(() =>
            {
                this.DeviceCollection.Add(new ScanDeviceInfo(deviceInfo));
            });
        }

        private void DeviceWatcher_Updated(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
        {
            if (sender != this.deviceWatcher) { return; }
            Debug.WriteLine(string.Format("Added {0}{1}", deviceInfoUpdate.Id, deviceInfoUpdate.Properties));

            ScanDeviceInfo dev = this.FindDeviceInfoById(deviceInfoUpdate.Id);
            if (dev == null) { return; }
            dev.Update(deviceInfoUpdate);
        }

        private void DeviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
        {
            string id = deviceInfoUpdate.Id;
            this.window.Dispatcher.Invoke(() =>
            {
                ScanDeviceInfo dev = this.FindDeviceInfoById(deviceInfoUpdate.Id);
                if (dev != null)
                {
                    this.DeviceCollection.Remove(dev);
                }

            });
        }

        private void DeviceWatcher_EnumerationCompleted(DeviceWatcher sender, object e)
        {
        }

        private void DeviceWatcher_Stopped(DeviceWatcher sender, object e)
        {
        }

        private ScanDeviceInfo FindDeviceInfoById(string id)
        {
            foreach (ScanDeviceInfo dev in this.DeviceCollection)
            {
                if (dev.DeviceId == id)
                {
                    return dev;
                }
            }

            return null;
        }

        private void btnStartScanClick(object sender, RoutedEventArgs e)
        {
            this.StartScan();
        }

        private async void btnAddClick(object sender, RoutedEventArgs e)
        {
            ScanDeviceInfo item = this.ScannedDevicesList.SelectedItem as ScanDeviceInfo;
            if (item == null)
            {
                return;
            }

            BluetoothLEDevice bluetoothLEDevice = await BluetoothLEDevice.FromIdAsync(item.DeviceId);
            string modelName = await SupportedSensors.GuessModelNameAsync(bluetoothLEDevice);
            bluetoothLEDevice?.Dispose();
            if (modelName == string.Empty)
            {
                this.txtblockMessage.Text = "Can't determinate Sensor Model.";
            }

            var sensor = SensorFactory.GetNewSensor(item.DeviceId, item.DeviceName, modelName);
            sensor.MACAddress = item.DeviceMAC;
            this.app.Sensors.Insert(0, sensor);
        }
    }
}
