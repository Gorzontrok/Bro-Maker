using BroMakerLib.Attributes;
using UnityEngine;

namespace BroMakerLib.Vanilla.Specials
{
    /// <summary>Tank Bro's tank-summon throw special. The whistle played on the throw takes the frame's one effect sound, so `throwSounds` and `throwSoundVolume` have no effect.</summary>
    [SpecialPreset("TankBro")]
    public class TankBroSpecial : GrenadeThrowSpecial
    {
        public AudioClip whistle;

        protected override HeroType SourceBroType => HeroType.TankBro;

        public TankBroSpecial()
        {
            grenade.name = "SummonTank";
        }

        protected override void CacheSoundsFromPrefab()
        {
            base.CacheSoundsFromPrefab();
            TankBro sourceBro = HeroController.GetHeroPrefab(SourceBroType) as TankBro;
            if (sourceBro == null)
            {
                return;
            }
            if (whistle == null)
            {
                whistle = sourceBro.whistle;
            }
        }

        public override void UseSpecial()
        {
            if (owner.SpecialAmmo > 0 && whistle != null)
            {
                Sound.GetInstance().PlaySoundEffect(whistle, 1f, 1.5f * Random.Range(0.9f, 1.1f));
            }
            base.UseSpecial();
        }
    }
}
