using BroMakerLib.Abilities;
using BroMakerLib.Loaders;
using BroMakerLib.Extensions;
using Newtonsoft.Json;
using UnityEngine;

namespace BroMakerLib.Vanilla.Specials
{
    /// <summary>Shared base for grenade-throw specials. Extended by: RambroSpecial, BaBroracusSpecial, BrodellWalkerSpecial, BroffySpecial, BroHardSpecial, DemolitionBroSpecial, DirtyHarrySpecial, TankBroSpecial, TollBroadSpecial, TrentBroserSpecial.</summary>
    public class GrenadeThrowSpecial : SpecialAbility
    {
        /// <summary>Name of the grenade prefab to throw.</summary>
        public string grenadeName = "Grenade";
        /// <summary>Whether to fire the bro's special event on throw (used by some bros for stat tracking).</summary>
        public bool triggerSpecialEvent = true;

        [JsonIgnore]
        protected Grenade grenade;

        /// <summary>Horizontal throw speed for a standard throw.</summary>
        public float throwForceX = 200f;
        /// <summary>Vertical throw speed for a standard throw.</summary>
        public float throwForceY = 150f;

        /// <summary>Horizontal spawn offset when throwing from a ducked position.</summary>
        public float feetOffsetX = 6f;
        /// <summary>Vertical spawn offset when throwing from a ducked position.</summary>
        public float feetOffsetY = 3f;
        /// <summary>Horizontal throw speed when throwing from a ducked position.</summary>
        public float feetForceX = 30f;
        /// <summary>Vertical throw speed when throwing from a ducked position.</summary>
        public float feetForceY = 70f;

        /// <summary>Vanilla bro to source `throwSounds` from. Override in subclasses to match the bro's own throw grunts (e.g., TankBroSpecial overrides to TankBro).</summary>
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

        public override void Initialize(BroBase owner)
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
