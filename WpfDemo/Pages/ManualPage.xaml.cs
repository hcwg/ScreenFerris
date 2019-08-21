namespace WpfDemo
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Windows.Controls;
    using System.Windows.Forms;

    /// <summary>
    /// Interaction logic for ManualPage.xaml.
    /// </summary>
    public partial class ManualPage : Page
    {
        private ObservableCollection<string> deviceListFound = new ObservableCollection<string>();

        public ManualPage()
        {
            this.InitializeComponent();
            Dictionary<uint, Display.MonitorInfo> devDic = Display.FindDevNumList();
            foreach (uint devNum in devDic.Keys)
            {
                this.deviceListFound.Add(devDic[devNum].deviceName);
            }

            this.deviceList.ItemsSource = this.deviceListFound;
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
