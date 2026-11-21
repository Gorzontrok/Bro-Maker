namespace BroMakerLib.Attributes
{
    /// <summary>Registers this class as a passive ability under the given name, which an entry in a bro's JSON `passives` list selects with `"preset"`.</summary>
    public class PassivePresetAttribute : CustomObjectPresetAttribute
    {
        public PassivePresetAttribute(string name) : base(name)
        {
        }
    }
}
