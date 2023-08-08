using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BroMakerLib.Storages
{
    public interface IStoredObject
    {
        string path { get; set; }
        string name { get; set; }
    }
}
