using BroMakerLib.Attributes;
using BroMakerLib.Extensions;
using UnityEngine;

namespace BroMakerLib.Vanilla.Melees
{
    /// <summary>Broniversal Soldier's Van Damme split-kick melee.</summary>
    [MeleePreset("BroniversalSoldier")]
    public class BroniversalSoldierMelee : SplitKickMelee
    {
        protected override HeroType SourceBroType => HeroType.BroniversalSoldier;

        protected override float CalculateKickForce()
        {
            float baseForce = 350f + Mathf.Abs(owner.xI);
            bool serumFrenzy = owner.GetFieldValue<bool>("serumFrenzy");
            return baseForce * (serumFrenzy ? 1.8f : 1f);
        }
    }
}
