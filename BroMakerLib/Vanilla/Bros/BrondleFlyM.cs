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

        protected override void Awake()
        {
            character = this;
            info = LoadHero.currentInfo;
            try
            {
                FixNullVariableLocal();
                this.SetupCustomHero();
                info.BeforeAwake(this);
                base.Awake();
                info.AfterAwake(this);
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
                info.BeforeStart(this);
                this.SetSprites();
                base.Start();
                info.AfterStart(this);
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
