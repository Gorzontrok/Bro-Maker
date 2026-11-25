using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using UnityEngine;

namespace BroMakerLib.Vanilla.Specials
{
    /// <summary>
    /// Colonel James Broddock's cluster-grenade launch special. The throw animates the gun sprite, so `animationRow`, `animationColumn` and `deactivateGun` have no effect, and
    /// `instantUse` has none either.
    /// </summary>
    [SpecialPreset("ColonelJamesBroddock")]
    public class ColJamesBrodockSpecial : SpecialAbility
    {
        protected override HeroType SourceBroType => HeroType.ColJamesBroddock;

        /// <summary>Grenade launched during the special, whose ducking throw reads `duckThrowForceX` as an absolute speed that `throwForceX` does not scale.</summary>
        public GrenadeConfig grenade = new GrenadeConfig
        {
            name = "Cluster",
            offsetX = 6f,
            offsetY = 10f,
            throwForceX = 200f,
            throwForceY = 95f,
            duckOffsetX = 6f,
            duckOffsetY = 7f,
            duckThrowForceX = 60f,
            duckThrowForceY = 25f
        };

        /// <summary>Fraction of the bro's current horizontal velocity added to the throw speed.</summary>
        public float momentumX = 0.45f;

        /// <summary>Fraction of the bro's current upward velocity added to the throw speed.</summary>
        public float momentumY = 0.3f;

        /// <summary>Volume of the throw attack sound.</summary>
        public float attackSoundVolume = 0.4f;

        public override void Initialize(BroBase owner)
        {
            base.Initialize(owner);
            grenade.Initialize(owner);
        }

        public override void PressSpecial()
        {
            if (!owner.hasBeenCoverInAcid && !hero.DoingMelee && owner.SpecialAmmo > 0 && owner.health > 0)
            {
                hero.UsingSpecial = true;
                owner.frame = 0;
                ResetTriggerFrames();
                hero.GunFrame = pressGunFrame;
                hero.ChangeFrame();
                hero.PressSpecialFacingDirection = (int)owner.transform.localScale.x;
            }
        }

        /// <summary>Gun sprite frame set when the special is pressed, on the same press that starts the throw animation.</summary>
        public int pressGunFrame = 4;

        /// <summary>Gun sprite column the throw animation counts down from, one column per frame.</summary>
        public int throwGunColumn = 5;

        /// <summary>Gun sprite row used by the throw animation.</summary>
        public int throwGunRow = 0;

        public ColJamesBrodockSpecial()
        {
            animationFrameCount = 6;
            triggerFrame = 0;
            frameRate = 0.0334f;
        }

        public override void AnimateSpecial()
        {
            hero.SetSpriteOffset(0f, 0f);
            hero.ActivateGun();
            hero.FrameRate = frameRate;
            hero.SetGunSprite(throwGunColumn - owner.frame, throwGunRow);
            if (IsTriggerFrame(triggerFrame))
            {
                UseSpecial();
            }
            if (owner.frame >= animationFrameCount - 1)
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
                sound.PlaySoundEffectAt(attackSounds, attackSoundVolume, owner.transform.position, 1f + owner.pitchShiftAmount, true, false, false, 0f);
                owner.SpecialAmmo--;
                if (owner.IsMine)
                {
                    float xMomentum = owner.xI * momentumX;
                    float yMomentum = owner.yI > 0f ? owner.yI * momentumY : 0f;
                    bool ducking = hero.Ducking && owner.down;
                    float offsetX = ducking ? grenade.duckOffsetX : grenade.offsetX;
                    float offsetY = ducking ? grenade.duckOffsetY : grenade.offsetY;
                    float forceX = ducking ? grenade.duckThrowForceX : grenade.throwForceX;
                    float forceY = ducking ? grenade.duckThrowForceY : grenade.throwForceY;

                    for (int i = 0; i < grenade.count; i++)
                    {
                        Vector2 force = grenade.GetThrowVector(Direction * forceX, forceY, i);
                        ProjectileController.SpawnGrenadeOverNetwork(grenade.basePrefab, owner, X + (Direction * offsetX), Y + offsetY, 0.001f, 0.011f, force.x + xMomentum,
                            force.y + yMomentum, PlayerNum, 1f);
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
