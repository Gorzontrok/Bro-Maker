using BroMakerLib.CustomObjects;
using BroMakerLib.Infos;
using BroMakerLib.Loaders;
using BroMakerLib.Loggers;
using RocketLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BroMakerLib.Vanilla.Bros
{
    [HeroPreset("Desperabro", HeroType.Desperabro)]
    public class DesperabroM : Desperabro, ICustomHero
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
            mariachiBroType = MariachiBroType.Desperabro;

            var bro = HeroController.GetHeroPrefab(HeroType.Desperabro).As<Desperabro>();

            bulletShell = bro.bulletShell;
            musicParticles = this.FindChildOfName("MusicParticles").GetComponent<ParticleSystem>();
            guitarGunSprite = this.FindChildOfName("Guitar").GetComponent<SpriteSM>();
            // Campa Bro
            campaBroMaterial = bro.campaBroMaterial;
            campaBroGuitarMaterial = bro.campaBroGuitarMaterial;
            campaBroProjectile = bro.campaBroProjectile;
            // Quino bro
            quinoBroGuitarMaterial = bro.quinoBroGuitarMaterial;
            quinoBroMaterial = bro.quinoBroMaterial;
            quinoBroProjectile = bro.quinoBroProjectile;
        }
    }
}
