using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Windows.Devices.Enumeration;
using Windows.Devices.Bluetooth;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.NetworkInformation;

namespace WpfDemo
{
    public class ScanDeviceInfo : INotifyPropertyChanged
    {
        public ScanDeviceInfo(DeviceInformation deviceInfoIn)
        {
            DeviceInformation = deviceInfoIn;
        }

        public DeviceInformation DeviceInformation { get; private set; }

        public string DeviceId => DeviceInformation.Id;
        public string DeviceName => DeviceInformation.Name;

        public string DeviceMAC => (string)DeviceInformation.Properties["System.Devices.Aep.DeviceAddress"];

        public event PropertyChangedEventHandler PropertyChanged;

        public void Update(DeviceInformationUpdate deviceInfoUpdate)
        {
            Debug.WriteLine(String.Format("Before update: {0}", DeviceInformation.Name));
            DeviceInformation.Update(deviceInfoUpdate);
            Debug.WriteLine(String.Format("After update: {0}", DeviceInformation.Name));

            OnPropertyChanged("Name");
            OnPropertyChanged("DeviceInformation");
        }
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
    /// <summary>
    /// Interaction logic for SensorsPage.xaml
    /// </summary>
    public partial class AddSensorPage : Page
    {
        private DeviceWatcher deviceWatcher;
        private App app;
        private Window window;
        //SFConfigStore configStore;

        public ObservableCollection<ScanDeviceInfo> DeviceCollection { get; set; }
        public AddSensorPage(App app, Window window)
        {
            this.app = app;
            this.window = window;
            DeviceCollection = new ObservableCollection<ScanDeviceInfo>();
            InitializeComponent();
        }

        public void StartScan()
        {
            if (deviceWatcher == null)
            {
                string[] requestedProperties = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsConnected", "System.Devices.Aep.Bluetooth.Le.IsConnectable" };

                // BT_Code: Example showing paired and non-paired in a single query.
                string aqsAllBluetoothLEDevices = "(System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\")";

                BluetoothLEDevice.GetDeviceSelectorFromPairingState(false);

                deviceWatcher =
                        DeviceInformation.CreateWatcher(
                            aqsAllBluetoothLEDevices,
                            requestedProperties,
                            DeviceInformationKind.AssociationEndpoint);
                deviceWatcher.Added += DeviceWatcher_Added;
                deviceWatcher.Updated += DeviceWatcher_Updated;
                deviceWatcher.Removed += DeviceWatcher_Removed;
                deviceWatcher.EnumerationCompleted += DeviceWatcher_EnumerationCompleted;
                deviceWatcher.Stopped += DeviceWatcher_Stopped;
                deviceWatcher.Start();

            }
        }
        public void StopScan()
        {
            if (deviceWatcher != null)
            {
                // Unregister the event handlers.
                deviceWatcher.Added -= DeviceWatcher_Added;
                deviceWatcher.Updated -= DeviceWatcher_Updated;
                deviceWatcher.Removed -= DeviceWatcher_Removed;
                deviceWatcher.EnumerationCompleted -= DeviceWatcher_EnumerationCompleted;
                deviceWatcher.Stopped -= DeviceWatcher_Stopped;

                // Stop the watcher.
                deviceWatcher.Stop();
                deviceWatcher = null;
            }
        }
        private void DeviceWatcher_Added(DeviceWatcher sender, DeviceInformation deviceInfo)
        {
            Debug.WriteLine(String.Format("Added {0}{1}", deviceInfo.Id, deviceInfo.Name));
            if (sender != deviceWatcher) { return; }
            window.Dispatcher.Invoke(() =>
            {
                DeviceCollection.Add(new ScanDeviceInfo(deviceInfo));
            });
        }
        private void DeviceWatcher_Updated(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
        {
            if (sender != deviceWatcher) { return; }
            Debug.WriteLine(String.Format("Added {0}{1}", deviceInfoUpdate.Id, deviceInfoUpdate.Properties));

            ScanDeviceInfo dev = FindDeviceInfoById(deviceInfoUpdate.Id);
            if (dev == null) { return; }
            dev.Update(deviceInfoUpdate);
        }
        private void DeviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
        {
            string id = deviceInfoUpdate.Id;
            window.Dispatcher.Invoke(() =>
            {
                ScanDeviceInfo dev = FindDeviceInfoById(deviceInfoUpdate.Id);
                if (dev != null)
                {
                    DeviceCollection.Remove(dev);
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
            foreach (ScanDeviceInfo dev in DeviceCollection)
            {
                if (dev.DeviceId == id)
                {
                    return dev;
                }
            }
            return null;
        }

        #region "UICODE"
        private void btnStartScanClick(object sender, RoutedEventArgs e)
        {
            StartScan();
        }
        private void btnAddClick(object sender, RoutedEventArgs e)
        {
            ScanDeviceInfo item = ScannedDevicesList.SelectedItem as ScanDeviceInfo;
            if (item == null)
            {
                return;
            }
            var sensor = new Sensors.TheSensor(item.DeviceId, item.DeviceName) {
                MACAddress = item.DeviceMAC,
                // PhysicalAddress.Parse(item.DeviceMAC.Replace(':', '-').ToUpper())
            };
            app.Sensors.Insert(0, sensor);
        }

        #endregion
    }
}
