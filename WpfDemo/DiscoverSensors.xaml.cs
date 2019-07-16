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

namespace WpfDemo
{
    public class DeviceInfo : INotifyPropertyChanged
    {
        public DeviceInfo(DeviceInformation deviceInfoIn)
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class DiscoverSensorsWindow : Window
    {
        private DeviceWatcher deviceWatcher;
        SFConfigStore configStore;

        public ObservableCollection<DeviceInfo> DeviceCollection { get; set; }
        public DiscoverSensorsWindow()
        {
            configStore = new SFConfigStore();
            DataContext = this;
            DeviceCollection = new ObservableCollection<DeviceInfo>();
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
            Dispatcher.Invoke(() =>
            {
                DeviceCollection.Add(new DeviceInfo(deviceInfo));
            });
        }
        private void DeviceWatcher_Updated(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
        {
            if (sender != deviceWatcher) { return; }
            Debug.WriteLine(String.Format("Added {0}{1}", deviceInfoUpdate.Id, deviceInfoUpdate.Properties));

            DeviceInfo dev = FindDeviceInfoById(deviceInfoUpdate.Id);
            if (dev == null) { return; }
            dev.Update(deviceInfoUpdate);
        }
        private void DeviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
        {
            string id = deviceInfoUpdate.Id;
            Dispatcher.Invoke(() =>
            {
                DeviceInfo dev = FindDeviceInfoById(deviceInfoUpdate.Id);
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

        private DeviceInfo FindDeviceInfoById(string id)
        {
            foreach (DeviceInfo dev in DeviceCollection)
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
            DeviceInfo item = ScannedDevicesList.SelectedItem as DeviceInfo;
            if (item == null)
            {
                return;
            }
            configStore.settings.sensors.Add(new BLEGravitySensors()
            {
                Id = item.DeviceId,
                Name = item.DeviceName,
            });
            configStore.Save();

        }

        #endregion

    }
}
