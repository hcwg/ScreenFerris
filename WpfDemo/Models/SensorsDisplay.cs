using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfDemo.Models
{
    public class SensorDisplay : INotifyPropertyChanged
    {
        public string DeviceID { get; set; }

        public string DeviceName { get; set; }

        public bool Connected { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
