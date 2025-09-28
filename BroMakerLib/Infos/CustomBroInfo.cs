using System.Collections.Generic;
using BroMakerLib.Cutscenes;
using BroMakerLib.Unlocks;
using Newtonsoft.Json;
using UnityEngine;

namespace BroMakerLib.Infos
{
    public class CustomBroInfo : CustomCharacterInfo
    {
        protected new string _defaultName = "BRO";
        public CustomBroInfo() : base() { }
        public CustomBroInfo(string name) : base(name) { }

        [JsonConverter(typeof(CutsceneConverter))]
        public List<CustomIntroCutscene> Cutscene = new List<CustomIntroCutscene> { new CustomIntroCutscene() };

        public BroUnlockConfig UnlockConfig { get; set; } = new BroUnlockConfig();

        [JsonIgnore]
        public List<string> SpritePath = new List<string>();
        [JsonIgnore]
        public List<string> GunSpritePath = new List<string>();
        [JsonIgnore]
        public List<Vector2> GunSpriteOffset = new List<Vector2> { Vector2.zero };
        [JsonIgnore]
        public List<List<Material>> SpecialMaterials = new List<List<Material>> { new List<Material>() };
        [JsonIgnore]
        public List<Vector2> SpecialMaterialOffset = new List<Vector2> { Vector2.zero };
        [JsonIgnore]
        public List<float> SpecialMaterialSpacing = new List<float> { 0f };
        [JsonIgnore]
        public List<Material> FirstAvatar = new List<Material> { null };
        [JsonIgnore]
        public int VariantCount = 1;

        public override string SerializeJSON()
        {
            return SerializeJSON(DirectoriesManager.BrosDirectory);
        }
    }
}
