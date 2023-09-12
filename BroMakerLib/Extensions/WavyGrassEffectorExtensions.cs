using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BroMakerLib
{
    public static class WavyGrassEffectorExtensions
    {
        public static void SetUnit(this WavyGrassEffector wavyGrassEffector, Unit unit)
        {
            Traverse.Create(wavyGrassEffector).Field("unit").SetValue(unit);
        }
    }
}
