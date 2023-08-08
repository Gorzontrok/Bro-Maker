using BroMakerLib.Stats;
using BroMakerLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BroMakerLib.Infos
{
    public class CustomSpecialInfo : CustomBroforceObjectInfo
    {
        public override string SerializeJSON()
        {
            return SerializeJSON(DirectoriesManager.AbilitiesDirectory);
        }
    }
}
