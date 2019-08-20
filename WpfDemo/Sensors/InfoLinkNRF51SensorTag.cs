namespace WpfDemo.Sensors
{
    using System;

    public class InfoLinkNRF51SensorTag : BaseSensor
    {
        public InfoLinkNRF51SensorTag(string deviceId, string deviceName)
            : base(deviceId, deviceName)
        {
        }

        public override string ModelName { get; } = SupportedSensors.InfoLinkNRF51SensorTag;

        public override Guid ServiceUuid { get; } = new Guid("6a800001-b5a3-f393-e0a9-e50e24dcca9e");

        public override Guid AccelerationCharacteristicUuid { get; } = new Guid("6a806050-b5a3-f393-e0a9-e50e24dcca9e");
    }
}
