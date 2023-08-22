using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RocketLib;

namespace BroMakerLib
{
    public static class AbilitiesManager
    {
        public static Dictionary<string, Type> abilities = new Dictionary<string, Type>();

        public static void Initialize()
        {

        }

        public static Type GetAbilityType(string name)
        {
            if (!abilities.ContainsKey(name))
                return null;
            return abilities[name];
        }

        public static void InvokeStaticMethod(string abilityName, string methodName, bool throwMissingMethodException = false, params object[] parameters)
        {
            InvokeStaticMethod(GetAbilityType(abilityName), methodName, throwMissingMethodException, parameters);
        }
        public static void InvokeStaticMethod(Type abilityType, string methodName, bool throwMissingMethodException = false, params object[] parameters)
        {
            if (abilityType == null)
                return;
            if(parameters == null)
                parameters = new object[0];

            var method = abilityType.GetMethod(methodName);
            if (method == null)
            {
                if (throwMissingMethodException)
                    throw new MissingMethodException(abilityType.Name, methodName);
                return;
            }

            if (!method.IsStatic)
                throw new Exception($"Method {methodName} is not static.");

            var methodParameters = method.GetParameters();
            if (methodParameters.Length != parameters.Length)
                throw new Exception($"The method {methodName} contains {methodParameters.Length} parameters. Only {parameters.Length} parameters were given.");

            for(int i = 0; i < methodParameters.Length; i++)
            {
                if (parameters[i] == null && !methodParameters[i].ParameterType.CanBeNull())
                    throw new NullReferenceException($"Parameter number {i} can't be null.");
                if (parameters[i].GetType() != methodParameters[i].GetType() && parameters.GetType().IsSubclassOf(methodParameters.GetType()))
                    throw new Exception($"Type mismatch: {parameters.GetType()} is not a subclass or the same type as {methodParameters.GetType()}");
            }
            method.Invoke(null, parameters);
        }
    }
}
