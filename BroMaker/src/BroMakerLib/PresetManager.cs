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
        public static Dictionary<string, Type> heroesPreset = new Dictionary<string, Type>();
        public static Dictionary<string, Type> customObjectsPreset = new Dictionary<string, Type>();

        static PresetManager()
        {
        }

        public static void Initialize()
        {
            assemblies = new List<Assembly>();
            assemblies.Add(Assembly.GetExecutingAssembly());
            GetPresetsInAssemblies();
        }

        public static Type GetPreset(string presetName)
        {
            if (!heroesPreset.ContainsKey(presetName))
                throw new Exception($"No preset named: {presetName} founded");
            return heroesPreset[presetName];
        }

        private static void GetPresetsInAssemblies()
        {
            assemblies.AddRange(DirectoriesManager.LoadAssembliesInStorage());
            foreach (Assembly assembly in assemblies)
            {
                try
                {
                    Type[] types = assembly.GetTypes();
                    CheckClassesAttributes(types);
                }
                catch(Exception ex)
                {
                    BMLogger.ExceptionLog($"{assembly.FullName}", ex);
                }
            }
        }

        private static void CheckClassesAttributes(Type[] types)
        {
            foreach (Type type in types)
            {
                var attributes = type.GetCustomAttributes(typeof(CustomObjectPresetAttribute), true);
                if (attributes.IsNotNullOrEmpty())
                {
                    var first = attributes[0];
                    try
                    {
                        if (first.GetType() == typeof(HeroPresetAttribute))
                        {
                            string name = first.As<HeroPresetAttribute>().name;
                            if (heroesPreset.ContainsKey(name))
                                BMLogger.Log($"Preset of name {first.As<HeroPresetAttribute>().name} already exist.\nPreset comes from {type} in {type.Assembly.FullName}");
                            else
                                heroesPreset.Add(name, type);
                        }
                        else if (first.GetType() == typeof(AbilityPresetAttribute))
                        {
                            string name = first.As<AbilityPresetAttribute>().name;
                            if (AbilitiesManager.abilities.ContainsKey(name))
                                BMLogger.Log($"Abilities of name {first.As<AbilityPresetAttribute>().name} already exist.\nAbilities comes from {type} in {type.Assembly.FullName}");
                            else
                                AbilitiesManager.abilities.Add(name, type);
                        }
                        else
                        {
                            string name = first.As<CustomObjectPresetAttribute>().name;
                            if (customObjectsPreset.ContainsKey(name))
                                BMLogger.Log($"Preset of name {first.As<CustomObjectPresetAttribute>().name} already exist.\nPreset comes from {type} in {type.Assembly.FullName}");
                            else
                                customObjectsPreset.Add(name, type);
                        }
                    }
                    catch (Exception ex)
                    {
                        BMLogger.ExceptionLog($"{first.As<CustomObjectPresetAttribute>().name}", ex);
                    }
                }
            }
        }
    }
}
