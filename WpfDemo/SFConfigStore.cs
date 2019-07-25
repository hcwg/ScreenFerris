using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfDemo
{
    public class SFConfigStore : INotifyPropertyChanged
    {
        public SFSettings settings;
        public bool Modified
        {
            get => modified; set
            {
                if (value != modified)
                {
                    modified = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Modified"));
                }
            }
        }
        private bool modified;
        private readonly string configurFilePath;


        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            //ContractResolver = new SettingsReaderContractResolver(),
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
            settings = Load(this.configurFilePath) as SFSettings;
            Modified = false;
            SettingsSanityCheck();
        }

        void SettingsSanityCheck()
        {
            if (settings == null)
            {
                settings = new SFSettings();
            }
            if (settings.sensors == null)
            {
                settings.sensors = new List<BLEGravitySensorConfig>();
            }
        }

        object Load(string filePath)
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
            return JsonConvert.SerializeObject(settings, Formatting.Indented);
        }

        public void Save()
        {
            File.WriteAllText(configurFilePath, Serialize());
            Modified = false;
        }

    }
}
