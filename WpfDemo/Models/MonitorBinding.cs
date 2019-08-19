namespace WpfDemo
{
    using System.ComponentModel;

    public class MonitorBinding : INotifyPropertyChanged
    {
        private string monitorDeviceName;

        public string MonitorDeviceName
        {
            get => this.monitorDeviceName;
            set
            {
                this.monitorDeviceName = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("MonitorDeviceName"));
            }
        }

        public bool Enabled { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private Display.Orientations? lastOrientation;

        public MonitorBinding()
        {
            this.MonitorDeviceName = string.Empty;
            this.lastOrientation = null;
            this.Enabled = true;
        }

        public void SensorPropertyChangedEventHandler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "Orientation") { return; }
            if (!this.Enabled) { return; }
            if (this.MonitorDeviceName == null || this.MonitorDeviceName == string.Empty) { return; }
            Display.Orientations? newOrientation = (sender as IBLEAccelerationSensor).Orientation;
            if (newOrientation == this.lastOrientation) { return; }
            if (newOrientation.HasValue)
            {
                Display.Rotate(this.MonitorDeviceName, newOrientation.Value);
            }

            this.lastOrientation = newOrientation;
        }
    }
}
