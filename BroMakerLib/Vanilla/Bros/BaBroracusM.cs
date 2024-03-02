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
            var bro = HeroController.GetHeroPrefab(HeroType.BaBroracus).As<BaBroracus>();
            flameSource = GetComponent<AudioSource>();
            flameSound = bro.flameSound;
            flameSoundEnd = bro.flameSoundEnd;
            projectile2 = bro.projectile2;
            projectile3 = bro.projectile3;
        }
    }
}
