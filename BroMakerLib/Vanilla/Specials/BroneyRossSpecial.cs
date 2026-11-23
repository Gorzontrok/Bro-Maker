using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using Newtonsoft.Json;
using UnityEngine;

namespace BroMakerLib.Vanilla.Specials
{
    /// <summary>Broney Ross's sticky-grenade burst special. The gun is always hidden during the throw, so `deactivateGun` has no effect.</summary>
    [SpecialPreset("BroneyRoss")]
    public class BroneyRossSpecial : SpecialAbility
    {
        protected override HeroType SourceBroType => HeroType.BroneyRoss;

        /// <summary>Grenade thrown during the special at three escalating hardcoded speeds, any throw past the third reusing the fastest, so only `name`, `damage`, `sprite` and `offsetX`/`offsetY` apply and `grenadeCount` sets how many are thrown.</summary>
        public GrenadeConfig grenade = new GrenadeConfig
        {
            name = "Sticky",
            offsetX = 8f,
            offsetY = 8f
        };

        /// <summary>Number of grenades thrown per special activation, or one when `instantUse` is set.</summary>
        public int grenadeCount = 3;

        [JsonIgnore] private int grenadesThrown;

        public BroneyRossSpecial()
        {
            animationColumn = 17;
            animationRow = 5;
            animationFrameCount = 8;
            triggerFrame = 4;
        }

        public override void Initialize(BroBase owner)
        {
            base.Initialize(owner);
            grenade.Initialize(owner);
        }

        public override void AnimateSpecial()
        {
            hero.SetSpriteOffset(0f, 0f);
            hero.DeactivateGun();
            hero.FrameRate = frameRate;
            int column = animationColumn + Mathf.Clamp(owner.frame, 0, animationFrameCount - 1);
            hero.Sprite.SetLowerLeftPixel(column * hero.SpritePixelWidth, animationRow * hero.SpritePixelHeight);
            if (IsTriggerFrame(triggerFrame))
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
                PlayThrowLightSound(0.4f);
                if (instantUse || grenadesThrown == grenadeCount - 1)
                {
                    owner.SpecialAmmo--;
                }
                if (!instantUse && grenadesThrown >= grenadeCount)
                {
                    return;
                }
                if (owner.IsMine)
                {
                    Grenade spawned = null;
                    float x = X + (Direction * grenade.offsetX);
                    float y = Y + grenade.offsetY;
                    int throwIndex = Mathf.Min(grenadesThrown, 2);
                    if (throwIndex == 2)
                    {
                        spawned = ProjectileController.SpawnGrenadeOverNetwork(grenade.basePrefab, owner, x, y, 0.001f, 0.011f, Direction * 200f, 155f, PlayerNum, 1f);
                    }
                    if (throwIndex == 1)
                    {
                        spawned = ProjectileController.SpawnGrenadeOverNetwork(grenade.basePrefab, owner, x, y, 0.001f, 0.011f, Direction * 160f, 130f, PlayerNum, 1f);
                    }
                    if (throwIndex == 0)
                    {
                        spawned = ProjectileController.SpawnGrenadeOverNetwork(grenade.basePrefab, owner, x, y, 0.001f, 0.011f, Direction * 115f, 105f, PlayerNum, 1f);
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
