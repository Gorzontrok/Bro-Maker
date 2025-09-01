using System;
using System.Collections.Generic;
using BroMakerLib.CustomObjects;
using BroMakerLib.Infos;
using BroMakerLib.Loggers;
using UnityEngine;

namespace BroMakerLib.Vanilla.Bros
{
    [HeroPreset("Brobocop", HeroType.Brobocop)]
    public class BrobocopM : Brobocop, ICustomHero
    {
        public CustomBroInfo Info { get; set; }
        public BroBase Character { get; set; }
        public MuscleTempleFlexEffect FlexEffect { get; set; }
        public int CurrentVariant { get; set; }
        public Vector2 CurrentGunSpriteOffset { get; set; }
        public List<Material> CurrentSpecialMaterials { get; set; }
        public Vector2 CurrentSpecialMaterialOffset { get; set; }
        public float CurrentSpecialMaterialSpacing { get; set; }
        public Material CurrentFirstAvatar { get; set; }

        protected override void Awake()
        {
            try
            {
                this.StandardBeforeAwake(FixNullVariableLocal);
                base.Awake();
                this.StandardAfterAwake();
            }
            catch (Exception ex)
            {
                BMLogger.ExceptionLog(ex);
                enabled = false;
            }
        }

        protected override void Start()
        {
            try
            {
                this.StandardBeforeStart();
                base.Start();
                this.StandardAfterStart();
            }
            catch (Exception ex)
            {
                BMLogger.ExceptionLog(ex);
                enabled = false;
            }
        }

        protected virtual void FixNullVariableLocal()
        {
            var bro = HeroController.GetHeroPrefab(HeroType.Brobocop).As<Brobocop>();

            targetSystemPrefab = bro.targetSystemPrefab;
            chargingSound = bro.chargingSound;
            finishedChargingSound = bro.finishedChargingSound;
            gunMaterialDefault = bro.gunMaterialDefault;
            gunMaterialOneLight = bro.gunMaterialOneLight;
            gunMaterialOtherLight = bro.gunMaterialOtherLight;
            gunMaterialBothLights = bro.gunMaterialBothLights;
        }
    }
}
