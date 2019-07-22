using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfDemo
{
    /// <summary>
    /// Interaction logic for ManualPage.xaml
    /// </summary>
    public partial class ManualPage : Page
    {
        public ObservableCollection<string> deviceListFound = new ObservableCollection<string>();
        public ManualPage()
        {
            InitializeComponent();
            Dictionary<uint, Display.MonitorInfo> devDic = Display.FindDevNumList();
            foreach (uint devNum in devDic.Keys)
            {
                deviceListFound.Add(devDic[devNum].deviceName);
            }
            deviceList.ItemsSource = deviceListFound;
        }
        private void MonitorChosed_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) { return; }
            string monitorString = (string)e.AddedItems[0];
            // Save it
        }
        private void hotkeyChangeForRight(object sender, DataGridViewRowDividerDoubleClickEventArgs e)
        {

        }
    }
}
