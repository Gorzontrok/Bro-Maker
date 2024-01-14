using System;

namespace BroMakerLib.Attributes
{

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class ParameterAttribute : Attribute
    {
        public Type valueType;
        public ParameterAttribute()
        { }
        [Obsolete("Useless")]
        public ParameterAttribute(Type type)
        {
            valueType = type;
        }
    }
}
