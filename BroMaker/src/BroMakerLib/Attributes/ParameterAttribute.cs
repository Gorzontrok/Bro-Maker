using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BroMakerLib.Attributes
{

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class ParameterAttribute : Attribute
    {
        public Type valueType;
        public ParameterAttribute(Type type)
        {
            valueType = type;
        }
    }
}
