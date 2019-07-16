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
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfDemo.Models;

namespace WpfDemo.UserControls
{
    /// <summary>
    /// Interaction logic for SensorItem.xaml
    /// </summary>
    public partial class SensorItem : UserControl
    {
        private SensorDisplay sensor;

        public static DependencyProperty SeonsorProperty;
        public static DependencyProperty DNameProperty;

        static SensorItem()
        {
            SeonsorProperty = DependencyProperty.Register("Sensor", typeof(SensorDisplay), typeof(SensorItem));
            DNameProperty = DependencyProperty.Register("DName", typeof(String), typeof(SensorItem));
        }
        public SensorItem()
        {
            InitializeComponent();
        }
        public SensorDisplay Sensor
        {
            get { return sensor; }
            set
            {
                sensor = value;
                Console.Write("FUCCCK" + sensor.DeviceName);
                //sensorNameLabel.DataContext = sensor;
            }
        }

        public String DName
        {
            set
            {
                SetValue(DNameProperty, value);
                Console.WriteLine("FUCCCK" + value);
            }
        }

    }
}
