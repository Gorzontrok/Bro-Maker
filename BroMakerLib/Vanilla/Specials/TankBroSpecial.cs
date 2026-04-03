using BroMakerLib.Attributes;
using Newtonsoft.Json;
using UnityEngine;

namespace BroMakerLib.Vanilla.Specials
{
    [SpecialPreset("TankBro")]
    public class TankBroSpecial : GrenadeThrowSpecial
    {
        [JsonIgnore]
        protected AudioClip whistle;

        public TankBroSpecial()
        {
            grenadeName = "SummonTank";
        }

        public override void Initialize(TestVanDammeAnim owner)
        {
            base.Initialize(owner);
            if (whistle == null)
            {
                var prefab = HeroController.GetHeroPrefab(HeroType.TankBro);
                var tankBro = prefab.GetComponent<TankBro>();
                if (tankBro != null)
                {
                    whistle = tankBro.whistle;
                }
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
