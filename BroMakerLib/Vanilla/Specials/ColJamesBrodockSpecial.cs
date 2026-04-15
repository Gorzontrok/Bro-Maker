using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using BroMakerLib.Loaders;
using BroMakerLib.Extensions;
using Newtonsoft.Json;
using UnityEngine;

namespace BroMakerLib.Vanilla.Specials
{
    /// <summary>Colonel James Brodock's cluster-grenade launch special.</summary>
    [SpecialPreset("ColonelJamesBroddock")]
    public class ColJamesBrodockSpecial : SpecialAbility
    {
        protected override HeroType SourceBroType => HeroType.ColJamesBroddock;
        /// <summary>Name of the grenade prefab launched during the special.</summary>
        public string grenadeName = "Cluster";
        /// <summary>Horizontal launch speed for a standard (non-ducking) throw.</summary>
        public float shootSpeedX = 250f;
        /// <summary>Vertical launch speed for a standard (non-ducking) throw.</summary>
        public float shootSpeedY = 60f;
        /// <summary>Multiplier applied to `shootSpeedX` when throwing while ducking.</summary>
        public float duckingSpeedMultiplier = 0.3f;
        /// <summary>Vertical launch speed used when throwing while ducking.</summary>
        public float duckingSpeedY = 25f;
        /// <summary>Fraction of the bro's current horizontal velocity added to the throw speed.</summary>
        public float momentumX = 0.45f;
        /// <summary>Fraction of the bro's current upward velocity added to the throw speed.</summary>
        public float momentumY = 0.3f;
        /// <summary>Volume of the throw attack sound.</summary>
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
            hero.FrameRate = frameRate;
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
