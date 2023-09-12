using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BroMakerLib
{
    public static class ParachuteExtensions
    {
        public static void Set_tvd(this Parachute parachute, TestVanDammeAnim tvd)
        {
            parachute.tvd = tvd;
        }
    }
}
