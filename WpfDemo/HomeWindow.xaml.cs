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
        }
        private void BtnManual_Click(object sender, RoutedEventArgs e)
        {
             //TODO: navigate from sensor page
             ManualPage manualPage = new ManualPage();
             this.Content = manualPage;
        }
        private void BtnSensor_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Change to sensor setting page
            SensorsPage sensorsPage = new SensorsPage(app);
            this.Content = sensorsPage;
        }
    }
}
