using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using BroMakerLib.Loaders;
using Newtonsoft.Json;
using UnityEngine;

namespace BroMakerLib.Vanilla.Specials
{
    [SpecialPreset("ColJamesBrodock")]
    public class ColJamesBrodockSpecial : SpecialAbility
    {
        public string grenadeName = "Cluster";
        public float shootSpeedX = 250f;
        public float shootSpeedY = 60f;
        public float duckingSpeedMultiplier = 0.3f;
        public float duckingSpeedY = 25f;
        public float momentumX = 0.45f;
        public float momentumY = 0.3f;
        public float attackSoundVolume = 0.4f;

        [JsonIgnore]
        protected Grenade grenade;

        public ColJamesBrodockSpecial()
        {
            spawnOffsetX = 6f;
            spawnOffsetY = 10f;
        }

        public override void Initialize(TestVanDammeAnim owner)
        {
            base.Initialize(owner);
            grenade = LoadBroforceObjects.GetGrenadeFromName(grenadeName);
            if (attackSounds == null)
            {
                var prefab = HeroController.GetHeroPrefab(HeroType.ColJamesBroddock);
                var sourceBro = prefab.GetComponent<TestVanDammeAnim>();
                attackSounds = sourceBro.soundHolder.attackSounds;
            }
        }

        public override void PressSpecial()
        {
            if (!owner.hasBeenCoverInAcid && !hero.DoingMelee && owner.SpecialAmmo > 0 && owner.health > 0)
            {
                hero.UsingSpecial = true;
                owner.frame = 0;
                hero.GunFrame = 4;
                hero.ChangeFrame();
                hero.PressSpecialFacingDirection = (int)owner.transform.localScale.x;
            }
        }

        public override void AnimateSpecial()
        {
            hero.SetSpriteOffset(0f, 0f);
            hero.ActivateGun();
            hero.FrameRate = 0.0334f;
            hero.SetGunSprite(5 - owner.frame, 0);
            if (owner.frame == 0)
            {
                UseSpecial();
            }
            if (owner.frame >= 5)
            {
                hero.GunFrame = 0;
                owner.frame = 0;
                hero.UsingSpecial = false;
            }
        }

        public override void UseSpecial()
        {
            if (owner.SpecialAmmo > 0)
            {
                Sound.GetInstance().PlaySoundEffectAt(attackSounds, attackSoundVolume, owner.transform.position);
                owner.SpecialAmmo--;
                if (owner.IsMine)
                {
                    float xMomentum = owner.xI * momentumX;
                    float yMomentum = owner.yI > 0f ? owner.yI * momentumY : 0f;
                    if (hero.Ducking && owner.down)
                    {
                        ProjectileController.SpawnGrenadeOverNetwork(grenade, owner,
                            X + Direction * spawnOffsetX, Y + 7f,
                            0.001f, 0.011f,
                            Direction * shootSpeedX * duckingSpeedMultiplier + xMomentum, duckingSpeedY + yMomentum,
                            PlayerNum, 1f);
                    }
                    else
                    {
                        ProjectileController.SpawnGrenadeOverNetwork(grenade, owner,
                            X + Direction * spawnOffsetX, Y + spawnOffsetY,
                            0.001f, 0.011f,
                            Direction * shootSpeedX + xMomentum, shootSpeedY + yMomentum,
                            PlayerNum, 1f);
                    }
                }
            }
            else
            {
                HeroController.FlashSpecialAmmo(PlayerNum);
                hero.ActivateGun();
            }
            hero.PressSpecialFacingDirection = 0;
        }
    }
}
