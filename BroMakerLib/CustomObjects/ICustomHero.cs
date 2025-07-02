using BroMakerLib.Infos;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

namespace BroMakerLib.CustomObjects
{
    public interface ICustomHero
    {
        [Syncronize]
        CustomBroInfo info { get; set; }
        [Syncronize]
        BroBase character { get; set; }
        MuscleTempleFlexEffect flexEffect { get; set; }
        int CurrentVariant { get; set; }
    }
}
