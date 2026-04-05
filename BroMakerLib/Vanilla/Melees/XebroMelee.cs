using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using Newtonsoft.Json;
using RocketLib.Extensions;
using UnityEngine;

namespace BroMakerLib.Vanilla.Melees
{
    [MeleePreset("Xebro")]
    public class XebroMelee : MeleeAbility
    {
        public float flipJumpYI = 100f;
        public float flipGravityM = 1f;
        public float warCryVolume = 0.15f;
        public float wallInFrontAttackDistance = 19f;

        // Used only when owner is not Xebro — when owner is Xebro we read/write through the owner's fields directly
        [JsonIgnore] private bool isXebroFlipping;
        [JsonIgnore] private bool _hasFlipped;
        [JsonIgnore] private float _lastXebroFlipAttackPressTime;
        [JsonIgnore] private float normalSpeed;

        [JsonIgnore] private bool ownerIsXebro;
        [JsonIgnore] private AudioSource warCryAudio;
        [JsonIgnore] private bool ownedWarCryAudio;

        private bool IsXebroFlipping
        {
            get => ownerIsXebro ? owner.GetFieldValue<bool>("isXebroFlipping") : isXebroFlipping;
            set
            {
                isXebroFlipping = value;
                if (ownerIsXebro)
                    owner.SetFieldValue("isXebroFlipping", value);
            }
        }

        private bool HasFlipped
        {
            get => ownerIsXebro ? owner.GetFieldValue<bool>("_hasFlipped") : _hasFlipped;
            set
            {
                _hasFlipped = value;
                if (ownerIsXebro)
                    owner.SetFieldValue("_hasFlipped", value);
            }
        }

        private float LastXebroFlipAttackPressTime
        {
            get => ownerIsXebro ? owner.GetFieldValue<float>("_lastXebroFlipAttackPressTime") : _lastXebroFlipAttackPressTime;
            set
            {
                _lastXebroFlipAttackPressTime = value;
                if (ownerIsXebro)
                    owner.SetFieldValue("_lastXebroFlipAttackPressTime", value);
            }
        }

        public override void Initialize(TestVanDammeAnim owner)
        {
            base.Initialize(owner);

            normalSpeed = owner.speed;

            ownerIsXebro = owner is Xebro;

            var xebro = owner as Xebro;
            if (xebro == null)
            {
                var prefab = HeroController.GetHeroPrefab(HeroType.Xebro);
                xebro = prefab as Xebro;
            }

            if (ownerIsXebro)
            {
                warCryAudio = owner.GetFieldValue<AudioSource>("warCryAudio");
                ownedWarCryAudio = false;
            }
            else
            {
                warCryAudio = owner.gameObject.AddComponent<AudioSource>();
                if (xebro != null)
                {
                    warCryAudio.clip = xebro.soundHolder.attack3Sounds[0];
                }
                warCryAudio.loop = true;
                warCryAudio.dopplerLevel = 0.06f;
                warCryAudio.rolloffMode = AudioRolloffMode.Linear;
                float num = 300f;
                warCryAudio.maxDistance = num;
                warCryAudio.minDistance = num;
                warCryAudio.volume = 0f;
                warCryAudio.Stop();
                ownedWarCryAudio = true;
            }
        }

        public override void Cleanup()
        {
            if (ownedWarCryAudio && warCryAudio != null)
            {
                warCryAudio.Stop();
                Object.Destroy(warCryAudio);
                warCryAudio = null;
            }
        }

        public override void Update()
        {
            if (IsXebroFlipping || (hero.DoingMelee && ((BroBase)owner).meleeType == BroBase.MeleeType.Custom))
            {
                if (!warCryAudio.isPlaying)
                {
                    warCryAudio.time = 0f;
                    warCryAudio.volume = 0f;
                    warCryAudio.Play();
                }
                float num = 1f + 0.05f * (1f - warCryAudio.volume / warCryVolume);
                warCryAudio.volume = warCryVolume;
                warCryAudio.pitch = num;
            }
            else if (warCryAudio.isPlaying)
            {
                float num2 = 1f - 0.2f * (1f - warCryAudio.volume / warCryVolume);
                warCryAudio.volume = Mathf.MoveTowards(warCryAudio.volume, 0f, hero.DeltaTime * warCryVolume * 8f);
                warCryAudio.pitch = num2;
                if (warCryAudio.volume <= 0f)
                {
                    warCryAudio.Stop();
                }
            }
        }

        public override void StartMelee()
        {
            if (owner.IsOnGround() && ((!owner.left && !owner.right) || owner.CallMethod<bool>("IsGroundBelowAtXOffset", (float)owner.Direction * (owner.GetFieldValue<float>("feetWidth") + 0.2f), true)))
            {
                IsXebroFlipping = false;
                owner.speed = normalSpeed;
            }
            else
            {
                if (!HasFlipped && owner.yI < flipJumpYI)
                {
                    owner.yI = flipJumpYI;
                }
                HasFlipped = true;
                IsXebroFlipping = true;
                owner.speed = normalSpeed * 1.33f;
            }

            hero.StartMeleeCommon();

            if (IsXebroFlipping)
            {
                owner.frame = 2;
            }
            else
            {
                owner.frame = 3;
            }
            owner.counter = 0f;
            AnimateMelee();
            hero.InvulnerableTime = 0.5f;
        }

