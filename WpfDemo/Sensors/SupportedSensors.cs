namespace WpfDemo.Sensors
{
    using System;
    using System.Threading.Tasks;
    using Windows.Devices.Bluetooth;
    using Windows.Devices.Bluetooth.GenericAttributeProfile;

    public class SupportedSensors
    {
        public static readonly string FerrisSensorV1 = "FerrisSensorV1";
        public static readonly string InfoLinkNRF51SensorTag = "InfoLink_NRF51_SensorTag";

        public static async Task<string> GuessModelNameAsync(BluetoothLEDevice bluetoothLEDevice)
        {
            GattDeviceServicesResult servicesResult = await bluetoothLEDevice.GetGattServicesAsync();
            if (servicesResult.Status != GattCommunicationStatus.Success)
            {
                return string.Empty;
            }

            foreach (var service in servicesResult.Services)
            {
                switch (service.Uuid.ToString())
                {
                    case "a2220000-f92a-ad93-dc47-9c4df7aa5e9e":
                        return SupportedSensors.FerrisSensorV1;
                    case "6a800001-b5a3-f393-e0a9-e50e24dcca9e":
                        return SupportedSensors.InfoLinkNRF51SensorTag;
                    default:
                        break;
                }
            }

            return "Unknown sensor";

        }
    }
}