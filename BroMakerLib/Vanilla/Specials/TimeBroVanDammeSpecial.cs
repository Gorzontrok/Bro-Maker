using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using RocketLib.Extensions;
using UnityEngine;

namespace BroMakerLib.Vanilla.Specials
{
    [SpecialPreset("TimeBroVanDamme")]
    public class TimeBroVanDammeSpecial : SpecialAbility
    {
        public float timeBoostDuration = 2f;
        public float heroBoostDuration = 2.3f;
        public float colorShiftDuration = 2.2f;
        public float soundVolume = 0.7f;

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
                Sound.GetInstance().PlaySoundEffect(soundHolder.special3Sounds, soundVolume, 1f);
                owner.SpecialAmmo--;
                if (owner.IsMine)
                {
                    HeroController.TimeBroBoost(timeBoostDuration);
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

        public override bool HandleDeath()
        {
            HeroController.CancelTimeBroBoost();
            return true;
        }
    }
}
