using System.Collections.Generic;
using BroMakerLib.Infos;
using UnityEngine;

namespace BroMakerLib.CustomObjects
{
    public interface ICustomHero
    {
        [Syncronize]
        CustomBroInfo Info { get; set; }
        [Syncronize]
        BroBase Character { get; set; }
        MuscleTempleFlexEffect FlexEffect { get; set; }
        int CurrentVariant { get; set; }
        Vector2 CurrentGunSpriteOffset { get; set; }
        List<Material> CurrentSpecialMaterials { get; set; }
        Vector2 CurrentSpecialMaterialOffset { get; set; }
        float CurrentSpecialMaterialSpacing { get; set; }
        Material CurrentFirstAvatar { get; set; }

        /// <summary>
        /// Called once at prefab creation time to set up fields that would otherwise be lost
        /// during the component swap. Runs before any Awake calls.
        /// </summary>
        void PrefabSetup();
    }
}
