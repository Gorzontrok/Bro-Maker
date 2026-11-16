using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public static Dictionary<string, Type> abilities = new Dictionary<string, Type>();
        public static Dictionary<string, Type> grenades = new Dictionary<string, Type>();

        public static bool disableWarnings = false;

        public static void Initialize()
        {
            heroesPreset = new Dictionary<string, Type>();
            customObjectsPreset = new Dictionary<string, Type>();
            abilities = new Dictionary<string, Type>();
            parameters = new Dictionary<string, MethodInfo>();

            CheckAssembly(Assembly.GetExecutingAssembly());

            // Load assemblies of mods
            foreach (BroMakerMod mod in BroMakerStorage.Mods)
            {
                using (BMLogger.Scope(mod.Name, null))
                {
                    if (mod.HasHarmonyPatch)
                    {
                        mod.Harmony = new Harmony(mod.Name);
                    }

                    foreach (string assemblyPath in mod.Assemblies)
                    {
                        string path = Path.Combine(mod.Path, assemblyPath);
                        if (!File.Exists(path))
                        {
                            continue;
                        }

                        Assembly assembly = Assembly.LoadFile(path);
                        BroMakerStorage.RegisterModAssembly(assembly, mod);
                        bool flag = CheckAssembly(assembly);
                        if (flag && mod.Harmony != null)
                        {
                            try
                            {
                                mod.Harmony.PatchAll(assembly);
                                BMLogger.Log($"{assemblyPath} patched!");
                            }
                            catch (Exception ex)
                            {
                                BMLogger.Error($"Error patching {assemblyPath}", ex);
                            }
                        }
                    }
                }
            }

            BroMakerStorage.PopulateTypesDictionary();
        }

        private static Type GetPreset(Dictionary<string, Type> collection, string name)
        {
            Type preset;
            return collection.TryGetValue(name, out preset) ? preset : null;
        }

        private static ReadOnlyCollection<KeyValuePair<string, Type>> GetAllPresets(Dictionary<string, Type> collection)
        {
            return new List<KeyValuePair<string, Type>>(collection).AsReadOnly();
        }

        /// <summary>Returns the hero type registered under a name, or null if no such preset exists.</summary>
        public static Type GetHeroPreset(string presetName)
        {
            return GetPreset(heroesPreset, presetName);
        }

        /// <summary>Returns the custom-object type registered under a name, or null if no such preset exists.</summary>
        internal static Type GetCustomObjectPreset(string name)
        {
            return GetPreset(customObjectsPreset, name);
        }

        /// <summary>Returns every registered hero preset as name/type pairs.</summary>
        internal static ReadOnlyCollection<KeyValuePair<string, Type>> GetAllHeroPresets()
        {
            return GetAllPresets(heroesPreset);
        }

        public static Type GetAbilityPreset(string name)
        {
            if (!abilities.ContainsKey(name))
            {
                return null;
            }

            return abilities[name];
        }

        public static Type GetGrenadePreset(string name)
        {
            if (!grenades.ContainsKey(name))
            {
                return null;
            }

            return grenades[name];
        }

        public static MethodInfo GetParameterMethod(string name)
        {
            MethodInfo method;
            return parameters.TryGetValue(name, out method) ? method : null;
        }

        public static bool CheckAssembly(Assembly assembly)
        {
            if (assembly == null)
            {
                return false;
            }

            try
            {
                Type[] types = assembly.GetTypes();
                if (types.Length == 0)
                {
                    BMLogger.Warning($"Assembly '{assembly.GetName().Name}' is empty");
                    return false;
                }


                RetrieveParameters(types);
                Type[] presets = FindPresets(types);
                AddPresets(presets);
            }
            catch (Exception ex)
            {
                BMLogger.Error($"Failed to scan assembly '{assembly.GetName().Name}'", ex);
                return false;
            }

            return true;
        }

        private static void RetrieveParameters(Type[] types)
        {
            Type paramerterType = types.FirstOrDefault((t) => t.Name == "Parameters");
            if (paramerterType == null || paramerterType.Name != "Parameters")
            {
                return;
            }

            MethodInfo[] methods = paramerterType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            foreach (MethodInfo method in methods)
            {
                object[] attributes = method.GetCustomAttributes(typeof(ParameterAttribute), true);
                if (attributes.IsNotNullOrEmpty())
                {
                    if (method.GetParameters().Length != 2)
                    {
                        BMLogger.Warning($"Parameter '{method.Name}' must take exactly two arguments", mod: BroMakerStorage.GetModByAssembly(paramerterType.Assembly)?.Name);
                        continue;
                    }

                    if (parameters.ContainsKey(method.Name))
                    {
                        BMLogger.Warning($"A parameter named '{method.Name}' already exists, so the one in {paramerterType.Assembly.GetName().Name} is ignored", mod: BroMakerStorage.GetModByAssembly(paramerterType.Assembly)?.Name);
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
            List<Type> result = new List<Type>();

            foreach (Type type in types)
            {
                object[] attributes = type.GetCustomAttributes(typeof(CustomObjectPresetAttribute), true);
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
                object[] attributes = type.GetCustomAttributes(typeof(CustomObjectPresetAttribute), true);
                if (attributes.IsNotNullOrEmpty())
                {
                    object first = attributes[0];
                    try
                    {
                        Dictionary<string, Type> presetCollection = GetPresetCollection(first);

                        if (presetCollection == null)
                        {
                            continue;
                        }

                        AddPresetToCollection(presetCollection, type, first.As<CustomObjectPresetAttribute>().name, GetCollectionName(first));
                    }
                    catch (Exception ex)
                    {
                        BMLogger.Error($"Failed to register preset '{first.As<CustomObjectPresetAttribute>().name}'", ex, BroMakerStorage.GetModByAssembly(type.Assembly)?.Name);
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

            if (attribute is AbilityPresetAttribute)
            {
                return abilities;
            }

            if (attribute is GrenadePresetAttribute)
            {
                return grenades;
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
                    Type winningType = collection[name];
                    BMLogger.Warning($"{(collectionName.IsNullOrEmpty() ? "Preset" : collectionName + " preset")} '{name}' from {preset} in {preset.Assembly.GetName().Name} was ignored because {winningType} in {winningType.Assembly.GetName().Name} already uses that name", mod: BroMakerStorage.GetModByAssembly(preset.Assembly)?.Name);
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

            if (attribute is AbilityPresetAttribute)
            {
                return "Ability";
            }

            if (attribute is GrenadePresetAttribute)
            {
                return "Grenade";
            }

            return string.Empty;
        }
    }
}
