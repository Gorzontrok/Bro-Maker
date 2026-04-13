using BroMakerLib.Abilities;
using BroMakerLib.Loaders;
using BroMakerLib.Extensions;
using Newtonsoft.Json;
using UnityEngine;

namespace BroMakerLib.Vanilla.Specials
{
    public class GrenadeThrowSpecial : SpecialAbility
    {
        public string grenadeName = "Grenade";
        public bool triggerSpecialEvent = true;

        [JsonIgnore]
        protected Grenade grenade;

        public float throwForceX = 200f;
        public float throwForceY = 150f;

        public float feetOffsetX = 6f;
        public float feetOffsetY = 3f;
        public float feetForceX = 30f;
        public float feetForceY = 70f;

        /// <summary>Vanilla bro to source <see cref="SpecialAbility.throwSounds" /> from. Override in
        /// subclasses to match the bro's own throw grunts (e.g., TankBroSpecial overrides to TankBro).</summary>
        protected override HeroType SourceBroType => HeroType.Rambro;

        public GrenadeThrowSpecial()
        {
            animationColumn = 17;
            animationRow = 5;
            animationFrameCount = 8;
            triggerFrame = 4;
            spawnOffsetX = 8f;
            spawnOffsetY = 8f;
        }

        public override void Initialize(TestVanDammeAnim owner)
        {
            base.Initialize(owner);
            grenade = LoadBroforceObjects.GetGrenadeFromName(grenadeName);
        }

        public override void UseSpecial()
        {
            if (owner.SpecialAmmo > 0 && grenade != null)
            {
                Sound.GetInstance().PlaySoundEffectAt(throwSounds, 0.4f, owner.transform.position, 1f + owner.pitchShiftAmount);
                owner.SpecialAmmo--;
                if (owner.IsMine)
                {
                    if (triggerSpecialEvent)
                    {
                        hero.TriggerBroSpecialEvent();
                    }
                    ActivateSpecial();
                }
            }
            else
            {
                HeroController.FlashSpecialAmmo(PlayerNum);
                hero.ActivateGun();
            }
            hero.PressSpecialFacingDirection = 0;
        }

        public override void ActivateSpecial()
        {
            if (owner.down && owner.IsOnGround() && hero.Ducking)
            {
                ProjectileController.SpawnGrenadeOverNetwork(
                    grenade, owner,
                    X + Direction * feetOffsetX, Y + feetOffsetY,
                    0.001f, 0.011f,
                    Direction * feetForceX, feetForceY,
                    PlayerNum, 1f);
            }
            else
            {
                ProjectileController.SpawnGrenadeOverNetwork(
                    grenade, owner,
                    X + Direction * spawnOffsetX, Y + spawnOffsetY,
                    0.001f, 0.011f,
                    Direction * throwForceX, throwForceY,
                    PlayerNum, 1f);
            }
        }
    }
}
