using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using BroMakerLib.Loaders;
using BroMakerLib.Extensions;
using Newtonsoft.Json;
using UnityEngine;

namespace BroMakerLib.Vanilla.Specials
{
    /// <summary>Broney Ross's sticky-grenade burst special.</summary>
    [SpecialPreset("BroneyRoss")]
    public class BroneyRossSpecial : SpecialAbility
    {
        protected override HeroType SourceBroType => HeroType.BroneyRoss;
        /// <summary>Name of the grenade prefab thrown during the special.</summary>
        public string grenadeName = "Sticky";
        /// <summary>Number of grenades thrown per special activation.</summary>
        public int grenadeCount = 3;

        [JsonIgnore]
        protected Grenade grenade;
        [JsonIgnore]
        private int grenadesThrown;

        public BroneyRossSpecial()
        {
            animationColumn = 17;
            animationRow = 5;
            animationFrameCount = 8;
            triggerFrame = 4;
            spawnOffsetX = 8f;
            spawnOffsetY = 8f;
        }

        public override void Initialize(BroBase owner)
        {
            base.Initialize(owner);
            grenade = LoadBroforceObjects.GetGrenadeFromName(grenadeName);
        }

        public override void AnimateSpecial()
        {
            hero.SetSpriteOffset(0f, 0f);
            hero.DeactivateGun();
            hero.FrameRate = frameRate;
            int column = animationColumn + Mathf.Clamp(owner.frame, 0, animationFrameCount - 1);
            hero.Sprite.SetLowerLeftPixel(column * hero.SpritePixelWidth, animationRow * hero.SpritePixelHeight);
            if (owner.frame == triggerFrame)
            {
                UseSpecial();
                grenadesThrown++;
                if (grenadesThrown < grenadeCount)
                {
                    owner.frame = 1;
                }
            }
            if (owner.frame >= animationFrameCount - 1)
            {
                owner.frame = 0;
                hero.UsingSpecial = false;
                hero.ActivateGun();
                hero.ChangeFrame();
                grenadesThrown = 0;
            }
        }

        public override void UseSpecial()
        {
            if (owner.SpecialAmmo > 0)
            {
                Sound.GetInstance().PlaySoundEffectAt(throwSounds, 0.4f, owner.transform.position, 1f + owner.pitchShiftAmount);
                if (grenadesThrown == 2)
                {
                    owner.SpecialAmmo--;
                }
                if (owner.IsMine)
                {
                    Grenade spawned = null;
                    float x = X + Direction * spawnOffsetX;
                    float y = Y + spawnOffsetY;
                    if (grenadesThrown == 2)
                    {
                        spawned = ProjectileController.SpawnGrenadeOverNetwork(grenade, owner, x, y, 0.001f, 0.011f, Direction * 200f, 155f, PlayerNum, 1f);
                    }
                    if (grenadesThrown == 1)
                    {
                        spawned = ProjectileController.SpawnGrenadeOverNetwork(grenade, owner, x, y, 0.001f, 0.011f, Direction * 160f, 130f, PlayerNum, 1f);
                    }
                    if (grenadesThrown == 0)
                    {
                        spawned = ProjectileController.SpawnGrenadeOverNetwork(grenade, owner, x, y, 0.001f, 0.011f, Direction * 115f, 105f, PlayerNum, 1f);
                    }
                    if (spawned != null)
                    {
                        GrenadeSticky component = spawned.GetComponent<GrenadeSticky>();
                        if (component != null)
                        {
                            component.stickGrenadeSwarmIndex = grenadesThrown;
                        }
                    }
                }
            }
            else
            {
                HeroController.FlashSpecialAmmo(PlayerNum);
                hero.ActivateGun();
            }
        }
    }
}
