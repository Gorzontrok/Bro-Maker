using BroMakerLib.Infos;
using BroMakerLib.Stats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BroMakerLib.CustomObjects.Components
{
    public class BroExtended : CharacterExtended
    {
        //public CustomBroInfo info { get; set; }

        public float holyWaterRevivePerformanceEnhancedTime = 0.02f;
        public float flexInvulnerability = 0.25f;

        public override void LoadStats(CharacterStats stats)
        {
            base.LoadStats(stats);
            if(stats is BroStats)
            {
                BroStats bstats = stats as BroStats;
                holyWaterRevivePerformanceEnhancedTime = bstats.holyWaterRevivePerformanceEnhancedTime;
                flexInvulnerability = bstats.flexPower.flexInvulnerability;
            }
        }
    }
}
