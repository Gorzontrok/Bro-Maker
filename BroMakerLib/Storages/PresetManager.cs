using BroMakerLib.Attributes;
using BroMakerLib.Loggers;
using BroMakerLib.Storages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace BroMakerLib
{
    public static class PresetManager
    {
        public static Dictionary<string, MethodInfo> parameters = new Dictionary<string, MethodInfo>();

        public static Dictionary<string, Type> heroesPreset = new Dictionary<string, Type>();
        public static Dictionary<string, Type> customObjectsPreset = new Dictionary<string, Type>();
        public static Dictionary<string, Type> abilities = new Dictionary<string, Type>();
        public static Dictionary<string, Type> grenades = new Dictionary<string, Type>();

        public static bool disableWarnings = false;

        static PresetManager()
        { }

        public static void Initialize()
        {
            heroesPreset = new Dictionary<string, Type>();
            customObjectsPreset = new Dictionary<string, Type>();
            abilities = new Dictionary<string, Type>();
            parameters = new Dictionary<string, MethodInfo>();

            CheckAssembly(Assembly.GetExecutingAssembly());

            // Load assemblies of mods
            foreach (BroMakerMod mod in BroMakerStorage.mods)
            {
                foreach(string assemblyPath in mod.Assemblies)
                {
                    try
                    {
                        var path = Path.Combine(mod.Path, assemblyPath);
                        if (File.Exists(path))
                        {
                            string destFileName = path + ".cache";
                            if (!File.Exists (destFileName))
                            {
                                //File.Delete(destFileName);
                                File.Copy(path, destFileName);
                            }
                            Assembly assembly = Assembly.LoadFile(path);
                            CheckAssembly(assembly);
                        }
                    }
                    catch (Exception ex)
                    {
                        BMLogger.ExceptionLog(assemblyPath, ex);
                    }
                }
            }
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

        public static Type GetGrenadePreset(string name)
        {
            if (!grenades.ContainsKey(name))
                return null;
            return grenades[name];
        }

        public static MethodInfo GetParameterMethod(string name)
        {
            if (!parameters.ContainsKey(name))
                return null;
            return parameters[name];
        }

        public static void CheckAssembly(Assembly assembly)
        {
            if (assembly == null)
                throw new NullReferenceException(nameof(assembly));

            try
            {
                Type[] types = assembly.GetTypes();
                if (types.Length == 0)
                {
                    BMLogger.Warning($"Assembly '{assembly.GetName().Name}' is somehow empty ( ͠° ͟ʖ ͡°)");
                    return;
                }

                RetrieveParameters(types);
                Type[] presets = FindPresets(types);
                AddPresets(presets);
            }
            catch (Exception ex)
            {
                BMLogger.ExceptionLog($"{assembly.FullName} - " + ex);
            }
        }

        private static void RetrieveParameters(Type[] types)
        {
            Type paramerterType = types.FirstOrDefault((t) => t.Name == "Parameters");
            if (paramerterType == null || paramerterType.Name != "Parameters")
                return;

            var methods = paramerterType.GetMethods();
            foreach(var method in methods)
            {
                var attributes = method.GetCustomAttributes(typeof(ParameterAttribute), true);
                if (attributes.IsNotNullOrEmpty())
                {
                    if (method.GetParameters().Length != 2)
                    {
                        BMLogger.Warning($"Parameter '{method.Name}' should have two parameter.");
                        continue;
                    }

                    if (parameters.ContainsKey(method.Name))
                        BMLogger.Warning($"Parameter of name {method.Name} already exist in assembly {paramerterType.Assembly.FullName}");
                    else
                        parameters.Add(method.Name, method);
                }
            }
        }

        private static Type[] FindPresets(Type[] types)
        {
            var result = new List<Type>();

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
            if (attribute is GrenadePresetAttribute)
                return grenades;

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
            if (attribute is GrenadePresetAttribute)
                return "Grenade";
            return string.Empty;
        }
    }
}
