using BroMakerLib.Abilities;
using UnityEngine;

namespace BroMakerLib.Vanilla.Specials
{
    /// <summary>Shared base for grenade-throw specials. Extended by: RambroSpecial, BaBroracusSpecial, BrodellWalkerSpecial, BroffySpecial, BroHardSpecial, DemolitionBroSpecial, DirtyHarrySpecial, TankBroSpecial, TollBroadSpecial, TrentBroserSpecial.</summary>
    public abstract class GrenadeThrowSpecial : SpecialAbility
    {
        /// <summary>Grenade thrown by the special.</summary>
        public GrenadeConfig grenade = new GrenadeConfig
        {
            name = "Grenade",
            offsetX = 8f,
            offsetY = 8f,
            throwForceX = 200f,
            throwForceY = 150f,
            duckOffsetX = 6f,
            duckOffsetY = 3f,
            duckThrowForceX = 30f,
            duckThrowForceY = 70f
        };

        /// <summary>Whether to fire the bro's special event on throw.</summary>
        public bool triggerSpecialEvent = true;

        /// <summary>Volume of the sound played when the grenade is thrown.</summary>
        public float throwSoundVolume = 0.4f;

        public GrenadeThrowSpecial()
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

        public override void UseSpecial()
        {
            if (owner.SpecialAmmo > 0 && grenade.basePrefab != null)
            {
                PlayThrowLightSound(throwSoundVolume);
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
            bool ducking = owner.down && owner.IsOnGround() && hero.Ducking;
            float offsetX = ducking ? grenade.duckOffsetX : grenade.offsetX;
            float offsetY = ducking ? grenade.duckOffsetY : grenade.offsetY;
            float forceX = ducking ? grenade.duckThrowForceX : grenade.throwForceX;
            float forceY = ducking ? grenade.duckThrowForceY : grenade.throwForceY;

            for (int i = 0; i < grenade.count; i++)
            {
                Vector2 force = grenade.GetThrowVector(Direction * forceX, forceY, i);
                ProjectileController.SpawnGrenadeOverNetwork(
                    grenade.basePrefab, owner,
                    X + (Direction * offsetX), Y + offsetY,
                    0.001f, 0.011f,
                    force.x, force.y,
                    PlayerNum, 1f);
            }
        }
    }
}
