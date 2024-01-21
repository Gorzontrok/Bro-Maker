using System;

namespace BroMakerLib.Attributes
{
    internal class GrenadePresetAttribute : CustomObjectPresetAttribute
    {
        public string basedOn = "Default";
        public GrenadePresetAttribute(string name, string baseOn = "Default") : base(name)
        {
            this.basedOn = baseOn;
        }
    }
}
