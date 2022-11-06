using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DojoManagerGui
{
    public class Config : INotifyPropertyChanged
    {
        const string ConfigFileName = "Config.json";
        private static Config? instance;

        public event PropertyChangedEventHandler? PropertyChanged;
        public static Config Instance
        {
            get
            {
                if (instance == null)
                    instance = Load();
                return instance;
            }
            private set => instance = value;
        }


        public string NomeAssociazione { get; set; } = "Ken Sei Dojo";
        public string DbName => NomeAssociazione.Replace(' ', '_') + "_db";
        public string[] SuggerimentiAssociazioni { get; set; }
        public string[] SuggerimentiSottoscrizioni { get; set; }
        public string[] SuggerimentiTipiSocio { get; set; }
        public string DbLocation { get; set; }

        public Config()
        {
            DbLocation = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            SuggerimentiAssociazioni = new string[] { NomeAssociazione, "CIK" };
            SuggerimentiSottoscrizioni = new string[] {
                "Ken Sei Dojo - quota associativa (aaaa/aaaa)",
                "CIK - quota associativa (aaaa/aaaa)" };
            SuggerimentiTipiSocio = new string[] { "Fondatore", "Onorario", "Ordinario" };
        }

        public static Config Load()
        {
            if (File.Exists(ConfigFileName))
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigFileName));
            }
            else
            {
                var conf = new Config();
                File.WriteAllText(ConfigFileName, JsonConvert.SerializeObject(conf, Formatting.Indented));
                return conf;
            }
        }

    }
}
