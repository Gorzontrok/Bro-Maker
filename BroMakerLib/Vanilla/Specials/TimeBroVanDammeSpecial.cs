using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using BroMakerLib.Extensions;
using HarmonyLib;
using Newtonsoft.Json;
using RocketLib.Extensions;
using UnityEngine;

namespace BroMakerLib.Vanilla.Specials
{
    /// <summary>Time Bro Van Damme's bullet-time slow-motion special.</summary>
    [SpecialPreset("TimeBro")]
    public class TimeBroVanDammeSpecial : SpecialAbility
    {
        protected override HeroType SourceBroType => HeroType.TimeBroVanDamme;

        protected override void CacheSoundsFromPrefab()
        {
            base.CacheSoundsFromPrefab();
            var sourceBro = HeroController.GetHeroPrefab(SourceBroType);
            if (sourceBro == null) return;
            if (special3Sounds == null) special3Sounds = sourceBro.soundHolder.special3Sounds.CloneArray();
        }
        /// <summary>Seconds the global time boost lasts.</summary>
        public float timeBoostDuration = 2f;
        /// <summary>Seconds the hero speed boost lasts.</summary>
        public float heroBoostDuration = 2.3f;
        /// <summary>Seconds the slow-motion color shift effect lasts.</summary>
        public float colorShiftDuration = 2.2f;
        /// <summary>Time scale applied to the game during bullet time.</summary>
        public float timeScale = 0.35f;
        /// <summary>Audio pitch applied during bullet time.</summary>
        public float soundPitch = 0.5f;
        /// <summary>Playback volume for the special activation sound.</summary>
        public float soundVolume = 0.7f;

        /// <summary>Sound played on special activation.</summary>
        public AudioClip[] special3Sounds;

        public TimeBroVanDammeSpecial()
        {
            animationColumn = 17;
            animationRow = 5;
            animationFrameCount = 8;
            triggerFrame = 4;
        }

        public override void UseSpecial()
        {
            if (owner.SpecialAmmo > 0)
            {
                Sound.GetInstance().PlaySoundEffect(special3Sounds, soundVolume, 1f);
                owner.SpecialAmmo--;
                if (owner.IsMine)
                {
                    Traverse.Create(typeof(HeroController)).Field("timeBoostTime").SetValue(timeBoostDuration);
                    Time.timeScale = timeScale;
                    Sound.SetPitch(soundPitch);
                    if (!GameModeController.IsDeathMatchMode && GameModeController.GameMode != GameMode.BroDown)
                    {
                        HeroController.TimeBroBoostHeroes(heroBoostDuration);
                    }
                    else
                    {
                        owner.TimeBroBoost(heroBoostDuration);
                    }
                    ColorShiftController.SlowTimeEffect(colorShiftDuration);
                }
            }
            else
            {
                HeroController.FlashSpecialAmmo(PlayerNum);
                hero.ActivateGun();
            }
            hero.PressSpecialFacingDirection = 0;
        }

        public override bool HandleSetDeltaTime()
        {
            owner.SetFieldValue("lastT", hero.DeltaTime);
            if (Time.timeScale > 0f)
            {
                owner.SetFieldValue("t", Mathf.Clamp(Time.deltaTime / Time.timeScale, 0f, 0.04f));
            }
            else
            {
                owner.SetFieldValue("t", 0f);
            }
            return false;
        }

        public override void Update()
        {
            // Compensates scaled Time.time so jump cooldowns elapse at real-time rate. Not in HandleSetDeltaTime: base routes to SetHighFiveBoostDeltaTime during timeBroBoost, skipping the hook.
            if (Time.timeScale > 0f && Time.timeScale < 1f)
            {
                float compensation = Time.deltaTime * (1f - Time.timeScale) / Time.timeScale;
                owner.SetFieldValue("lastJumpTime",
                    owner.GetFieldValue<float>("lastJumpTime") - compensation);
                owner.SetFieldValue("lastButtonJumpTime",
                    owner.GetFieldValue<float>("lastButtonJumpTime") - compensation);
            }
        }

        public override bool HandleDeath()
        {
            HeroController.CancelTimeBroBoost();
            return true;
        }
    }
}
