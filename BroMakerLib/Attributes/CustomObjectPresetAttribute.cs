using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BroMakerLib
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class CustomObjectPresetAttribute : Attribute
    {
        public string name;
        public CustomObjectPresetAttribute(string name)
        {
            this.name = name;
        }
    }
}
