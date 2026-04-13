using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using Networking;
using Newtonsoft.Json;
using RocketLib.Extensions;
using UnityEngine;

namespace BroMakerLib.Vanilla.Specials
{
    [SpecialPreset("TheBrofessional")]
    public class TheBrofessionalSpecial : SpecialAbility
    {
        protected override HeroType SourceBroType => HeroType.TheBrofessional;
        [JsonIgnore]
        protected MatildaTargettingWave matildaWavePrefab;
        [JsonIgnore]
        private bool usedSpecial;

        public TheBrofessionalSpecial()
        {
            spawnOffsetX = 5f;
            spawnOffsetY = 9f;
        }

        public override void Initialize(TestVanDammeAnim owner)
        {
            base.Initialize(owner);
            var prefab = HeroController.GetHeroPrefab(HeroType.TheBrofessional);
            var brofessional = prefab.GetComponent<TheBrofessional>();
            if (brofessional != null)
            {
                matildaWavePrefab = brofessional.matildaTargettingWavePrefab;
            }
        }

        public override void PressSpecial()
        {
            base.PressSpecial();
            usedSpecial = false;
        }

        public override bool HandleMustIgnoreHighFiveMeleePress()
        {
            if (hero.UsingSpecial)
            {
                owner.SetFieldValue("meleeFollowUp", true);
            }
            return true;
        }

        public override void AnimateSpecial()
        {
            hero.SetSpriteOffset(0f, 0f);
            hero.DeactivateGun();
            hero.FrameRate = 0.0334f;
            int column = 25 + Mathf.Clamp(owner.frame, 0, 6);
            hero.Sprite.SetLowerLeftPixel(column * hero.SpritePixelWidth, hero.SpritePixelHeight * 5);
            if (owner.frame == 0)
            {
                owner.counter -= 0.033f;
            }
            if (owner.frame == 4 && !usedSpecial)
            {
                UseSpecial();
                owner.counter -= 0.2f;
            }
            if (owner.frame >= 6)
            {
                if (owner.player != null)
                {
                    owner.player.SetAvatarSpecialFrame(2f);
                }
                owner.frame = 0;
                hero.UsingSpecial = false;
                if (hero.MeleeFollowUp)
                {
                    owner.CallMethod("PressHighFiveMelee", false);
                }
                else
                {
                    hero.ActivateGun();
                    hero.ChangeFrame();
                }
            }
        }

        public override void UseSpecial()
        {
            usedSpecial = true;
            if (owner.SpecialAmmo > 0)
            {
                float spawnY = Y + spawnOffsetY;
                owner.SetFieldValue("ceilingHeight", owner.CalculateCeilingHeight());
                float ceilingHeight = owner.GetFieldValue<float>("ceilingHeight");
                if (ceilingHeight - 20f >= owner.groundHeight + 9f)
                {
                    if (ceilingHeight - 20f < spawnY)
                    {
                        spawnY = ceilingHeight - 20f;
                    }
                }
                owner.SpecialAmmo--;
                FlameWallExplosion wave = global::Networking.Networking.Instantiate<MatildaTargettingWave>(matildaWavePrefab, new Vector3(X + Direction * spawnOffsetX, spawnY, 0f), Quaternion.identity, null, false);
                DirectionEnum dir;
                if (owner.right)
                {
                    dir = DirectionEnum.Right;
                }
                else if (owner.left)
                {
                    dir = DirectionEnum.Left;
                }
                else if (owner.transform.localScale.x > 0f)
                {
                    dir = DirectionEnum.Right;
                }
                else
                {
                    dir = DirectionEnum.Left;
                }
                wave.Setup(PlayerNum, owner, dir);
                hero.PressSpecialFacingDirection = 0;
            }
            else
            {
                if (owner.player != null)
                {
                    owner.player.StopAvatarSpecialFrame();
                }
                HeroController.FlashSpecialAmmo(PlayerNum);
            }
        }
    }
}
