namespace WpfDemo
{
    using System;
    using System.ComponentModel;
    using System.Numerics;

    public delegate void AngleChangeHandler(IBLEAccelerationSensor sender, double angle);

    public delegate void AccelerationChangeHandler(IBLEAccelerationSensor sender, Vector3 acceleration);

    public delegate void OrientationChanged(IBLEAccelerationSensor sender, Display.Orientations orientation);

    public enum BLESensorConnectionStatus
    {
        Connected,
        NotConnected,
        Connecting,
        Error,
    }

    public interface IBLEAccelerationSensor : INotifyPropertyChanged
    {
        string DeviceId { get; }

        string DeviceName { get; set; }

        string MACAddress { get; }

        bool AutoConnect { get; set; }

        bool Connected { get; }

        Vector3? Baseline { get; }

        Vector3? Normal { get; }

        Vector3? Acceleration { get; }

        DateTime LastReport { get; }

        double? Angle { get; }

        Display.Orientations? Orientation { get; }

        MonitorBinding Binding { get; }

        BLESensorConnectionStatus ConnectionStatus { get; }

        string StatusMessage { get; }

        void Disconnect();

        // event AngleChangeHandler AngleChange;
        // event AccelerationChangeHandler AccelerationChange;
        // event OrientationChanged OrientationChanged;
    }
}
