using BroMakerLib.Attributes;
using BroMakerLib.Loggers;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace BroMakerLib
{
    public static class PresetManager
    {
        /// <summary>
        /// The assembly that contains presets.
        /// Add yours if your assembly contains presets.
        /// </summary>
        public static List<Assembly> assemblies = new List<Assembly>();
        public static Dictionary<string, MethodInfo> parameters = new Dictionary<string, MethodInfo>();

        public static Dictionary<string, Type> heroesPreset = new Dictionary<string, Type>();
        public static Dictionary<string, Type> customObjectsPreset = new Dictionary<string, Type>();
        public static Dictionary<string, Type> abilities = new Dictionary<string, Type>();

        public static bool disableWarnings = false;

        static PresetManager()
        { }

        public static void Initialize()
        {
            heroesPreset = new Dictionary<string, Type>();
            customObjectsPreset = new Dictionary<string, Type>();
            abilities = new Dictionary<string, Type>();

            assemblies = new List<Assembly>();
            assemblies.Add(Assembly.GetExecutingAssembly());
            assemblies.AddRange(DirectoriesManager.LoadAssembliesInStorage());

            CheckAssemblies(assemblies);
        }

        /// <summary>
        /// Return hero Preset
        /// </summary>
        /// <param name="presetName"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static Type GetHeroPreset(string presetName)
        {
            if (!heroesPreset.ContainsKey(presetName))
                return null;
            return heroesPreset[presetName];
        }

        public static Type GetAbilityPreset(string name)
        {
            if (!abilities.ContainsKey(name))
                return null;
            return abilities[name];
        }
        public static MethodInfo GetParameterMethod(string name)
        {
            if (!parameters.ContainsKey(name))
                return null;
            return parameters[name];
        }


        private static void CheckAssemblies(List<Assembly> assemblies)
        {
            foreach (Assembly assembly in assemblies)
            {
                try
                {
                    RetrieveParameters(assembly);
                    Type[] presets = FindPresets(assembly);
                    AddPresets(presets);
                }
                catch(Exception ex)
                {
                    BMLogger.ExceptionLog($"{assembly.FullName}", ex);
                }
            }
        }

        private static void RetrieveParameters(Assembly assembly)
        {
            var type = assembly.GetType("Parameters");
            if (type == null) return;

            var methods = type.GetMethods();
            foreach(var method in methods)
            {
                var attributes = method.GetCustomAttributes(typeof(ParameterAttribute), true);
                if (attributes.IsNotNullOrEmpty())
                {
                    if (parameters.ContainsKey(method.Name))
                        BMLogger.Warning($"Parameter of name {method.Name} already exist in assembly {type.Assembly.FullName}");
                    else
                        parameters.Add(method.Name, method);
                }
            }
        }

        private static Type[] FindPresets(Assembly assembly)
        {
            if (assembly == null)
                throw new NullReferenceException(nameof(assembly));

            var result = new List<Type>();
            var types = assembly.GetTypes();

            foreach (Type type in types)
            {
                var attributes = type.GetCustomAttributes(typeof(CustomObjectPresetAttribute), true);
                if (attributes.IsNotNullOrEmpty())
                {
                    result.Add(type);
                }
            }
            return result.ToArray();
        }

        private static void AddPresets(Type[] types)
        {
            foreach (Type type in types)
            {
                var attributes = type.GetCustomAttributes(typeof(CustomObjectPresetAttribute), true);
                if (attributes.IsNotNullOrEmpty())
                {
                    var first = attributes[0];
                    try
                    {
                        var presetCollection = GetPresetCollection(first);

                        if (presetCollection == null)
                            continue;

                        AddPresetToCollection(presetCollection, type, first.As<CustomObjectPresetAttribute>().name, GetCollectionName(first));
                    }
                    catch (Exception ex)
                    {
                        BMLogger.ExceptionLog($"{first.As<CustomObjectPresetAttribute>().name}", ex);
                    }
                }
            }
        }

        private static Dictionary<string, Type> GetPresetCollection(object attribute)
        {
            if (attribute == null || attribute as CustomObjectPresetAttribute == null)
                return null;

            if (attribute is HeroPresetAttribute)
                return heroesPreset;
            if (attribute is AbilityPresetAttribute)
                return abilities;

            return customObjectsPreset;
        }

        private static void AddPresetToCollection(Dictionary<string, Type> collection, Type preset, string name, string collectionName = "")
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));
            if (preset == null)
                throw new ArgumentNullException(nameof(preset));
            if (name.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(name), "is null or empty.");

            if (collection.ContainsKey(name))
            {
                if ( !disableWarnings )
                {
                    BMLogger.Warning($"{collectionName} Preset of name {name} already exist. Type: {preset} ; Assembly: {preset.Assembly.FullName}");
                }
            }
            else
                collection.Add(name, preset);
        }

        private static string GetCollectionName(object attribute)
        {
            if (attribute is HeroPresetAttribute)
                return "Hero";
            if (attribute is AbilityPresetAttribute)
                return "Ability";
            return string.Empty;
        }
    }
}
