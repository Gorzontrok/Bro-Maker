using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BroMakerLib.CustomObjects
{
    [CustomObjectPreset("Halo_AlwaysShown")]
    public class HaloAlwaysShown : BroHaloM
    {
        public override void Hide()
        { }
    }
}
