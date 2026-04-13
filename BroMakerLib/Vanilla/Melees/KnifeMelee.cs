using BroMakerLib.Abilities;
using BroMakerLib.Attributes;

namespace BroMakerLib.Vanilla.Melees
{
    /// <summary>
    /// Standard knife melee shared by 17 bros. Uses all base class defaults —
    /// no overrides needed. Knife animation (row 1/col 25 standing, row 6/col 17 jumping),
    /// standard hit detection, and Rambro sounds are all base class defaults.
    /// </summary>
    [MeleePreset("KnifeMelee")]
    public class KnifeMelee : MeleeAbility
    {
    }
}
