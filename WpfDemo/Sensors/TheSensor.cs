using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Security.Cryptography;

namespace WpfDemo.Sensors
{
    using System.Net.NetworkInformation;
    using Orientations = WpfDemo.Display.Orientations;
    public class TheSensor : IBLEAccelerationSensor
    {
        protected string deviceId;
        protected string deviceName;
        protected bool autoConnect;
        protected Task autoConnectTask;
        protected Vector3? baseline, normal;
        protected DateTime lastReport;
        protected MonitorBinding monitorBinding;

        // BLE variable
        protected BluetoothLEDevice bluetoothLEDevice;
        protected GattDeviceService selectedService;
        protected GattCharacteristic subscribedCharacteristic = null;


        // Sensor status
        protected Vector3? acceleration;
        protected double? angle;
        protected Orientations? orientation;
        protected bool connected;

        // 
        protected bool shouldAutoConnectContinue;

        BLESensorConnectionStatus connectionStatus;

        readonly Guid serviceUuid = new Guid("6a800001-b5a3-f393-e0a9-e50e24dcca9e");
        readonly Guid characteristicUuid = new Guid("6a806050-b5a3-f393-e0a9-e50e24dcca9e");


        public TheSensor(string deviceId, string deviceName)
        {
            this.deviceId = deviceId;
            this.deviceName = deviceName;
            autoConnect = false;
            acceleration = null;
            angle = null;
            orientation = null;
            shouldAutoConnectContinue = false;
            monitorBinding = new MonitorBinding();
            this.PropertyChanged += monitorBinding.SensorPropertyChangedEventHandler;
            connectionStatus = BLESensorConnectionStatus.NotConnected;
        }

        public Vector3? Baseline
        {
            get => baseline;
            set
            {
                baseline = value;
                if (baseline.HasValue && baseline.Value.Length() > 0) { baseline /= baseline.Value.Length(); }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Baseline"));

            }
        }
        public Vector3? Normal
        {
            get => normal;
            set
            {
                normal = value;
                if (normal.HasValue && normal.Value.Length() > 0) { normal /= normal.Value.Length(); }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Normal"));


            }
        }
        public string DeviceId
        {
            get => deviceId;
        }

        public string DeviceName
        {
            get => deviceName;
            set
            {
                deviceName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DeviceName"));

            }
        }

        public bool AutoConnect
        {
            get => autoConnect;
            set
            {
                if (value == autoConnect) { return; }
                if (value == true)
                {
                    EnableAutoConnect();
                }
                else
                {
                    DisableAutoConnect();

                }
                bool changed = value != autoConnect;
                autoConnect = value;
                if (changed) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AutoConnect")); }

            }
        }

        public bool Connected
        {
            get => connected;
            private set
            {
                bool changed = value != connected;
                connected = value;
                if (!connected)
                {
                    CleanUpSubscription();
                }
                if (changed) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Connected")); }
                if (connected)
                {
                    ConnectionStatus = BLESensorConnectionStatus.Connected;
                } else
                {
                    ConnectionStatus = BLESensorConnectionStatus.NotConnected;
                }
            }
        }

        public Vector3? Acceleration { get => acceleration; }

        public double? Angle { get => angle; }

        public Orientations? Orientation { get => orientation; }

        public string MACAddress { get; set; }
        public DateTime LastReport
        {
            get => lastReport;
            private set
            {
                lastReport = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LastReport"));
            }
        }


        public MonitorBinding Binding { get => monitorBinding; }

