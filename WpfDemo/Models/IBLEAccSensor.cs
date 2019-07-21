using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace WpfDemo
{
    public delegate void AngleChangeHandler(IBLEAccelerationSensor sender, double angle);
    public delegate void AccelerationChangeHandler(IBLEAccelerationSensor sender, Vector3 acceleration);
    public delegate void OrientationChanged(IBLEAccelerationSensor sender, Display.Orientations orientation);
    public interface IBLEAccelerationSensor : INotifyPropertyChanged
    {
        string DeviceId { get;}

        string DeviceName { get; set; }

        bool AutoConnect { get; set; }

        bool Connected { get;}

        Vector3? Baseline { get; }

        Vector3? Normal { get; }

        Vector3? Acceleration { get; }

        double? Angle { get; }

        Display.Orientations? Orientation { get; }

        //event AngleChangeHandler AngleChange;
        //event AccelerationChangeHandler AccelerationChange;
        //event OrientationChanged OrientationChanged;
    }
}
