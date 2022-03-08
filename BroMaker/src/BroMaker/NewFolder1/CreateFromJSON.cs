using System;
using System.Collections.Generic;
using System.IO;
using BroMakerLoadMod;
using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace BroMakerLib
{
   /* internal class CreateBroFromJSON
    {
        private string BrosFolder = string.Empty;
        private string[] Files = new string[] { };
        public void Load()
        {
            try
            {
                this.BrosFolder = Path.Combine(Main.mod.Path, "CustomBro");
                this.Files = GetFiles();

                foreach(string file in Files)
                {
                    string json = File.ReadAllText(file);
                    List<IBro_Info> bros = JsonConvert.DeserializeObject<List<IBro_Info>>(json, new Bro_Converter());
                    foreach (IBro_Info bro in bros)
                    {
                        new CustomBro((Bro_Info)bro);
                    }
                }
            }catch(Exception ex) { Main.Log(ex); }
        }

        private string[] GetFiles()
        {
            List<string> TempList = new List<string>(Directory.GetFiles(BrosFolder, "*.json", SearchOption.TopDirectoryOnly));
            foreach(string file in Directory.GetFiles(BrosFolder, "*.txt", SearchOption.TopDirectoryOnly))
            {
                TempList.Add(file);
            }
            return TempList.ToArray();
        }
    }
    internal class Bro_Converter : CustomCreationConverter<IBro_Info>
    {
        public override IBro_Info Create(Type objectType)
        {
            return new Bro_Info();
        }
    }

    internal interface IBro_Info
    {
        string Name { get; set; }
        string AssetsFolder { get; set; }
        string BaseBro { get; set; }
    }

    public class Bro_Info : IBro_Info
    {
        public string Name { get; set; }
        public string AssetsFolder { get; set; }
        public string BaseBro { get; set; }
    }*/
}
