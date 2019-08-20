namespace WpfDemo.Sensors
{
    class SensorFactory
    {
        public static IBLEAccelerationSensor GetNewSensor(string deviceId, string deviceName, string modelName)
        {
            switch (modelName)
            {
                case "FerrisSensorV1":
                    return new FerrisSensorV1(deviceId, deviceName);
                case "InfoLink_NRF51_SensorTag":
                    return new InfoLinkNRF51SensorTag(deviceId, deviceName);
                default:
                    return new BaseSensor(deviceId, deviceName);
            }
        }


    }
}
