namespace BroMakerLib.Attributes
{
    /// <summary>Registers this class as a special ability under the given name, which a bro's JSON `special` config selects with `"preset"`.</summary>
    public class SpecialPresetAttribute : CustomObjectPresetAttribute
    {
        public SpecialPresetAttribute(string name) : base(name)
        {
        }
    }
}
