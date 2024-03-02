using BroMakerLib.CustomObjects.Components;
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

        List<Material> specialMaterials { get; set; }
        Vector2 specialMaterialOffset { get; set; }
        float specialMaterialSpacing { get; set; }
        Material firstAvatar { get; set; }
        Vector2 gunSpriteOffset { get; set; }
        MuscleTempleFlexEffect flexEffect { get; set; }
    }
}
