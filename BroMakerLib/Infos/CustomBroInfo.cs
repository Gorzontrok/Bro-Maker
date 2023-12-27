using BroMakerLib.Stats;
using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using BroMakerLib;
using BroMakerLib.Loggers;
using Networking;
using Net = Networking.Networking;

namespace BroMakerLib.Infos
{
    public class CustomBroInfo : CustomCharacterInfo
    {
        protected new string _defaultName = "BRO";
        public CustomBroInfo() : base() { }
        public CustomBroInfo(string name) : base(name) { }

        public List<Material> specialMaterials = new List<Material>();

        public string spritePath;
        public string gunSpritePath;
        public float deathGunspriteOffsetX = 0f;
        public float deathGunspriteOffsetY = 0f; 

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
            if (parameters.IsNullOrEmpty()) return;
            var character = obj.As<BroBase>();
            if (GetParameterValue<bool>("Halo"))
            {
                var halo = HeroController.GetHeroPrefab(HeroType.Broffy).halo;

                string haloPreset = GetParameterValue<string>("HaloType");
                if (haloPreset.IsNullOrEmpty())
                    haloPreset = "Halo_Default";

                character.halo = UnityEngine.Object.Instantiate(halo, halo.transform.localPosition, Quaternion.identity);
                character.halo.transform.parent = character.transform;
                Type haloType = null;
                if (PresetManager.customObjectsPreset.TryGetValue(haloPreset, out haloType))
                {
                    character.halo.gameObject.AddComponent(haloType);
                }
                else
                    BMLogger.Log($"Halo preset {haloPreset} not founded.", LogType.Warning);
            }
            if (GetParameterValue<bool>("JetPackSprite"))
            {
                var jetPackSprite = HeroController.GetHeroPrefab(HeroType.Rambro).As<BroBase>().jetPackSprite;
                character.jetPackSprite = UnityEngine.Object.Instantiate(jetPackSprite, jetPackSprite.transform.localPosition, Quaternion.identity);
                character.jetPackSprite.transform.parent = character.transform;
            }
            if (GetParameterValue<bool>("BetterAnimation"))
            {
                character.doRollOnLand = true;
                character.useDashFrames = true;
                character.useNewFrames = true;
                character.useNewKnifingFrames = true;
                character.useNewLedgeGrappleFrames = true;
                character.useNewThrowingFrames = true;
                character.useNewHighFivingFrames = true;
                character.hasNewAirFlexFrames = true;
                character.useNewKnifeClimbingFrames = true;
                character.useDuckingFrames = true;
                character.useNewDuckingFrames = true;
            }
        }
        public void LoadSpecialIcons()
        {
            if (!parameters.IsNullOrEmpty() && parameters.ContainsKey("SpecialIcons") && specialMaterials.Count == 0)
            {
                if (parameters["SpecialIcons"] is JArray)
                {
                    JArray iconFiles = parameters["SpecialIcons"] as JArray;
                    for (int i = 0; i < iconFiles.Count; ++i)
                    {
                        Material specialMat = BroMaker.CreateMaterialFromFile(Path.Combine(path, iconFiles[i].ToObject<string>()));
                        specialMaterials.Add(specialMat);
                    }
                }
                else
                {
                    string iconFile = parameters["SpecialIcons"] as string;
                    Material specialMat = BroMaker.CreateMaterialFromFile(Path.Combine(path, iconFile));
                    specialMaterials.Add(specialMat);
                }
            }
        }

        public void LoadOffset()
        {
            if (!parameters.IsNullOrEmpty())
            {
                if (parameters.ContainsKey("deathGunspriteOffsetX"))
                {
                    this.deathGunspriteOffsetX = float.Parse(parameters["deathGunspriteOffsetX"].ToString());
                }
                if (parameters.ContainsKey("deathGunspriteOffsetY"))
                {
                    this.deathGunspriteOffsetY = float.Parse(parameters["deathGunspriteOffsetY"].ToString());
                }
            }
        }

        public Material LoadAvatar()
        {
            Material result = null;
            if (!parameters.IsNullOrEmpty() && parameters.ContainsKey("Avatar") )
            {
                result = BroMaker.CreateMaterialFromFile(Path.Combine(path, parameters["Avatar"] as string));
            }
            return result;
        }
    }
}
