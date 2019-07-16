using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace WpfDemo
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private MainWindow mainWindow;
        private DiscoverSensorsWindow discoverSensorsWindow;

        SFConfigStore configStore;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            configStore = new SFConfigStore();

            mainWindow = new MainWindow(this, configStore);
            discoverSensorsWindow = new DiscoverSensorsWindow();

            mainWindow.Show();

        }
    }
}
