using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfDemo.Models
{
    public class ShortcutBinding : INotifyPropertyChanged
    {
        private string shortcut;
        private bool enabled;
        private bool isActive;
        public string Shortcut
        {
            get => shortcut;
            set
            {
                if (value != shortcut)
                {
                    shortcut = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Shortcut"));
                }
            }
        }
        public bool Enabled
        {
            get => enabled;
            set
            {
                if (value != enabled)
                {
                    enabled = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Enabled"));

                }

            }
        }

        public bool IsActive
        {
            get => isActive;
            set
            {
                if (value != isActive)
                {
                    isActive = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("isActive"));
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
