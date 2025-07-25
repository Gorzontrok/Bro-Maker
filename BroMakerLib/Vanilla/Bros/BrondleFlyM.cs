using BroMakerLib.CustomObjects;
using BroMakerLib.Infos;
using BroMakerLib.Loaders;
using BroMakerLib.Loggers;
using RocketLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

namespace BroMakerLib.Vanilla.Bros
{
    [HeroPreset("SethBrondle")]
    public class BrondleFlyM : BrondleFly, ICustomHero
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
            var bro = HeroController.GetHeroPrefab(HeroType.BrondleFly).As<BrondleFly>();

            bulletShell = bro.bulletShell;
            teleportOutAnimation = bro.teleportOutAnimation;
            teleportInAnimation = bro.teleportInAnimation;
            teleportSound = bro.teleportSound;
            coveredInBloodMaterial = bro.coveredInBloodMaterial;
            openMouthBloodyMaterial = bro.openMouthBloodyMaterial;
            openMouthMaterial = bro.openMouthMaterial;
            hoverClip = bro.hoverClip;
            _vomitParticles = GetComponentInChildren<ParticleSystem>();
        }
    }
}
