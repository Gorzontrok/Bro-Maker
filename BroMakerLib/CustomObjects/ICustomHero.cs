using BroMakerLib.Infos;
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
        Vector2 CurrentGunSpriteOffset { get; set; }
        List<Material> CurrentSpecialMaterials { get; set; }
        Vector2 CurrentSpecialMaterialOffset { get; set; }
        float CurrentSpecialMaterialSpacing { get; set; }
        Material CurrentFirstAvatar { get; set; }
    }
}
