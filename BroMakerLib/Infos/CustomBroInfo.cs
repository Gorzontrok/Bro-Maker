using BroMakerLib.Stats;
using System;
using System.IO;
using Newtonsoft.Json;
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
    }
}
