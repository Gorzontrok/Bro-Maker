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
    [HeroPreset("Broden", HeroType.Broden)]
    public class BrodenM : Broden, ICustomHero
    {
        public CustomBroInfo info { get; set; }
        public BroBase character { get; set; }

        public List<Material> specialMaterials { get; set; } = new List<Material>();
        public Vector2 specialMaterialOffset { get; set; } = Vector2.zero;
        public float specialMaterialSpacing { get; set; } = 0f;
        public Material firstAvatar { get; set; } = null;
        public Vector2 gunSpriteOffset { get; set; } = Vector2.zero;
        public MuscleTempleFlexEffect flexEffect { get; set; }

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
            //var bro = HeroController.GetHeroPrefab(HeroType.Broden).As<Broden>();
            zapper = this.FindChildOfName("ElectricZap").GetComponent<ElectricZap>();
        }
    }
}
