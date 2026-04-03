using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using BroMakerLib.Loaders;
using Newtonsoft.Json;
using UnityEngine;

namespace BroMakerLib.Vanilla.Specials
{
    [SpecialPreset("Brommando")]
    public class BrommandoSpecial : SpecialAbility
    {
        public string barageProjectileName = "DrunkRocket";
        public float rocketSpeed = 150f;
        public float rocketSpeedY = 0f;
        public int barageCount = 4;
        public float barageInterval = 0.1333f;

        [JsonIgnore]
        protected Projectile barageProjectile;
        [JsonIgnore]
        private float specialX;
        [JsonIgnore]
        private float specialY;
        [JsonIgnore]
        private int barageDirection;
        [JsonIgnore]
        private int currentBarageCount;
        [JsonIgnore]
        private float barageCounter;
        [JsonIgnore]
        private bool firingBarage;

        public BrommandoSpecial()
        {
            animationColumn = 17;
            animationRow = 5;
            animationFrameCount = 8;
            triggerFrame = 4;
            spawnOffsetX = 16f;
            spawnOffsetY = 6.5f;
        }

        public override void Initialize(TestVanDammeAnim owner)
        {
            base.Initialize(owner);
            barageProjectile = LoadBroforceObjects.GetProjectileFromName(barageProjectileName);
            if (attackSounds == null)
            {
                var prefab = HeroController.GetHeroPrefab(HeroType.Brommando);
                var sourceBro = prefab.GetComponent<TestVanDammeAnim>();
                attackSounds = sourceBro.soundHolder.attackSounds;
            }
        }

        public override void UseSpecial()
        {
            if (owner.SpecialAmmo > 0)
            {
                owner.SpecialAmmo--;
                HeroController.SetSpecialAmmo(PlayerNum, owner.SpecialAmmo);
                if (owner.IsMine)
                {
                    specialX = X + Direction * spawnOffsetX;
                    specialY = Y + spawnOffsetY;
                    barageDirection = (int)Mathf.Sign(owner.transform.localScale.x);
                    ProjectileController.SpawnProjectileOverNetwork(barageProjectile, owner, specialX, specialY, barageDirection * rocketSpeed, rocketSpeedY, false, PlayerNum, false, false, 0f);
                    Sound.GetInstance().PlaySoundEffectAt(attackSounds, 0.3f, owner.transform.position, 1f + owner.pitchShiftAmount);
                    firingBarage = true;
                    barageCounter = barageInterval;
                    currentBarageCount = barageCount;
                }
            }
            else
            {
                HeroController.FlashSpecialAmmo(PlayerNum);
                owner.gunSprite.gameObject.SetActive(true);
            }
            hero.PressSpecialFacingDirection = 0;
        }

        public override void Update()
        {
            if (firingBarage)
            {
                barageCounter -= hero.DeltaTime;
                if (barageCounter <= 0f)
                {
                    barageCounter = barageInterval;
                    currentBarageCount--;
                    if (currentBarageCount >= 0)
                    {
                        ProjectileController.SpawnProjectileOverNetwork(barageProjectile, owner, specialX, specialY, barageDirection * rocketSpeed, rocketSpeedY, false, PlayerNum, false, false, 0f);
                        Sound.GetInstance().PlaySoundEffectAt(attackSounds, 0.3f, owner.transform.position, 1f + owner.pitchShiftAmount);
                    }
                    else
                    {
                        firingBarage = false;
                    }
                }
            }
        }
    }
}