        public override void AnimateMelee()
        {
            if (owner.frame < 8)
            {
                hero.FrameRate = 0.022f;
            }
            else
            {
                hero.FrameRate = 0.033f;
            }
            if ((owner.frame == 9 || owner.frame == 10) && owner.IsOnGround())
            {
                owner.speed = normalSpeed * 1.33f;
                owner.xI = owner.speed * owner.transform.localScale.x;
                owner.yI = flipJumpYI * 1.2f;
                IsXebroFlipping = true;
                HasFlipped = true;
                Map.ForgetPlayer(owner.playerNum, owner.X, owner.Y, 32f, true, false);
                Map.KnockMooks(owner, DamageType.Fall, 9f, 16f, owner.X + (float)(owner.Direction * 4), owner.Y, owner.xI * 0.55f, 50f + Random.value * 170f, true, true, true);
            }
            if (IsXebroFlipping && (owner.frame > 21 || owner.yI < -80f) && Time.time - LastXebroFlipAttackPressTime < 0.2f && (owner.left || owner.right) && owner.IsNearGround(34f) && owner.yI < 0f)
            {
                Map.ForgetPlayer(owner.playerNum, owner.X, owner.Y, 48f, 24f, true, false);
                if (!Map.KnockMooks(owner, DamageType.Fall, 13f, 18f, owner.X + (float)(owner.Direction * 7), owner.Y, owner.xI * 0.55f, 120f + Random.value * 160f, true, true, true))
                {
                    if (owner.yI > -80f)
                    {
                        owner.frame = 2;
                    }
                    else
                    {
                        owner.frame = 3;
                    }
                    IsXebroFlipping = false;
                }
                else
                {
                    owner.speed = normalSpeed * 1.33f;
                    IsXebroFlipping = true;
                    owner.xI = owner.speed * owner.transform.localScale.x;
                    owner.yI = flipJumpYI * 1.2f;
                    owner.frame -= 12;
                }
            }
            if ((!IsXebroFlipping && owner.frame > 15) || (IsXebroFlipping && owner.frame > 21))
            {
                owner.frame = 0;
                hero.CancelMelee();
            }
            else if (!IsXebroFlipping)
            {
                hero.Sprite.SetLowerLeftPixel((float)(owner.frame * hero.SpritePixelWidth), (float)(11 * hero.SpritePixelHeight));
                if (owner.frame > 13)
                {
                    owner.frame = 0;
                    hero.CancelMelee();
                }
            }
            else
            {
                hero.DeactivateGun();
                hero.FrameRate = 0.025f;
                if (owner.frame < 0)
                {
                    owner.frame = 0;
                }
                hero.Sprite.SetLowerLeftPixel((float)((19 + owner.frame % 12) * hero.SpritePixelWidth), (float)(hero.SpritePixelHeight * 11));
            }
        }

        public override void RunMeleeMovement()
        {
            if (!owner.IsOnGround())
            {
                if (!ownerIsXebro)
                {
                    // Replicate Xebro.ApplyFallingGravity for non-Xebro owners
                    bool isInQuicksand = owner.GetFieldValue<bool>("isInQuicksand");
                    bool isParachuteActive = owner.IsParachuteActive;
                    if (!isInQuicksand && !isParachuteActive && hero.DoingMelee && IsXebroFlipping)
                    {
                        owner.yI -= 1100f * hero.DeltaTime * flipGravityM;
                    }
                    else
                    {
                        owner.CallMethod("ApplyFallingGravity");
                    }
                }
                else
                {
                    owner.CallMethod("ApplyFallingGravity");
                }
                if (owner.yI < owner.maxFallSpeed)
                {
                    owner.yI = owner.maxFallSpeed;
                }
                owner.xI = Mathf.Lerp(owner.xI, owner.speed * 1.26f * owner.transform.localScale.x, Time.deltaTime * 15f);
                owner.speed = normalSpeed * 1.25f;
            }
            else
            {
                owner.speed = normalSpeed;
                if (owner.frame <= 2)
                {
                    owner.xI = 0f;
                    owner.yI = 0f;
                }
            }
        }

        public override void CancelMelee()
        {
            if (owner.heldMook != null)
            {
                owner.CallMethod("ReleaseHeldObject", false);
            }
            IsXebroFlipping = false;
            owner.speed = normalSpeed;
        }

        public override void HandleAfterLand()
        {
            if (!ownerIsXebro)
            {
                IsXebroFlipping = false;
                HasFlipped = false;
                owner.speed = normalSpeed;
            }
        }

        public override void HandleAfterDeath()
        {
            if (warCryAudio != null && warCryAudio.isPlaying)
            {
                warCryAudio.Stop();
            }
        }

        public override bool HandleAlertNearbyMooks()
        {
            if (hero.DoingMelee && IsXebroFlipping)
            {
                return false;
            }
            return true;
        }

        public override bool HandleWallDrag(bool value)
        {
            if (value)
            {
                HasFlipped = false;
                if (hero.DoingMelee)
                {
                    hero.CancelMelee();
                }
            }
            return true;
        }

        public override void HandleAfterHitCeiling()
        {
            LastXebroFlipAttackPressTime = -1f;
        }

        public bool IsFlipping()
        {
            return IsXebroFlipping;
        }
    }
}
