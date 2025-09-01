using System;
using System.Collections.Generic;
using BroMakerLib.CustomObjects;
using BroMakerLib.Infos;
using BroMakerLib.Loggers;
using RocketLib;
using UnityEngine;

namespace BroMakerLib.Vanilla.Bros
{
    [HeroPreset("IndianaBrones", HeroType.IndianaBrones)]
    public class IndianaBronesM : IndianaBrones, ICustomHero
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
            var bro = HeroController.GetHeroPrefab(HeroType.IndianaBrones).As<IndianaBrones>();
            whipHitPuff = bro.whipHitPuff;
            whipSprite = this.FindChildOfName("Whip").GetComponent<SpriteSM>();
            materialArmless = bro.materialArmless;
            ropeStretchSound = bro.ropeStretchSound;
        }
    }
}
