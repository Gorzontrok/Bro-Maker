using BroMakerLib.CustomObjects;
using BroMakerLib.Infos;
using BroMakerLib.Loaders;
using BroMakerLib.Loggers;
using System;
using UnityEngine;
using System.Collections.Generic;

namespace BroMakerLib.Vanilla.Bros
{
    [HeroPreset("BaBroracus", HeroType.BaBroracus)]
    public class BaBroracusM : BaBroracus, ICustomHero
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
            var bro = HeroController.GetHeroPrefab(HeroType.BaBroracus).As<BaBroracus>();
            flameSource = GetComponent<AudioSource>();
            flameSound = bro.flameSound;
            flameSoundEnd = bro.flameSoundEnd;
            projectile2 = bro.projectile2;
            projectile3 = bro.projectile3;
        }
    }
}
