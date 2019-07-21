using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfDemo
{
    public class MonitorBinding : INotifyPropertyChanged
    {
        public string MonitorDeviceName { get; set; }

        public bool Enabled { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private Display.Orientations? lastOrientation;

        public MonitorBinding()
        {
            MonitorDeviceName = "";
            lastOrientation = null;
            Enabled = true;
        }
        public void SensorPropertyChangedEventHandler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "Orientation") { return; }
            if (!Enabled) { return; }
            if (MonitorDeviceName == null || MonitorDeviceName == "") { return; }
            Display.Orientations? newOrientation = (sender as IBLEAccelerationSensor).Orientation;
            if (newOrientation == lastOrientation) { return; }
            if (newOrientation.HasValue)
            {
                Display.Rotate(MonitorDeviceName, newOrientation.Value);
            }
            lastOrientation = newOrientation;
        }
    }
}
