using System;
using System.Collections.Generic;
using BroMakerLib.CustomObjects;
using BroMakerLib.Infos;
using BroMakerLib.Loggers;
using UnityEngine;

namespace BroMakerLib.Vanilla.Bros
{
    [HeroPreset("Brominator", HeroType.Brominator)]
    public class BrominatorM : Brominator, ICustomHero
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
            var bro = HeroController.GetHeroPrefab(HeroType.Brominator).As<Brominator>();

            miniGunSoundWindup = bro.miniGunSoundWindup;
            miniGunSoundSpinning = bro.miniGunSoundSpinning;
            miniGunSoundWindDown = bro.miniGunSoundWindDown;
            metalBrominator = bro.metalBrominator;
            humanBrominator = bro.humanBrominator;
            metalGunBrominator = bro.metalGunBrominator;
            humanGunBrominator = bro.metalGunBrominator;
            brominatorHumanAvatar = bro.brominatorHumanAvatar;
            brominatorRobotAvatar = bro.brominatorRobotAvatar;
            bulletShell = bro.bulletShell;
        }
    }
}
