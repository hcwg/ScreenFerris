using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfDemo
{
    /// <summary>
    /// Interaction logic for HomeWindow.xaml
    /// </summary>
    public partial class HomeWindow : Window
    {
        private App app;
        public HomeWindow(App app)
        {
            this.app = app;
            InitializeComponent();
            SensorsFrame.Content = new SensorsPage(app);
            ManualFrame.Content = new ManualPage();
            AddSensorFrame.Content = new AddSensorPage(app, this);
        }
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            app.SaveSettings();
        }
        private void BtnQuit_Click(object sender, RoutedEventArgs e)
        {
            app.Shutdown();
        }

        protected override void OnClosed(EventArgs e)
        {
            app.SetHomeWindowNull();
            base.OnClosed(e);
        }

      
    }
}
