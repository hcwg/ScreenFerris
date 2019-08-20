namespace WpfDemo.Sensors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class FerrisSensorV1 : BaseSensor
    {
        public FerrisSensorV1(string deviceId, string deviceName)
            : base(deviceId, deviceName)
        {
        }

        public override string ModelName { get; } = SupportedSensors.FerrisSensorV1;

        public override Guid ServiceUuid { get; } = new Guid("a2220000-f92a-ad93-dc47-9c4df7aa5e9e");

        public override Guid AccelerationCharacteristicUuid { get; } = new Guid("a2226050-f92a-ad93-dc47-9c4df7aa5e9e");
    }
}
