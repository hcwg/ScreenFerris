namespace WpfDemo
{
    using System;
    using System.Windows;

    /// <summary>
    /// Interaction logic for HomeWindow.xaml.
    /// </summary>
    public partial class HomeWindow : Window
    {
        private App app;

        public HomeWindow(App app)
        {
            this.app = app;
            this.InitializeComponent();
            this.SensorsFrame.Content = new SensorsPage(app);
            this.ManualFrame.Content = new ManualPage();
            this.AddSensorFrame.Content = new AddSensorPage(app, this);
            this.btnSave.DataContext = app;
        }
        protected override void OnClosed(EventArgs e)
        {
            this.app.SetHomeWindowNull();
            base.OnClosed(e);
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            this.app.SaveSettings();
        }

        private void BtnQuit_Click(object sender, RoutedEventArgs e)
        {
            this.app.Shutdown();
        }
    }
}
