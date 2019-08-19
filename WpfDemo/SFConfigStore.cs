namespace WpfDemo
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using Newtonsoft.Json;

    public class SFConfigStore : INotifyPropertyChanged
    {
        public SFSettings settings;

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

        private readonly string configurFilePath;
        private bool modified;

        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            // ContractResolver = new SettingsReaderContractResolver(),
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        public event PropertyChangedEventHandler PropertyChanged;

        public SFConfigStore(String configurFilePath = null)
        {
            if (configurFilePath == null)
            {
                configurFilePath = "SFSettings.json";
            }

            this.configurFilePath = configurFilePath;
            this.settings = this.Load(this.configurFilePath) as SFSettings;
            this.Modified = false;
            this.SettingsSanityCheck();
        }

        private void SettingsSanityCheck()
        {
            if (this.settings == null)
            {
                this.settings = new SFSettings();
            }

            if (this.settings.sensors == null)
            {
                this.settings.sensors = new List<BLEGravitySensorConfig>();
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

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this.settings, Formatting.Indented);
        }

        public void Save()
        {
            File.WriteAllText(this.configurFilePath, this.Serialize());
            this.Modified = false;
        }

    }
}
