namespace WpfDemo
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using Newtonsoft.Json;

    public class SFConfigStore : INotifyPropertyChanged
    {
        public SFSettings Settings;

        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            // ContractResolver = new SettingsReaderContractResolver(),
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        };

        private readonly string configurFilePath;
        private bool modified;

        public SFConfigStore(string configurFilePath = null)
        {
            if (configurFilePath == null)
            {
                configurFilePath = "SFSettings.json";
            }

            this.configurFilePath = configurFilePath;
            this.Settings = this.Load(this.configurFilePath) as SFSettings;
            this.Modified = false;
            this.SettingsSanityCheck();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool Modified
        {
            get => this.modified; set
            {
                if (value != this.modified)
                {
                    this.modified = value;
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Modified"));
                }
            }
        }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this.Settings, Formatting.Indented);
        }

        public void Save()
        {
            File.WriteAllText(this.configurFilePath, this.Serialize());
            this.Modified = false;
        }

        private void SettingsSanityCheck()
        {
            if (this.Settings == null)
            {
                this.Settings = new SFSettings();
            }

            if (this.Settings.sensors == null)
            {
                this.Settings.sensors = new List<BLEGravitySensorConfig>();
            }
        }

        private object Load(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return Activator.CreateInstance(typeof(SFSettings));
            }

            string jsonFile = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject(jsonFile, typeof(SFSettings), JsonSerializerSettings);
        }
    }
}
