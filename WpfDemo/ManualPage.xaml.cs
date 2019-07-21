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
        }
        
        private void BtnScan_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<uint, Display.MonitorInfo> devDic = Display.FindDevNumList();
            foreach (uint devNum in devDic.Keys)
            {
                this.monitorsListView.Items.Add(devDic[devNum].deviceString);
                deviceListFound.Add(devDic[devNum].deviceString);
            }
            deviceList.ItemsSource = deviceListFound;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Save custumer's choice to config
        }
        
        private void BtnQuit_Click(object sender, RoutedEventArgs e)
        {
            // Quit the app
        }
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            // Go back to window from a page
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
