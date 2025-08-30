using BroMakerLib.CustomObjects.Bros;

namespace BroMakerLib.Vanilla.Bros
{
    [HeroPreset("Rambro")]
    public class RambroM : CustomHero
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
