using BroMakerLib.Infos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BroMakerLib.CustomObjects
{
    public interface ICustomObjectFromFile<T, TInfo> where T : BroforceObject where TInfo : CustomBroforceObjectInfo
    {
        TInfo info { get; set; }
        T obj { get; set; }

        void SetupObject();

        void LoadStats();
    }
}
