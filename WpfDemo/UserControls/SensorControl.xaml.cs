using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfDemo.UserControls
{
    /// <summary>
    /// Interaction logic for SensorControl.xaml
    /// </summary>
    public partial class SensorControl : UserControl
    {
        IBLEAccelerationSensor sensor;
        private ObservableCollection<string> monitorDeviceNames = new ObservableCollection<string>();

        public SensorControl(IBLEAccelerationSensor sensor)
        {
            InitializeComponent();
            this.sensor = sensor;
            stackPanelMain.DataContext = sensor;
            comboBindMonitor.ItemsSource = monitorDeviceNames;
            RefreshDisplays();
        }

        private void ButtonCalibrate_Click(object sender, RoutedEventArgs e)
        {
            CalibrationWindow calibrationWindow = new CalibrationWindow(sensor);
            calibrationWindow.Show();
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshDisplays();
        }

        private void BtnDisconnect_Click(object sender, RoutedEventArgs e)
        {
            sensor.Disconnect();
        }

        private void BtnClearBind_Click(object sender, RoutedEventArgs e)
        {
            sensor.Binding.MonitorDeviceName = "";
        }
        private void RefreshDisplays()
        {
            Dictionary<uint, Display.MonitorInfo> devDic = Display.FindDevNumList();
            monitorDeviceNames.Clear();
            foreach (uint devNum in devDic.Keys)
            {
                monitorDeviceNames.Add(devDic[devNum].deviceName);
            }
        }
    }

   
}
