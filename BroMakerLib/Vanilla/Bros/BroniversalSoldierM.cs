using System;
using System.Collections.Generic;
using BroMakerLib.CustomObjects;
using BroMakerLib.Infos;
using BroMakerLib.Loggers;
using RocketLib;
using UnityEngine;

namespace BroMakerLib.Vanilla.Bros
{
    [HeroPreset("BroniversalSoldier", HeroType.BroniversalSoldier)]
    public class BroniversalSoldierM : BroniversalSoldier, ICustomHero
    {
        public CustomBroInfo info { get; set; }
        public BroBase character { get; set; }
        public MuscleTempleFlexEffect flexEffect { get; set; }
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
            var bro = HeroController.GetHeroPrefab(HeroType.BroniversalSoldier).As<BroniversalSoldier>();
            bulletShell = bro.bulletShell;
#pragma warning disable
            serumParticles = this.FindChildOfName("SerumParticles").GetComponent<ParticleEmitter>();
#pragma warning restore
            reviveBlastPrefab = bro.reviveBlastPrefab;
            reviveClips = bro.reviveClips;
        }
    }
}
