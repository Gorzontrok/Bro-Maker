using System;
using System.Collections.Generic;
using BroMakerLib.CustomObjects;
using BroMakerLib.Infos;
using BroMakerLib.Loggers;
using UnityEngine;

namespace BroMakerLib.Vanilla.Bros
{
    [HeroPreset("BoondockBro", HeroType.BoondockBros)]
    public class BoondockBroM : BoondockBro, ICustomHero
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
            isLeadBro = true;
            var bro = HeroController.GetHeroPrefab(HeroType.BoondockBros).As<BoondockBro>();
            billyConnollyPrefab = bro.billyConnollyPrefab;
            secondBroMaterial = bro.secondBroMaterial;
            avatar1 = bro.avatar1;
            avatar2 = bro.avatar2;

            // RuntimeUnityEditor friendly
            gameObject.name.Insert(0, "[LEAD] ");
        }
    }
}
