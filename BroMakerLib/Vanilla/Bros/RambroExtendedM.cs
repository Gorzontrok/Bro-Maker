using BroMakerLib.CustomObjects;
using BroMakerLib.CustomObjects.Bros;
using BroMakerLib.Infos;
using BroMakerLib.Loaders;
using BroMakerLib.Loggers;
using System;
using World.Generation.MapGenV4;

namespace BroMakerLib.Vanilla.Bros
{
    [HeroPreset("RambroExtended")]
    public class RambroExtendedM : CustomHeroExtended
    {
        protected override void FireWeapon(float x, float y, float xSpeed, float ySpeed)
        {
            base.FireWeapon(x, y, xSpeed, ySpeed);
            if (this.attachedToZipline != null)
            {
                this.SetGunSprite(3, 0);
            }
        }
    }
}
