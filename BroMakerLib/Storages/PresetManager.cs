using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private static Dictionary<string, MethodInfo> _parameters = new Dictionary<string, MethodInfo>();

        private static Dictionary<string, Type> _heroesPreset = new Dictionary<string, Type>();
        private static Dictionary<string, Type> _customObjectsPreset = new Dictionary<string, Type>();
        private static Dictionary<string, Type> _specialPresets = new Dictionary<string, Type>();
        private static Dictionary<string, Type> _meleePresets = new Dictionary<string, Type>();
        private static Dictionary<string, Type> _passivePresets = new Dictionary<string, Type>();

        /// <summary>The live registry of `[Parameter]` methods, keyed by parameter name.</summary>
        public static Dictionary<string, MethodInfo> parameters => _parameters;

        /// <summary>The live registry of hero preset types, keyed by preset name.</summary>
        public static Dictionary<string, Type> heroesPreset => _heroesPreset;

        /// <summary>The live registry of custom object preset types, keyed by preset name.</summary>
        public static Dictionary<string, Type> customObjectsPreset => _customObjectsPreset;

        internal static void Initialize()
        {
            _heroesPreset = new Dictionary<string, Type>();
            _customObjectsPreset = new Dictionary<string, Type>();
            _specialPresets = new Dictionary<string, Type>();
            _meleePresets = new Dictionary<string, Type>();
            _passivePresets = new Dictionary<string, Type>();
            _parameters = new Dictionary<string, MethodInfo>();

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

                    List<KeyValuePair<string, Assembly>> assemblies = new List<KeyValuePair<string, Assembly>>();
                    foreach (KeyValuePair<string, string> dll in BroMakerStorage.ListedDlls(mod, true))
                    {
                        Assembly assembly;
                        try
                        {
                            assembly = Assembly.LoadFile(dll.Value);
                            // Assembly.LoadFile doesn't resolve types, so GetTypes surfaces type-load failures here
                            assembly.GetTypes();
                        }
                        catch (Exception ex)
                        {
                            string message = BMLogger.IsIncompatibleAssembly(ex) ? IncompatibleRefusal(mod, ex) : $"'{mod.Name}' wasn't loaded because its file '{dll.Key}' couldn't be loaded. Cause: {ex.Message}";
                            BroMakerStorage.MarkModIncompatible(mod, message, ex);
                            assemblies = null;
                            break;
                        }
                        assemblies.Add(new KeyValuePair<string, Assembly>(dll.Key, assembly));
                    }
                    if (assemblies == null)
                    {
                        continue;
                    }

                    List<string> libraries = new List<string>();
                    List<string> targets = new List<string>();
                    List<string> items = new List<string>();
                    foreach (KeyValuePair<string, Assembly> entry in assemblies)
                    {
                        foreach (Type type in entry.Value.GetTypes())
                        {
                            try
                            {
                                Type libraryBase = null;
                                for (Type current = type.BaseType; current != null; current = current.BaseType)
                                {
                                    string assemblyName = current.Assembly.GetName().Name;
                                    if (assemblyName == BroMakerStorage.BroMakerLibName || assemblyName == BroMakerStorage.RocketLibName)
                                    {
                                        libraryBase = current;
                                        break;
                                    }
                                }
                                if (libraryBase == null)
                                {
                                    continue;
                                }

                                foreach (MethodInfo method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                                {
                                    if (!method.IsVirtual || (method.Attributes & MethodAttributes.NewSlot) != 0)
                                    {
                                        continue;
                                    }

                                    MethodInfo definition = method.GetBaseDefinition();
                                    if (definition.DeclaringType == method.DeclaringType && definition.MetadataToken == method.MetadataToken)
                                    {
                                        string target = type.FullName + "::" + method.Name + "(" + ParameterList(method, true) + ")";
                                        string item = $"{ShortName(type)}.{method.Name}({ParameterList(method, false)}), which overrides a method {ShortName(libraryBase)} no longer has";
                                        libraries.Add(libraryBase.Assembly.GetName().Name);
                                        targets.Add(target);
                                        items.Add(item);
                                    }
                                }
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                    if (items.Count > 0)
                    {
                        foreach (string target in targets)
                        {
                            BroMakerStorage.AddToken(mod, "orphan:" + target);
                        }
                        string closing;
                        string reason = BroMakerStorage.LibraryReason(mod, libraries, "it builds on ", ", so some of its code would never run", out closing);
                        bool loaded = BroMakerStorage.LoadAnywayOrRefuse(mod, reason, closing, "Never called", items);
                        BMLogger.Log($"'{mod.Name}' never calls: {string.Join("; ", targets.ToArray())}");
                        if (!loaded)
                        {
                            continue;
                        }
                    }

                    foreach (KeyValuePair<string, Assembly> entry in assemblies)
                    {
                        string assemblyPath = entry.Key;
                        Assembly assembly = entry.Value;
                        BroMakerStorage.RegisterModAssembly(assembly, mod);
                        bool flag = CheckAssembly(assembly);
                        if (BroMakerStorage.IncompatibleMods.Contains(mod))
                        {
                            break;
                        }

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

            foreach (BroMakerMod mod in BroMakerStorage.Mods)
            {
                if (mod.StoredHeroes.Length == 0 || BroMakerStorage.IncompatibleMods.Contains(mod))
                {
                    continue;
                }

                List<string> dropped = new List<string>();
                string message = null;
                foreach (StoredHero hero in mod.StoredHeroes)
                {
                    if (hero == null || hero.info == null || GetHeroPreset(hero.info.CharacterPreset) != null)
                    {
                        continue;
                    }

                    message = $"'{hero.name}' wasn't loaded because its characterPreset '{hero.info.CharacterPreset}' doesn't exist in BroMaker {Info.VERSION}. If it comes from another mod, that mod isn't installed or wasn't loaded.";
                    BroMakerStorage.RemoveBro(hero);
                    hero.mod.StoredHeroes = Array.FindAll(hero.mod.StoredHeroes, stored => !ReferenceEquals(stored, hero));
                    BroSpawnManager.BroStatusChanged();
                    BMLogger.Error(message, mod: mod.Name, bro: hero.name);
                    dropped.Add($"{hero.name} ('{hero.info.CharacterPreset}')");
                }

                if (dropped.Count > 0 && mod.StoredHeroes.Length == 0)
                {
                    if (dropped.Count > 1)
                    {
                        message = $"'{mod.Name}' wasn't loaded because none of its bros has a characterPreset that exists in BroMaker {Info.VERSION}: {string.Join(", ", dropped.ToArray())}. If a preset comes from another mod, that mod isn't installed or wasn't loaded.";
                    }
                    BroMakerStorage.MarkModIncompatible(mod, message, report: false);
                }
            }

            foreach (BroMakerMod mod in BroMakerStorage.Mods)
            {
                if (mod.LoadedAnywayWarning != null)
                {
                    BMLogger.Warning(mod.LoadedAnywayWarning, mod: mod.Name);
                    BroMakerStorage.LogMissing(mod);
                }
            }

            BroMakerStorage.PopulateTypesDictionary();
        }

        private static string ParameterList(MethodInfo method, bool fullNames)
        {
            ParameterInfo[] parameters = method.GetParameters();
            string[] names = new string[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                names[i] = fullNames ? parameters[i].ParameterType.ToString() : ShortName(parameters[i].ParameterType);
            }
            return string.Join(", ", names);
        }

        private static string ShortName(Type type)
        {
            int arity = type.Name.IndexOf('`');
            return arity < 0 ? type.Name : type.Name.Substring(0, arity);
        }

        private static string IncompatibleRefusal(BroMakerMod mod, Exception ex)
        {
            return $"'{mod.Name}' wasn't loaded because it uses {BMLogger.MissingCodePhrase}. It was probably built for an older BroMaker and needs an update from its author, or one of its own dependencies is missing or out of date. Cause: {BMLogger.MissingMemberMessage(ex)}";
        }

        private static Type GetPreset(Dictionary<string, Type> collection, string name)
        {
            Type preset;
            return name != null && collection.TryGetValue(name, out preset) ? preset : null;
        }

        private static ReadOnlyCollection<KeyValuePair<string, Type>> GetAllPresets(Dictionary<string, Type> collection)
        {
            return new List<KeyValuePair<string, Type>>(collection).AsReadOnly();
        }

        /// <summary>Returns the hero type registered under a name, or null if no such preset exists.</summary>
        public static Type GetHeroPreset(string presetName)
        {
            return GetPreset(_heroesPreset, presetName);
        }

        /// <summary>Returns the custom-object type registered under a name, or null if no such preset exists.</summary>
        internal static Type GetCustomObjectPreset(string name)
        {
            return GetPreset(_customObjectsPreset, name);
        }

        /// <summary>Returns the special ability type registered under a name, or null if no such preset exists.</summary>
        public static Type GetSpecialPreset(string name)
        {
            return GetPreset(_specialPresets, name);
        }

        /// <summary>Returns the melee ability type registered under a name, or null if no such preset exists.</summary>
        public static Type GetMeleePreset(string name)
        {
            return GetPreset(_meleePresets, name);
        }

        /// <summary>Returns the passive ability type registered under a name, or null if no such preset exists.</summary>
        public static Type GetPassivePreset(string name)
        {
            return GetPreset(_passivePresets, name);
        }

        /// <summary>Returns every registered hero preset as name/type pairs.</summary>
        internal static ReadOnlyCollection<KeyValuePair<string, Type>> GetAllHeroPresets()
        {
            return GetAllPresets(_heroesPreset);
        }

        /// <summary>Returns every registered special ability preset as name/type pairs.</summary>
        public static ReadOnlyCollection<KeyValuePair<string, Type>> GetAllSpecialPresets()
        {
            return GetAllPresets(_specialPresets);
        }

        /// <summary>Returns every registered melee ability preset as name/type pairs.</summary>
        public static ReadOnlyCollection<KeyValuePair<string, Type>> GetAllMeleePresets()
        {
            return GetAllPresets(_meleePresets);
        }

        /// <summary>Returns every registered passive ability preset as name/type pairs.</summary>
        public static ReadOnlyCollection<KeyValuePair<string, Type>> GetAllPassivePresets()
        {
            return GetAllPresets(_passivePresets);
        }

        public static MethodInfo GetParameterMethod(string name)
        {
            MethodInfo method;
            return name != null && _parameters.TryGetValue(name, out method) ? method : null;
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
                BroMakerMod mod = BMLogger.IsIncompatibleAssembly(ex) ? BroMakerStorage.GetModByAssembly(assembly) : null;
                if (mod != null)
                {
                    BroMakerStorage.MarkModIncompatible(mod, IncompatibleRefusal(mod, ex), ex);
                }
                else
                {
                    BMLogger.Error($"Failed to scan assembly '{assembly.GetName().Name}'", ex);
                }
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

                    if (_parameters.ContainsKey(method.Name))
                    {
                        BMLogger.Warning($"A parameter named '{method.Name}' already exists, so the one in {paramerterType.Assembly.GetName().Name} is ignored", mod: BroMakerStorage.GetModByAssembly(paramerterType.Assembly)?.Name);
                    }
                    else
                    {
                        _parameters.Add(method.Name, method);
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
                return _heroesPreset;
            }

            if (attribute is SpecialPresetAttribute)
            {
                return _specialPresets;
            }

            if (attribute is MeleePresetAttribute)
            {
                return _meleePresets;
            }

            if (attribute is PassivePresetAttribute)
            {
                return _passivePresets;
            }

            return _customObjectsPreset;
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
                Type winningType = collection[name];
                BMLogger.Warning($"{(collectionName.IsNullOrEmpty() ? "Preset" : collectionName + " preset")} '{name}' from {preset} in {preset.Assembly.GetName().Name} was ignored because {winningType} in {winningType.Assembly.GetName().Name} already uses that name", mod: BroMakerStorage.GetModByAssembly(preset.Assembly)?.Name);
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