        public BLESensorConnectionStatus ConnectionStatus
        {
            get => connectionStatus;
            private set
            {
                bool changed = value != connectionStatus;
                connectionStatus = value;
                if (changed) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ConnectionStatus")); }
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;


        #region logic code
        protected void bluetoothLeDeviceConnectionStatusChanged(BluetoothLEDevice sender, object e)
        {
            if (sender.ConnectionStatus == BluetoothConnectionStatus.Disconnected)
            {
                Connected = false;
                sender.ConnectionStatusChanged -= bluetoothLeDeviceConnectionStatusChanged;
            }
        }
        protected async Task<bool> ConnectToSensor()
        {
            if (bluetoothLEDevice == null)
            {
                bluetoothLEDevice = await BluetoothLEDevice.FromIdAsync(deviceId);
                bluetoothLEDevice.ConnectionStatusChanged += bluetoothLeDeviceConnectionStatusChanged;
            }

            GattDeviceServicesResult servicesResult = await bluetoothLEDevice.GetGattServicesForUuidAsync(serviceUuid);
            if (servicesResult.Status != GattCommunicationStatus.Success)
            {
                Debug.WriteLine("GetGattServicesAsync Error");
                Debug.WriteLine(servicesResult.ProtocolError.ToString());
                return false;
            }
            if (servicesResult.Services.Count == 0)
            {
                Debug.WriteLine("Can not get service.");
                return false;
            }
            if (selectedService != null)
            {
                selectedService.Dispose();
            }
            selectedService = servicesResult.Services[0];

            IReadOnlyList<GattCharacteristic> characteristics = null;


            Debug.WriteLine(selectedService.Uuid);

            DeviceAccessStatus accessStatus = await selectedService.RequestAccessAsync();
            if (accessStatus == DeviceAccessStatus.Allowed)
            {
                var result = await selectedService.GetCharacteristicsForUuidAsync(characteristicUuid);
                if (result.Status == GattCommunicationStatus.Success)
                {
                    characteristics = result.Characteristics;
                }
                else
                {
                    Debug.WriteLine("Error accessing service " + result.Status + ".");
                    return false;
                }
            }
            else
            {
                Debug.WriteLine("ERROR RequestAccessAsync");
                Debug.WriteLine(accessStatus.ToString());
                return false;
            }


            Debug.WriteLine("Successful get service");
            if (characteristics.Count == 0)
            {
                Debug.WriteLine("Characteristic not found.");
                return false;
            }
            GattCharacteristic selectedCharacteristic = characteristics[0];

            GattCommunicationStatus disableNotificationstatus =
                await selectedCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.None);
            if (disableNotificationstatus != GattCommunicationStatus.Success)
            {
                Debug.WriteLine("Error clearing registering for value changes: " + disableNotificationstatus);
                return false;
            }
            // Enable notify
            var cccdValue = GattClientCharacteristicConfigurationDescriptorValue.None;
            if (selectedCharacteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Indicate))
            {
                cccdValue = GattClientCharacteristicConfigurationDescriptorValue.Indicate;
            }
            else if (selectedCharacteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Notify))
            {
                cccdValue = GattClientCharacteristicConfigurationDescriptorValue.Notify;
            }
            else
            {
                Debug.WriteLine("Characteristic doesn't support Indicate or Notify");
                return false;
            }
            Debug.WriteLine("Enable notify");
            GattCommunicationStatus status = await selectedCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(cccdValue);
            if (status != GattCommunicationStatus.Success)
            {
                Debug.WriteLine("Error registering for value changes: " + status);
                return false;
            }
            subscribedCharacteristic = selectedCharacteristic;
            subscribedCharacteristic.ValueChanged += Characteristic_ValueChanged;

            return true;

        }
        protected void Characteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            lastReport = DateTime.Now;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LastReport"));

