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
        public static Dictionary<string, Type> heroPreset = new Dictionary<string, Type>();

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
            if (!heroPreset.ContainsKey(presetName))
                throw new Exception($"No preset named: {presetName} founded");
            return heroPreset[presetName];
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
                            if (heroPreset.ContainsKey(name))
                                BMLogger.Log($"Preset of name {first.As<HeroPresetAttribute>().name} already exist.\nPreset comes from {type} in {type.Assembly.FullName}");
                            else
                                heroPreset.Add(name, type);
                        }
                    }
                    catch (Exception ex)
                    {
                        BMLogger.ExceptionLog($"{first.As<HeroPresetAttribute>().name}", ex);
                    }
                }
            }
        }
    }
}
