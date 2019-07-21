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

        // BLE variable
        protected BluetoothLEDevice bluetoothLeDevice;
        GattCharacteristic selectedCharacteristic = null;


        // Sensor status
        protected Vector3? acceleration;
        protected double? angle;
        protected Orientations? orientation;
        protected bool connected;

        // 
        protected bool shouldAutoConnectContinue;

        const string serviceUuid = "6a800001-b5a3-f393-e0a9-e50e24dcca9e";
        const string characteristicUuid = "6a806050-b5a3-f393-e0a9-e50e24dcca9e";


        public TheSensor(string deviceId, string deviceName)
        {
            this.deviceId = deviceId;
            this.deviceName = deviceName;
            autoConnect = false;
            acceleration = null;
            angle = null;
            orientation = null;
            shouldAutoConnectContinue = false;
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
                autoConnect = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AutoConnect"));
            }
        }

        public bool Connected
        {
            get => connected;
        }

        public Vector3? Acceleration { get => acceleration; }

        public double? Angle { get => angle; }

        public Orientations? Orientation { get => orientation; }

        public string MACAddress { get; set; }
        public DateTime LastReport { get => lastReport; }

        public event PropertyChangedEventHandler PropertyChanged;


        #region logic code

        protected async Task<bool> ConnectToSensor()
        {
            bluetoothLeDevice = await BluetoothLEDevice.FromIdAsync(deviceId);
            GattDeviceServicesResult servicesResult = await bluetoothLeDevice.GetGattServicesAsync(BluetoothCacheMode.Uncached);
            if (servicesResult.Status != GattCommunicationStatus.Success)
            {
                Debug.WriteLine("GetGattServicesAsync Error");
                Debug.WriteLine(servicesResult.ProtocolError.ToString());
                return false;
            }
            IReadOnlyList<GattCharacteristic> characteristics = null;
            foreach (GattDeviceService service in servicesResult.Services)
            {
                Debug.WriteLine(service.Uuid);
                if (service.Uuid.ToString() == serviceUuid)
                {
                    var accessStatus = await service.RequestAccessAsync();
                    if (accessStatus == DeviceAccessStatus.Allowed)
                    {
                        var result = await service.GetCharacteristicsAsync(BluetoothCacheMode.Uncached);
                        if (result.Status == GattCommunicationStatus.Success)
                        {
                            characteristics = result.Characteristics;
                        }
                        else
                        {
                            Debug.WriteLine("Error accessing service.");
                            return false;
                        }
                    }
                    else
                    {
                        Debug.WriteLine("ERROR RequestAccessAsync");
                        Debug.WriteLine(accessStatus.ToString());
                        return false;
                    }
                }
            }
            Debug.WriteLine("Successful get service");
            selectedCharacteristic = null;
            foreach (GattCharacteristic c in characteristics)
            {
                if (c.Uuid.ToString() == characteristicUuid)
                {
                    selectedCharacteristic = c;
                }
            }
            if (selectedCharacteristic == null)
            {
                Debug.WriteLine("Characteristic not found.");
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
            selectedCharacteristic.ValueChanged += Characteristic_ValueChanged;

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
                if (!connected)
                {
                    var task = ConnectToSensor();
                    if (await task)
                    {
                        connected = true;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Connected"));
                    }
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
        #endregion
    }
}
