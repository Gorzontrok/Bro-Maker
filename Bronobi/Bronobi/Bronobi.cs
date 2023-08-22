using System;
using UnityEngine;
using BroMakerLib.CustomObjects.Bros;
using BroMakerLib;
using BroMakerLib.Loggers;

namespace BronobiMod
{
    [HeroPreset("Bronobi", HeroType.Blade)]
    public class Bronobi : SwordHero
    {
        protected BronobiForceWave forceWave;

        protected override void SetGunPosition(float xOffset, float yOffset)
        {
            this.gunSprite.transform.localPosition = new Vector3(xOffset + 4f, yOffset, -1f);
        }

        protected override void SetupThrownMookVelocity(out float XI, out float YI)
        {
            base.SetupThrownMookVelocity(out XI, out YI);
            XI *= 1.2f;
            YI *= 1.2f;
        }

        protected override void UseSpecial()
        {
            if (SpecialAmmo > 0)
            {
                /* DirectionEnum direction;
                 if (this.right)
                 {
                     direction = DirectionEnum.Right;
                 }
                 else if (this.left)
                 {
                     direction = DirectionEnum.Left;
                 }
                 else if (base.transform.localScale.x > 0f)
                 {
                     direction = DirectionEnum.Right;
                 }
                 else
                 {
                     direction = DirectionEnum.Left;
                 }*/
                try
                {

                }
                catch (Exception ex)
                {
                    BMLogger.Log($"[{typeof(Bronobi)}] {ex}");
                }
            }
            else
            {
                this.player.StopAvatarSpecialFrame();
                HeroController.FlashSpecialAmmo(base.playerNum);
            }
        }
    }
}
