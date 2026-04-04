using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using BroMakerLib.Loaders;
using Newtonsoft.Json;
using UnityEngine;

namespace BroMakerLib.Vanilla.Specials
{
    [SpecialPreset("BroMax")]
    public class BroMaxSpecial : SpecialAbility
    {
        public string boomerangName = "Boomerang";
        public float boomerangSpeed = 240f;
        public float boomerangSpeedY = 0f;
        public float catchFrameRate = 0.045f;

        [JsonIgnore]
        protected Projectile boomerang;
        [JsonIgnore]
        protected bool grabbingBoomerang;
        [JsonIgnore]
        protected int grabbingFrame;

        public BroMaxSpecial()
        {
            animationColumn = 17;
            animationRow = 5;
            animationFrameCount = 8;
            triggerFrame = 4;
            spawnOffsetX = 6f;
            spawnOffsetY = 15f;
        }

        public override void Initialize(TestVanDammeAnim owner)
        {
            base.Initialize(owner);
            boomerang = LoadBroforceObjects.GetProjectileFromName(boomerangName);
            if (throwSounds == null) throwSounds = owner.soundHolder.throwSounds;
        }

        public override void PressSpecial()
        {
            if (owner.SpecialAmmo > 0)
            {
                grabbingBoomerang = false;
            }
            base.PressSpecial();
        }

        public override void AnimateSpecial()
        {
            if (grabbingBoomerang)
            {
                grabbingFrame--;
                hero.SetSpriteOffset(0f, 0f);
                hero.DeactivateGun();
                hero.FrameRate = catchFrameRate;
                int column = animationColumn + Mathf.Clamp(grabbingFrame, 0, animationFrameCount - 1);
                hero.Sprite.SetLowerLeftPixel(column * hero.SpritePixelWidth, animationRow * hero.SpritePixelHeight);
                if (grabbingFrame <= 0)
                {
                    owner.frame = 0;
                    hero.UsingSpecial = false;
                    grabbingBoomerang = false;
                }
            }
            else
            {
                base.AnimateSpecial();
            }
        }

        public override void UseSpecial()
        {
            if (owner.SpecialAmmo > 0)
            {
                Sound.GetInstance().PlaySoundEffectAt(throwSounds, 0.4f, owner.transform.position, 1f + owner.pitchShiftAmount);
                owner.SpecialAmmo--;
                if (owner.IsMine)
                {
                    ProjectileController.SpawnProjectileOverNetwork(boomerang, owner, X + Direction * spawnOffsetX, Y + spawnOffsetY, Direction * boomerangSpeed, boomerangSpeedY, false, PlayerNum, false, false, 0f);
                }
            }
            else
            {
                HeroController.FlashSpecialAmmo(PlayerNum);
                hero.ActivateGun();
            }
            hero.PressSpecialFacingDirection = 0;
        }

        public void ReturnBoomerang(Boomerang boomerang)
        {
            owner.SpecialAmmo++;
            if (!hero.UsingSpecial)
            {
                hero.UsingSpecial = true;
                grabbingFrame = 4;
                grabbingBoomerang = true;
                hero.ChangeFrame();
            }
        }
    }
}
