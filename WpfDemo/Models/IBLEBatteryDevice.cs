namespace WpfDemo.Models
{
    using System.ComponentModel;

    public interface IBLEBatteryDevice : INotifyPropertyChanged
    {
        int BatteryLevel { get; }

        int BatteryVoltage { get; }
    }
}
