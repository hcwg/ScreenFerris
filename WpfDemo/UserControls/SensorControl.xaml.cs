namespace WpfDemo.UserControls
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for SensorControl.xaml.
    /// </summary>
    public partial class SensorControl : UserControl
    {
        IBLEAccelerationSensor sensor;
        private ObservableCollection<string> monitorDeviceNames = new ObservableCollection<string>();

        public SensorControl(IBLEAccelerationSensor sensor)
        {
            this.InitializeComponent();
            this.sensor = sensor;
            this.stackPanelMain.DataContext = sensor;
            this.comboBindMonitor.ItemsSource = this.monitorDeviceNames;
            this.RefreshDisplays();
        }

        private void ButtonCalibrate_Click(object sender, RoutedEventArgs e)
        {
            CalibrationWindow calibrationWindow = new CalibrationWindow(this.sensor);
            calibrationWindow.Show();
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            this.RefreshDisplays();
        }

        private void BtnDisconnect_Click(object sender, RoutedEventArgs e)
        {
            this.sensor.Disconnect();
        }

        private void BtnClearBind_Click(object sender, RoutedEventArgs e)
        {
            this.sensor.Binding.MonitorDeviceName = string.Empty;
        }

        private void RefreshDisplays()
        {
            Dictionary<uint, Display.MonitorInfo> devDic = Display.FindDevNumList();
            this.monitorDeviceNames.Clear();
            foreach (uint devNum in devDic.Keys)
            {
                this.monitorDeviceNames.Add(devDic[devNum].deviceName);
            }
        }
    }

}
