using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using Networking;

namespace BroMakerLib.Infos
{
    public class CustomBroInfo : CustomCharacterInfo
    {
        protected new string _defaultName = "BRO";
        public CustomBroInfo() : base() { }
        public CustomBroInfo(string name) : base(name) { }

        public string spritePath;
        public string gunSpritePath;
        [JsonIgnore]
        public Vector2 gunSpriteOffset = Vector2.zero;

        public override void Initialize()
        {
            base.Initialize();
        }

        public override string SerializeJSON()
        {
            return SerializeJSON(DirectoriesManager.BrosDirectory);
        }

        [AllowedRPC]
        public override void ReadParameters(object obj)
        {
            base.ReadParameters(obj);
        }
    }
}
