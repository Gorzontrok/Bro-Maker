namespace BroMakerLib.Attributes
{
    /// <summary>Registers this class as a melee ability under the given name, which a bro's JSON `melee` config selects with `"preset"`.</summary>
    public class MeleePresetAttribute : CustomObjectPresetAttribute
    {
        public MeleePresetAttribute(string name) : base(name)
        {
        }
    }
}
