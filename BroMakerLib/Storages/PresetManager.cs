using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BroMakerLib.Attributes;
using BroMakerLib.Loggers;
using BroMakerLib.Storages;
using HarmonyLib;

namespace BroMakerLib
{
    public static class PresetManager
    {
        public static Dictionary<string, MethodInfo> parameters = new Dictionary<string, MethodInfo>();

        public static Dictionary<string, Type> heroesPreset = new Dictionary<string, Type>();
        public static Dictionary<string, Type> customObjectsPreset = new Dictionary<string, Type>();
        public static Dictionary<string, Type> specialPresets = new Dictionary<string, Type>();
        public static Dictionary<string, Type> meleePresets = new Dictionary<string, Type>();
        public static Dictionary<string, Type> passivePresets = new Dictionary<string, Type>();

        public static bool disableWarnings = false;

        static PresetManager()
        {
        }

        public static void Initialize()
        {
            heroesPreset = new Dictionary<string, Type>();
            customObjectsPreset = new Dictionary<string, Type>();
            specialPresets = new Dictionary<string, Type>();
            meleePresets = new Dictionary<string, Type>();
            passivePresets = new Dictionary<string, Type>();
            parameters = new Dictionary<string, MethodInfo>();

            CheckAssembly(Assembly.GetExecutingAssembly());

            // Load assemblies of mods
            foreach (var mod in BroMakerStorage.mods)
            {
                if (mod.HasHarmonyPatch)
                {
                    mod.Harmony = new Harmony(mod.Name);
                }

                foreach (var assemblyPath in mod.Assemblies)
                {
                    var path = Path.Combine(mod.Path, assemblyPath);
                    if (!File.Exists(path))
                    {
                        continue;
                    }

                    var assembly = Assembly.LoadFile(path);
                    var flag = CheckAssembly(assembly);
                    if (flag && mod.Harmony != null)
                    {
                        try
                        {
                            mod.Harmony.PatchAll(assembly);
                            BMLogger.Log($"{assemblyPath} patched!");
                        }
                        catch (Exception ex)
                        {
                            BMLogger.ExceptionLog($"Error patching {assemblyPath}", ex);
                        }
                    }
                }
            }

            BroMakerStorage.PopulateTypesDictionary();
        }

        public static Type GetHeroPreset(string presetName)
        {
            if (!heroesPreset.ContainsKey(presetName))
            {
                return null;
            }

            return heroesPreset[presetName];
        }

        public static Type GetSpecialPreset(string name)
        {
            if (!specialPresets.ContainsKey(name))
            {
                return null;
            }

            return specialPresets[name];
        }

        public static Type GetMeleePreset(string name)
        {
            if (!meleePresets.ContainsKey(name))
            {
                return null;
            }

            return meleePresets[name];
        }

        public static Type GetPassivePreset(string name)
        {
            if (!passivePresets.ContainsKey(name))
            {
                return null;
            }

            return passivePresets[name];
        }

        public static Dictionary<string, Type> GetAllSpecialPresets()
        {
            return new Dictionary<string, Type>(specialPresets);
        }

        public static Dictionary<string, Type> GetAllMeleePresets()
        {
            return new Dictionary<string, Type>(meleePresets);
        }

        public static Dictionary<string, Type> GetAllPassivePresets()
        {
            return new Dictionary<string, Type>(passivePresets);
        }

        public static MethodInfo GetParameterMethod(string name)
        {
            if (!parameters.ContainsKey(name))
            {
                return null;
            }

            return parameters[name];
        }

        public static bool CheckAssembly(Assembly assembly)
        {
            if (assembly == null)
            {
                return false;
            }

            try
            {
                var types = assembly.GetTypes();
                if (types.Length == 0)
                {
                    BMLogger.Warning($"Assembly '{assembly.GetName().Name}' is somehow empty ( ͠° ͟ʖ ͡°)");
                    return false;
                }


                RetrieveParameters(types);
                var presets = FindPresets(types);
                AddPresets(presets);
            }
            catch (Exception ex)
            {
                BMLogger.ExceptionLog($"{assembly.FullName} - " + ex);
                return false;
            }

            return true;
        }

        private static void RetrieveParameters(Type[] types)
        {
            var paramerterType = types.FirstOrDefault((t) => t.Name == "Parameters");
            if (paramerterType == null || paramerterType.Name != "Parameters")
            {
                return;
            }

            var methods = paramerterType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            foreach (var method in methods)
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
                    {
                        BMLogger.Warning($"Parameter of name {method.Name} already exist in assembly {paramerterType.Assembly.FullName}");
                    }
                    else
                    {
                        parameters.Add(method.Name, method);
                    }
                }
            }
        }

        private static Type[] FindPresets(Type[] types)
        {
            var result = new List<Type>();

            foreach (var type in types)
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
            foreach (var type in types)
            {
                var attributes = type.GetCustomAttributes(typeof(CustomObjectPresetAttribute), true);
                if (attributes.IsNotNullOrEmpty())
                {
                    var first = attributes[0];
                    try
                    {
                        var presetCollection = GetPresetCollection(first);

                        if (presetCollection == null)
                        {
                            continue;
                        }

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
            {
                return null;
            }

            if (attribute is HeroPresetAttribute)
            {
                return heroesPreset;
            }

            if (attribute is SpecialPresetAttribute)
            {
                return specialPresets;
            }

            if (attribute is MeleePresetAttribute)
            {
                return meleePresets;
            }

            if (attribute is PassivePresetAttribute)
            {
                return passivePresets;
            }

            return customObjectsPreset;
        }

        private static void AddPresetToCollection(Dictionary<string, Type> collection, Type preset, string name, string collectionName = "")
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (preset == null)
            {
                throw new ArgumentNullException(nameof(preset));
            }

            if (name.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(name), "is null or empty.");
            }

            if (collection.ContainsKey(name))
            {
                if (!disableWarnings)
                {
                    BMLogger.Warning($"{collectionName} Preset of name {name} already exist. Type: {preset} ; Assembly: {preset.Assembly.FullName}");
                }
            }
            else
            {
                collection.Add(name, preset);
            }
        }

        private static string GetCollectionName(object attribute)
        {
            if (attribute is HeroPresetAttribute)
            {
                return "Hero";
            }

            if (attribute is SpecialPresetAttribute)
            {
                return "Special";
            }

            if (attribute is MeleePresetAttribute)
            {
                return "Melee";
            }

            if (attribute is PassivePresetAttribute)
            {
                return "Passive";
            }

            return string.Empty;
        }
    }
}