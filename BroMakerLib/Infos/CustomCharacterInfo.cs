using BroMakerLib.Loaders;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BroMakerLib.Infos
{
    public class CustomCharacterInfo : CustomBroforceObjectInfo
    {
        protected new string _defaultName = "CHARACTER";
        public CustomCharacterInfo() : base() { }
        public CustomCharacterInfo(string name) : base(name) { }

        public string characterPreset = "CustomHero";
    }
}
