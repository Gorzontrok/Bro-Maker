using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BroMakerLib.Attributes
{
    public class WeaponPresetAttribute : CustomObjectPresetAttribute
    {
        public WeaponPresetAttribute(string name) : base(name)
        { }
    }
}
