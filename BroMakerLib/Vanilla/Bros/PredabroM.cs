using System;
using System.Collections.Generic;
using BroMakerLib.CustomObjects;
using BroMakerLib.Infos;
using BroMakerLib.Loggers;
using RocketLib;
using UnityEngine;

namespace BroMakerLib.Vanilla.Bros
{
    [HeroPreset("Brodator", HeroType.Predabro)]
    public class PredabroM : Predabro, ICustomHero
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
            var bro = HeroController.GetHeroPrefab(HeroType.Predabro).As<Predabro>();
            stealthMaterial = bro.stealthMaterial;
            stealthGunMaterial = bro.stealthGunMaterial;
            targetPrefab = bro.targetPrefab;
            shoulderCannonBaseSprite = this.FindChildOfName("ShoulderCanon Base").GetComponent<SpriteSM>();
            shoulderCannonSprite = shoulderCannonBaseSprite.FindChildOfName("ShoulderCanon").transform;
            shoulderCannonBullet = bro.shoulderCannonBullet;

            var temp = new List<SpriteSM>();
            Transform laserSightParent = null;
            for (int i = 0; i < shoulderCannonSprite.transform.childCount; i++)
            {
                var child = shoulderCannonSprite.transform.GetChild(i);
                if (child.gameObject.name == "Lazer")
                    temp.Add(child.GetComponent<SpriteSM>());
                else
                    laserSightParent = child.transform;
            }
            laserSightLazers = temp.ToArray();

            var temp2 = new List<Transform>();
            for (int i = 0; i < laserSightParent.childCount; i++)
            {
                temp2.Add(laserSightParent.GetChild(i).transform);
            }
            laserSights = temp2.ToArray();

            cloakingSound = bro.cloakingSound;
            selfDestructSoundSound = bro.selfDestructSoundSound;
            outlineSprite = this.FindChildOfName("StealthOutline").GetComponent<ReplicateSprite>();
            searchRing = this.FindChildOfName("Search Ring").GetComponent<SpriteSM>();
            giltchTextures = bro.giltchTextures;
            hudSpecialCountDownMaterials = bro.hudSpecialCountDownMaterials;
            heldSpearSprite = this.FindChildOfName("Gun").FindChildOfName("PullBackFrame").GetComponent<SpriteSM>();
            heldSpearSpriteCharged = this.FindChildOfName("Gun").FindChildOfName("PullBackFrameCharged").GetComponent<SpriteSM>();
            chargedSpearProectilePrefab = bro.chargedSpearProectilePrefab;
            predabroSymbolPrefab = bro.predabroSymbolPrefab;
        }
    }
}
