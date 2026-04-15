using BroMakerLib.Attributes;
using BroMakerLib.Extensions;
using UnityEngine;

namespace BroMakerLib.Vanilla.Passives
{
    /// <summary>BroLee's airdash.</summary>
    [PassivePreset("BroLee")]
    public class BroLeePassive : AirdashPassive
    {
        protected override HeroType SourceBroType => HeroType.BroLee;

        protected override bool IsOwnerRedundant(TestVanDammeAnim owner) => owner is BroLee;

        /// <summary>Fraction of `airdashMaxTime` used for upward dash duration.</summary>
        public float upDashTimeMultiplier = 0.5f;

        /// <summary>Disable for bros whose sprite sheet doesn't match BroLee's airdash layout.</summary>
        public bool enableCustomAirdashAnimation = true;
        public int upDashColumn = 1;
        public int downDashColumn = 2;
        public int horizontalDashColumn = 0;
        public int horizontalDashWindupColumn = 18;
        /// <summary>Vertical velocity threshold below which the down-dash shows the stomp frame instead of the jump frame.</summary>
        public float downDashSlowThreshold = -50f;

        /// <summary>Airdash ignition sound played at 0.5 volume.</summary>
        public AudioClip[] specialAttackSounds;

        public BroLeePassive()
        {
            defaultAirdashDelay = 0.075f;
            airdashMaxTime = 0.225f;
            animationRow = 6;
        }

        protected override void CacheSoundsFromPrefab()
        {
            var sourceBro = HeroController.GetHeroPrefab(SourceBroType) as BroLee;
            if (sourceBro == null) return;
            if (specialAttackSounds == null) specialAttackSounds = sourceBro.soundHolder.specialAttackSounds.CloneArray();
        }

        public override bool HandlePlayAidDashSound()
        {
            if (specialAttackSounds != null && specialAttackSounds.Length > 0)
                sound.PlaySoundEffectAt(specialAttackSounds, 0.5f, owner.transform.position, 1f, true, false, false, 0f);
            return false;
        }

        public override void HandleAfterAirDashUp()
        {
            owner.SetFieldValue("airdashTime", owner.airdashMaxTime * upDashTimeMultiplier);
        }

        public override bool HandleAnimateAirdash()
        {
            if (!enableCustomAirdashAnimation) return true;

            DirectionEnum direction = hero.AirdashDirection;
            switch (direction)
            {
                case DirectionEnum.Up:
                    hero.DeactivateGun();
                    hero.Sprite.SetLowerLeftPixel((float)(upDashColumn * hero.SpritePixelWidth), (float)(animationRow * hero.SpritePixelHeight));
                    return false;
                case DirectionEnum.Down:
                    if (owner.yI > downDashSlowThreshold)
                    {
                        hero.AnimateJumping();
                    }
                    else
                    {
                        hero.DeactivateGun();
                        hero.Sprite.SetLowerLeftPixel((float)(downDashColumn * hero.SpritePixelWidth), (float)(animationRow * hero.SpritePixelHeight));
                    }
                    return false;
                case DirectionEnum.Left:
                case DirectionEnum.Right:
                    hero.DeactivateGun();
                    float airDashDelay = owner.GetFieldValue<float>("airDashDelay");
                    int col = airDashDelay > 0f ? horizontalDashWindupColumn : horizontalDashColumn;
                    hero.Sprite.SetLowerLeftPixel((float)(col * hero.SpritePixelWidth), (float)(animationRow * hero.SpritePixelHeight));
                    return false;
                default:
                    return true;
            }
        }
    }
}
