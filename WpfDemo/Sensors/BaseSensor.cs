namespace WpfDemo.Sensors
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Numerics;
    using System.Threading;
    using System.Threading.Tasks;
    using Windows.Devices.Bluetooth;
    using Windows.Devices.Bluetooth.GenericAttributeProfile;
    using Windows.Devices.Enumeration;
    using Windows.Security.Cryptography;
    using Orientations = WpfDemo.Display.Orientations;

    public class BaseSensor : IBLEAccelerationSensor
    {
        protected string deviceId;
        protected string deviceName;
        protected bool autoConnect;
        protected Task autoConnectTask;
        protected Vector3? baseline;
        protected Vector3? normal;
        protected DateTime lastReport;
        protected DateTime lastConnected;
        protected MonitorBinding monitorBinding;
        protected string statusMessage;

        // BLE variable
        protected BluetoothLEDevice bluetoothLEDevice;
        protected GattDeviceService selectedService;
        protected GattCharacteristic subscribedCharacteristic = null;

        // Sensor status
        protected Vector3? acceleration;
        protected double? angle;
        protected Orientations? orientation;
        protected bool connected;

        // AutoConnecting
        protected bool shouldAutoConnectContinue;

        protected BLESensorConnectionStatus connectionStatus;

        public BaseSensor(string deviceId, string deviceName)
        {
            this.deviceId = deviceId;
            this.deviceName = deviceName;
            this.autoConnect = false;
            this.acceleration = null;
            this.angle = null;
            this.orientation = null;
            this.shouldAutoConnectContinue = false;
            this.monitorBinding = new MonitorBinding();
            this.PropertyChanged += this.monitorBinding.SensorPropertyChangedEventHandler;
            this.monitorBinding.PropertyChanged += this.MonitorBindingPropertyChanged;
            this.connectionStatus = BLESensorConnectionStatus.NotConnected;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual Guid ServiceUuid { get; } = new Guid("00000000-f92a-ad93-dc47-9c4df7aa5e9e");

        public virtual Guid AccelerationCharacteristicUuid { get; } = new Guid("00006050-f92a-ad93-dc47-9c4df7aa5e9e");

        public Vector3? Baseline
        {
            get => this.baseline;
            set
            {
                this.baseline = value;
                if (this.baseline.HasValue && this.baseline.Value.Length() > 0)
                {
                    this.baseline /= this.baseline.Value.Length();
                }

                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Baseline"));
            }
        }

        public Vector3? Normal
        {
            get => this.normal;
            set
            {
                this.normal = value;
                if (this.normal.HasValue && this.normal.Value.Length() > 0)
                {
                    this.normal /= this.normal.Value.Length();
                }

                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Normal"));
            }
        }

        public string DeviceId
        {
            get => this.deviceId;
        }

        public virtual string ModelName { get; } = "BaicBLEAcclerationSensor";

        public string DeviceName
        {
            get => this.deviceName;
            set
            {
                this.deviceName = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DeviceName"));
            }
        }

        public bool AutoConnect
        {
            get => this.autoConnect;
            set
            {
                if (value == this.autoConnect)
                {
                    return;
                }

                if (value == true)
                {
                    this.EnableAutoConnect();
                }
                else
                {
                    this.DisableAutoConnect();
                }

                bool changed = value != this.autoConnect;
                this.autoConnect = value;
                if (changed)
                {
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AutoConnect"));
                }
            }
        }

        public bool Connected
        {
            get => this.connected;
            private set
            {
                if (value != this.connected)
                {
                    this.connected = value;
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Connected"));
                    if (!this.connected)
                    {
                        this.CleanUpSubscription();
                    }

                    if (this.connected)
                    {
                        this.ConnectionStatus = BLESensorConnectionStatus.Connected;
                    }
                    else
                    {
                        this.ConnectionStatus = BLESensorConnectionStatus.NotConnected;
                    }
                }
            }
        }

        public Vector3? Acceleration { get => this.acceleration; }

        public double? Angle { get => this.angle; }

        public Orientations? Orientation { get => this.orientation; }

        public string MACAddress { get; set; }

        public DateTime LastReport
        {
            get => this.lastReport;
            private set
            {
                this.lastReport = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LastReport"));
            }
        }

        public MonitorBinding Binding { get => this.monitorBinding; }

        public BLESensorConnectionStatus ConnectionStatus
        {
            get => this.connectionStatus;
            private set
            {
                bool changed = value != this.connectionStatus;
                this.connectionStatus = value;
                if (changed)
                {
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ConnectionStatus"));
                }
            }
        }

        public string StatusMessage
        {
            get => this.statusMessage;
            set
            {
                if (value != this.statusMessage)
                {
                    this.statusMessage = value;
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("StatusMessage"));
                }

                Debug.WriteLine(value);
            }
        }

        public void Disconnect()
        {
            this.AutoConnect = false;
            this.CleanUpSubscription();
        }

        protected void TriggerPropertyChanged(object sender, PropertyChangedEventArgs ev)
        {
            this.PropertyChanged?.Invoke(sender, ev);
        }

        protected void BluetoothLeDeviceConnectionStatusChanged(BluetoothLEDevice sender, object e)
        {
            if (sender.ConnectionStatus == BluetoothConnectionStatus.Disconnected)
            {
                this.Connected = false;
                sender.ConnectionStatusChanged -= this.BluetoothLeDeviceConnectionStatusChanged;
            }
        }

        protected async Task<bool> ConnectToSensor()
        {
            this.StatusMessage = "Connecting" + this.DeviceId;
            if (this.bluetoothLEDevice == null)
            {
                this.bluetoothLEDevice = await BluetoothLEDevice.FromIdAsync(this.deviceId);
                this.bluetoothLEDevice.ConnectionStatusChanged += this.BluetoothLeDeviceConnectionStatusChanged;
            }

            GattDeviceServicesResult servicesResult = await this.bluetoothLEDevice.GetGattServicesForUuidAsync(this.ServiceUuid);
            if (servicesResult.Status != GattCommunicationStatus.Success)
            {
                this.StatusMessage = "GetGattServicesAsync Error:" + servicesResult.ProtocolError.ToString();
                return false;
            }

            if (servicesResult.Services.Count == 0)
            {
                this.StatusMessage = "Can not get service.";
                return false;
            }

            if (this.selectedService != null)
            {
                this.selectedService.Dispose();
            }

            this.selectedService = servicesResult.Services[0];

            IReadOnlyList<GattCharacteristic> characteristics = null;

            this.StatusMessage = "Successfully conecte to service: " + this.selectedService.Uuid.ToString();

            DeviceAccessStatus accessStatus = await this.selectedService.RequestAccessAsync();
            if (accessStatus == DeviceAccessStatus.Allowed)
            {
                var result = await this.selectedService.GetCharacteristicsForUuidAsync(this.AccelerationCharacteristicUuid);
                if (result.Status == GattCommunicationStatus.Success)
                {
                    characteristics = result.Characteristics;
                }
                else
                {
                    this.StatusMessage = "Error accessing service " + result.Status + ".";
                    return false;
                }
            }
            else
            {
                this.StatusMessage = "ERROR RequestAccessAsync: " + accessStatus.ToString();
                return false;
            }

            if (characteristics.Count == 0)
            {
                this.StatusMessage = "Characteristic not found.";
                return false;
            }

            GattCharacteristic selectedCharacteristic = characteristics[0];

            GattCommunicationStatus disableNotificationstatus =
                await selectedCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.None);
            if (disableNotificationstatus != GattCommunicationStatus.Success)
            {
                this.StatusMessage = "Error clearing registering for value changes: " + disableNotificationstatus;
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
                this.StatusMessage = "Characteristic doesn't support Indicate or Notify";
                return false;
            }

            GattCommunicationStatus status = await selectedCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(cccdValue);
            if (status != GattCommunicationStatus.Success)
            {
                this.StatusMessage = "Error registering for value changes: " + status;
                return false;
            }

            this.StatusMessage = "Notify enabled";
            this.subscribedCharacteristic = selectedCharacteristic;
            this.subscribedCharacteristic.ValueChanged += this.Characteristic_ValueChanged;

            return true;
        }

        protected void MonitorBindingPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("BindMonitor"));
        }

        protected void Characteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            this.LastReport = DateTime.Now;
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LastReport"));

            byte[] data;
            CryptographicBuffer.CopyToByteArray(args.CharacteristicValue, out data);
            if (System.BitConverter.IsLittleEndian)
            {
                for (int i = 0; i < data.Length; i += 2)
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
            this.OnNewAcceleration(new Vector3((float)x, (float)y, (float)z));
        }

        protected void OnNewAcceleration(Vector3 acceleration)
        {
            if (acceleration.Length() == 0)
            {
                return;
            }

            Vector3 v = acceleration;
            Vector3? right = null;
            if (this.normal.HasValue && this.baseline.HasValue)
            {
                right = Vector3.Cross(this.normal.Value, this.baseline.Value);
            }

            if (this.normal != null)
            {
                v -= this.normal.Value * Vector3.Dot(v, this.normal.Value);
                v /= v.Length();
            }

            double? newAngle = null;
            if (this.baseline.HasValue)
            {
                if (!right.HasValue)
                {
                    right = Vector3.Cross(v, this.baseline.Value);
                    right /= right.Value.Length();
                }

                float x = Vector3.Dot(v, this.baseline.Value);
                float y = Vector3.Dot(v, right.Value);
                newAngle = Math.Atan2(y, x);
            }

            Orientations? newOrientation = null;
            if (newAngle.HasValue && this.normal.HasValue)
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

            if (newAngle != this.angle)
            {
                this.angle = newAngle;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Angle"));
            }

            if (newOrientation != this.orientation)
            {
                this.orientation = newOrientation;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Orientation"));
            }

            if (acceleration != this.acceleration)
            {
                this.acceleration = acceleration;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Acceleration"));
            }
        }

        protected async void AutoConnectWorkerAsync()
        {
            Debug.WriteLine(string.Format("Enabling atuoconnect on {0}", this.deviceId));
            while (this.shouldAutoConnectContinue)
            {
                var timeoutDeadline = DateTime.Now - new TimeSpan(hours: 0, minutes: 0, seconds: 30);
                if (this.lastReport < timeoutDeadline && this.lastConnected < timeoutDeadline)
                {
                    this.Connected = false;
                }

                if (!this.Connected)
                {
                    this.ConnectionStatus = BLESensorConnectionStatus.Connecting;
                    var task = this.ConnectToSensor();
                    try
                    {
                        if (await task)
                        {
                            this.Connected = true;
                            this.lastConnected = DateTime.Now;
                        }
                    }
                    catch (Exception e)
                    {
                        this.StatusMessage = "Connection Error:" + e.Message;
                    }

                    if (!this.Connected)
                    {
                        this.ConnectionStatus = BLESensorConnectionStatus.NotConnected;
                    }
                }

                Thread.Sleep(5000);
            }
        }

        protected void EnableAutoConnect()
        {
            this.shouldAutoConnectContinue = true;
            this.autoConnectTask = Task.Run(() => this.AutoConnectWorkerAsync());
        }

        protected void DisableAutoConnect()
        {
            if (this.autoConnectTask != null)
            {
                this.shouldAutoConnectContinue = false;
            }
        }

        protected void CleanUpSubscription()
        {
            if (this.subscribedCharacteristic != null)
            {
                _ = Task.Run(async () =>
                    {
                        GattCommunicationStatus disableNotificationstatus =
                        await this.subscribedCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.None);
                        if (disableNotificationstatus != GattCommunicationStatus.Success)
                        {
                            Debug.WriteLine("Error clearing registering for value changes: " + disableNotificationstatus);
                        }
                    });

                this.subscribedCharacteristic.ValueChanged -= this.Characteristic_ValueChanged;
                this.subscribedCharacteristic = null;

                Debug.WriteLine("Cleanup Subscription");
            }

            if (this.selectedService != null)
            {
                this.selectedService.Session.Dispose();
                this.selectedService.Dispose();
                this.selectedService = null;
            }

            if (this.bluetoothLEDevice != null)
            {
                this.bluetoothLEDevice.Dispose();
                this.bluetoothLEDevice = null;
            }
        }
    }
}
