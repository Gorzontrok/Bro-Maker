using BroMakerLib.Stats;
using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using BroMakerLib;
using BroMakerLib.Loggers;

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

        public override void ReadParameters(object obj)
        {
            base.ReadParameters(obj);
            if (parameters.IsNullOrEmpty()) return;
            var character = obj.As<BroBase>();
            if (GetParameterValue<bool>("Halo"))
            {
                character.halo = HeroController.GetHeroPrefab(HeroType.Broffy).halo;
            }
            if (GetParameterValue<bool>("JetPackSprite"))
            {
                character.jetPackSprite = UnityEngine.Object.Instantiate(HeroController.GetHeroPrefab(HeroType.Rambro).As<BroBase>().jetPackSprite, character.transform);
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
