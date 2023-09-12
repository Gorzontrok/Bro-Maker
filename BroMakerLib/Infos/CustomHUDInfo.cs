using BroMakerLib.Stats;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BroMakerLib.Infos
{
    [Obsolete]
    public class CustomHUDInfo : CustomBroforceObjectInfo
    {
        [JsonIgnore, EditorIgnore]
        public new string name = string.Empty;

        protected new string _defaultName = "HUD";
        public CustomHUDInfo() : base() { }
        public CustomHUDInfo(string name) : base(name) { }
    }
}