            byte[] data;
            CryptographicBuffer.CopyToByteArray(args.CharacteristicValue, out data); ;
            if (System.BitConverter.IsLittleEndian)
            {
                for (int i = 0; i < 12; i += 2)
                {
                    byte t = data[i];
                    data[i] = data[i + 1];
                    data[i + 1] = t;
                }
            }
            Int16 ix = System.BitConverter.ToInt16(data, 0);
            Int16 iy = System.BitConverter.ToInt16(data, 2);
            Int16 iz = System.BitConverter.ToInt16(data, 4);
            double umax = 32768;
            double x = ix / umax * 20;
            double y = iy / umax * 20;
            double z = iz / umax * 20;
            OnNewAcceleration(new Vector3((float)x, (float)y, (float)z));
        }

        protected void OnNewAcceleration(Vector3 acceleration)
        {
            if (acceleration.Length() == 0) { return; }
            Vector3 v = acceleration;
            Vector3? right = null;
            if (normal.HasValue && baseline.HasValue)
            {
                right = Vector3.Cross(normal.Value, baseline.Value);
            }
            if (normal != null)
            {
                v -= normal.Value * Vector3.Dot(v, normal.Value);
                v /= v.Length();
            }

            double? newAngle = null;
            if (baseline.HasValue)
            {
                if (!right.HasValue)
                {
                    right = Vector3.Cross(v, baseline.Value);
                    right /= right.Value.Length();
                }
                float x = Vector3.Dot(v, baseline.Value);
                float y = Vector3.Dot(v, right.Value);
                newAngle = Math.Atan2(y, x);
            }


            Orientations? newOrientation = null;
            if (newAngle.HasValue && normal.HasValue)
            {
                if (newAngle > -Math.PI / 4 && newAngle <= Math.PI / 4)
                {
                    newOrientation = Orientations.DEGREES_CW_0;
                }
                else if (newAngle > Math.PI / 4 && newAngle <= Math.PI / 4 * 3)
                {
                    newOrientation = Orientations.DEGREES_CW_270;
                }
                else if (newAngle > Math.PI / 4 * 3 || newAngle <= -Math.PI / 4 * 3)
                {
                    newOrientation = Orientations.DEGREES_CW_180;
                }
                else
                {
                    newOrientation = Orientations.DEGREES_CW_90;
                }
            }

            if (newAngle != angle)
            {
                angle = newAngle;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Angle"));
            }
            if (newOrientation != orientation)
            {
                orientation = newOrientation;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Orientation"));
            }
            if (acceleration != this.acceleration)
            {
                this.acceleration = acceleration;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Acceleration"));
            }

        }
        protected async void autoConnectWorkerAsync()
        {
            Debug.WriteLine(String.Format("Enabling atuoconnect on {0}", deviceId));
            while (shouldAutoConnectContinue)
            {
                if (DateTime.Now - LastReport > new TimeSpan(hours: 0, minutes: 0, seconds: 30))
                {
                    Connected = false;
                }
                if (!Connected)
                {
                    ConnectionStatus = BLESensorConnectionStatus.Connecting;
                    var task = ConnectToSensor();
                    try
                    {
                        if (await task)
                        {
                            Connected = true;
                        }

                    }
                    catch (Exception e)
                    {
                        Debugger.Break();
                    }
                    if (!Connected)
                    {
                        ConnectionStatus = BLESensorConnectionStatus.NotConnected;
                    }
                    lastReport = DateTime.Now; // W
                }
                Thread.Sleep(5000);
            }
        }
        protected void EnableAutoConnect()
        {
            shouldAutoConnectContinue = true;
            autoConnectTask = Task.Run(autoConnectWorkerAsync);
        }
        protected void DisableAutoConnect()
        {
            if (autoConnectTask != null)
            {
                shouldAutoConnectContinue = false;
            }

        }
        protected void CleanUpSubscription()
        {
            if (subscribedCharacteristic != null)
            {
                subscribedCharacteristic.ValueChanged -= Characteristic_ValueChanged;
                subscribedCharacteristic = null;
             
                Debug.WriteLine("Cleanup Subscription");
            }
            if (selectedService != null)
            {
                selectedService.Session.Dispose();
                selectedService.Dispose();
                selectedService = null;
            }
            if (bluetoothLEDevice != null)
            {
                bluetoothLEDevice.Dispose();
                bluetoothLEDevice = null;
            }
        }

        public void Disconnect()
        {
            this.AutoConnect = false;
            CleanUpSubscription();
        }
        #endregion
    }
}
