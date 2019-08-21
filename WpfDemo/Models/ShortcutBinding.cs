namespace WpfDemo.Models
{
    using System.ComponentModel;

    public class ShortcutBinding : INotifyPropertyChanged
    {
        private string shortcut;
        private bool enabled;
        private bool isActive;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Shortcut
        {
            get => this.shortcut;
            set
            {
                if (value != this.shortcut)
                {
                    this.shortcut = value;
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Shortcut"));
                }
            }
        }

        public bool Enabled
        {
            get => this.enabled;
            set
            {
                if (value != this.enabled)
                {
                    this.enabled = value;
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Enabled"));

                }

            }
        }

        public bool IsActive
        {
            get => this.isActive;
            set
            {
                if (value != this.isActive)
                {
                    this.isActive = value;
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("isActive"));
                }
            }
        }
    }
}
