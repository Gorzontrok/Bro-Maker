using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using Networking;
using Newtonsoft.Json;
using UnityEngine;

namespace BroMakerLib.Vanilla.Specials
{
    /// <summary>Ellen Ripbro's flame-wave special.</summary>
    [SpecialPreset("EllenRipbro")]
    public class EllenRipbroSpecial : SpecialAbility
    {
        protected override HeroType SourceBroType => HeroType.EllenRipbro;
        /// <summary>Delay in seconds after the flame wave is spawned before ammo is decremented.</summary>
        public float ammoDecrementDelay = 0.2f;

        [JsonIgnore]
        protected FlameWallExplosion flameWavePrefab;
        [JsonIgnore]
        private FlameWallExplosion currentFlameWave;
        [JsonIgnore]
        private bool firedSpecial;
        [JsonIgnore]
        private float flameWaveSpecialUseDelay;

        public EllenRipbroSpecial()
        {
            spawnOffsetX = 5f;
            spawnOffsetY = 9f;
            animationRow = 7;
        }

        public override void Initialize(BroBase owner)
        {
            base.Initialize(owner);
            var prefab = HeroController.GetHeroPrefab(HeroType.EllenRipbro);
            var ellen = prefab.GetComponent<EllenRipbro>();
            if (ellen != null)
            {
                flameWavePrefab = ellen.flameWavePrefab;
            }
        }

        public override void PressSpecial()
        {
            if (!owner.hasBeenCoverInAcid && !hero.UsingSpecial)
            {
                if (owner.SpecialAmmo > 0)
                {
                    hero.UsingSpecial = true;
                    firedSpecial = false;
                    owner.frame = 0;
                    hero.ChangeFrame();
                    owner.counter = 0f;
                    hero.PressSpecialFacingDirection = (int)owner.transform.localScale.x;
                }
                else
                {
                    HeroController.FlashSpecialAmmo(PlayerNum);
                }
            }
        }

        public override void AnimateSpecial()
        {
            hero.SetSpriteOffset(0f, 0f);
            hero.DeactivateGun();
            hero.FrameRate = frameRate;
            int column = Mathf.Clamp(owner.frame, 0, 6);
            hero.Sprite.SetLowerLeftPixel(column * hero.SpritePixelWidth, hero.SpritePixelHeight * animationRow);
            if (owner.frame == 3 && !firedSpecial)
            {
                UseSpecial();
            }
            if (owner.frame >= 6)
            {
                owner.frame = 0;
                hero.UsingSpecial = false;
                hero.ActivateGun();
                hero.ChangeFrame();
            }
        }

        public override void UseSpecial()
        {
            if (!owner.IsMine)
            {
                return;
            }
            if (owner.SpecialAmmo > 0)
            {
                firedSpecial = true;
                flameWaveSpecialUseDelay = ammoDecrementDelay;
                currentFlameWave = global::Networking.Networking.Instantiate<FlameWallExplosion>(flameWavePrefab, new Vector3(X + Direction * spawnOffsetX, Y + spawnOffsetY, 0f), Quaternion.identity, null, false);
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
                currentFlameWave.Setup(PlayerNum, owner, dir);
            }
            else
            {
                HeroController.FlashSpecialAmmo(PlayerNum);
                hero.ActivateGun();
            }
            hero.PressSpecialFacingDirection = 0;
        }

        public override void Update()
        {
            if (flameWaveSpecialUseDelay > 0f)
            {
                flameWaveSpecialUseDelay -= hero.DeltaTime;
                if (flameWaveSpecialUseDelay <= 0f && currentFlameWave != null)
                {
                    owner.SpecialAmmo--;
                }
            }
        }
    }
}
