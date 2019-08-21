namespace WpfDemo.Sensors
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Threading.Tasks;
    using Windows.Devices.Bluetooth;
    using Windows.Devices.Bluetooth.GenericAttributeProfile;
    using Windows.Devices.Enumeration;
    using Windows.Devices.Power;
    using Windows.Security.Cryptography;
    using WpfDemo.Models;

    public class FerrisSensorV1 : BaseSensor, IBLEBatteryDevice
    {
        protected GattDeviceService batteryLevelService;
        protected GattCharacteristic batteryLevelCharacteristic;
        protected GattCharacteristic batteryVoltageCharacteristic;

        protected int batteryLevel;
        protected int batteryVoltage;

        public FerrisSensorV1(string deviceId, string deviceName)
            : base(deviceId, deviceName)
        {
            this.PropertyChanged += this.SelfPropertyChanged;
        }

        public override string ModelName { get; } = SupportedSensors.FerrisSensorV1;

        public override Guid ServiceUuid { get; } = new Guid("a2220000-f92a-ad93-dc47-9c4df7aa5e9e");

        public override Guid AccelerationCharacteristicUuid { get; } = new Guid("a2226050-f92a-ad93-dc47-9c4df7aa5e9e");

        public virtual Guid BatteryLevelServiceUuid { get; } = new Guid("0000180f-0000-1000-8000-00805f9b34fb");

        public virtual Guid BatteryLevelCharacteristicUuid { get; } = new Guid("00002a19-0000-1000-8000-00805f9b34fb");

        public virtual Guid BatteryVoltageCharacteristicUuid { get; } = new Guid("a2224256-f92a-ad93-dc47-9c4df7aa5e9e");

        public int BatteryLevel
        {
            get => this.batteryLevel;
            protected set
            {
                if (value != this.batteryLevel)
                {
                    this.batteryLevel = value;
                    this.TriggerPropertyChanged(this, new PropertyChangedEventArgs("BatteryLevel"));
                }
            }
        }

        public int BatteryVoltage
        {
            get => this.batteryVoltage;
            protected set
            {
                if (value != this.batteryVoltage)
                {
                    this.batteryVoltage = value;
                    this.TriggerPropertyChanged(this, new PropertyChangedEventArgs("BatteryVoltage"));
                }
            }
        }

        protected void SelfPropertyChanged(object sender, PropertyChangedEventArgs ev)
        {
            if (ev.PropertyName == "Connected")
            {
                if (this.Connected == true)
                {
                    this.OnConnected();
                }
                else
                {
                    this.OnDisConnected();
                }
            }
        }

        protected void OnConnected()
        {
            _ = this.ConnectBatteryLevelChar();
        }

        protected async Task<bool> ConnectBatteryLevelChar()
        {
            GattDeviceServicesResult servicesResult = await this.bluetoothLEDevice.GetGattServicesForUuidAsync(this.BatteryLevelServiceUuid);
            if (servicesResult.Status != GattCommunicationStatus.Success || servicesResult.Services.Count == 0)
            {
                return false;
            }

            this.batteryLevelService?.Dispose();

            this.batteryLevelService = servicesResult.Services[0];
            DeviceAccessStatus accessStatus = await this.batteryLevelService.RequestAccessAsync();
            if (accessStatus != DeviceAccessStatus.Allowed)
            {
                return false;
            }

            IReadOnlyList<GattCharacteristic> characteristics = null;
            var result = await this.batteryLevelService.GetCharacteristicsForUuidAsync(this.BatteryLevelCharacteristicUuid);

            if (result.Status == GattCommunicationStatus.Success)
            {
                characteristics = result.Characteristics;
            }
            else
            {
                this.StatusMessage = "Error accessing service " + result.Status + ".";
                return false;
            }

            if (characteristics.Count == 0)
            {
                this.StatusMessage = "BatteryLevel Characteristic not found.";
                return false;
            }

            this.batteryLevelCharacteristic = characteristics[0];

            await this.ReadBatteryLevel();

            return true;
        }

        protected async Task ReadBatteryLevel()
        {
            if (this.batteryLevelCharacteristic == null)
            {
                return;
            }

            GattReadResult result = await this.batteryLevelCharacteristic.ReadValueAsync(BluetoothCacheMode.Uncached);
            if (result.Status != GattCommunicationStatus.Success)
            {
                this.StatusMessage = "Error reading BatteryLevel: " + result.Status + ".";
                return;
            }

            byte[] data;
            CryptographicBuffer.CopyToByteArray(result.Value, out data);
            this.BatteryLevel = data[0];
        }

        protected void OnDisConnected()
        {
            this.batteryLevelService?.Dispose();
        }
    }
}
