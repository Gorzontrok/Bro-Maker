using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using BroMakerLib.CustomObjects.Bros;
using BroMakerLib.Cutscenes;
using BroMakerLib.Infos;
using BroMakerLib.Loggers;
using BroMakerLib.Unlocks;
using RocketLib.Utils;
using UnityModManagerNet;

namespace BroMakerLib.Storages
{
    public static class BroMakerStorage
    {
        internal const string BroMakerLibName = "BroMakerLib";
        internal const string RocketLibName = "RocketLib";
        private const string AuthorClosing = "It needs an update from its author.";
        private const string MissingLabel = "Missing";
        private const int MaxListedItems = 3;
        private const int MaxListedSites = 3;

        internal static ReadOnlyCollection<BroMakerMod> Mods { get; private set; } = new List<BroMakerMod>().AsReadOnly();

        internal static ReadOnlyCollection<StoredHero> Bros
        {
            get { return _brosReadOnly; }
        }

        internal static ReadOnlyCollection<string> BroNames
        {
            get { return _broNamesReadOnly; }
        }

        private static List<BroMakerMod> _mods = new List<BroMakerMod>();
        private static readonly List<BroMakerMod> _incompatibleMods = new List<BroMakerMod>();
        internal static ReadOnlyCollection<BroMakerMod> IncompatibleMods { get; } = _incompatibleMods.AsReadOnly();
        private static List<StoredHero> _bros = new List<StoredHero>();
        private static ReadOnlyCollection<StoredHero> _brosReadOnly = new List<StoredHero>().AsReadOnly();
        private static Dictionary<string, StoredHero> _brosByName = new Dictionary<string, StoredHero>();
        private static Dictionary<Type, StoredHero> _brosByType = new Dictionary<Type, StoredHero>();
        private static List<string> _broNames = new List<string>();
        private static ReadOnlyCollection<string> _broNamesReadOnly = new List<string>().AsReadOnly();
        private static readonly Dictionary<Assembly, BroMakerMod> _modByAssembly = new Dictionary<Assembly, BroMakerMod>();
        private static Version _installedRocketLib;

        internal static void Initialize()
        {
            _mods = new List<BroMakerMod>();
            Mods = _mods.AsReadOnly();
            LoadMods();

            _bros = new List<StoredHero>();
            _brosByName = new Dictionary<string, StoredHero>();
            _brosByType = new Dictionary<Type, StoredHero>();
            _broNames = new List<string>();
            _modByAssembly.Clear();

            foreach (BroMakerMod mod in _mods)
            {
                StoreCharactersFromMod(mod);
            }

            _broNames.Sort();
            _brosReadOnly = _bros.AsReadOnly();
            _broNamesReadOnly = _broNames.AsReadOnly();

            BMLogger.Debug("BroMakerStorage Initialized.");
        }

        /// <summary>Looks up a loaded mod by name, or null if not found.</summary>
        internal static BroMakerMod GetModByName(string modName)
        {
            if (string.IsNullOrEmpty(modName))
            {
                return null;
            }
            foreach (BroMakerMod mod in _mods)
            {
                if (string.Equals(mod.Name, modName, StringComparison.Ordinal))
                {
                    return mod;
                }
            }
            return null;
        }

        /// <summary>Looks up the owning mod's name for a registered bro.</summary>
        internal static string TryResolveModFromBro(string broName)
        {
            if (string.IsNullOrEmpty(broName))
            {
                return null;
            }
            StoredHero hero;
            if (_brosByName.TryGetValue(broName, out hero) && hero != null && hero.mod != null)
            {
                return hero.mod.Name;
            }
            return null;
        }

        /// <summary>Returns the name of the most deeply nested loaded mod whose folder contains the given path, or null if none does.</summary>
        internal static string TryResolveModFromPath(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return null;
            }
            string file = filePath.Replace('\\', '/');
            BroMakerMod best = null;
            int bestLength = -1;
            foreach (BroMakerMod mod in _mods)
            {
                if (string.IsNullOrEmpty(mod.Path))
                {
                    continue;
                }
                string folder = mod.Path.Replace('\\', '/').TrimEnd('/');
                if (folder.Length > bestLength && IsSameOrUnder(file, folder))
                {
                    best = mod;
                    bestLength = folder.Length;
                }
            }
            return best?.Name;
        }

        private static bool IsSameOrUnder(string path, string folder)
        {
            return path.StartsWith(folder, StringComparison.OrdinalIgnoreCase) && (path.Length == folder.Length || path[folder.Length] == '/');
        }

        /// <summary>Looks up the mod that owns the given Assembly.</summary>
        internal static BroMakerMod GetModByAssembly(Assembly assembly)
        {
            if (assembly == null)
            {
                return null;
            }
            BroMakerMod mod;
            return _modByAssembly.TryGetValue(assembly, out mod) ? mod : null;
        }

        internal static void MarkModIncompatible(BroMakerMod mod, string message, Exception ex = null, bool report = true)
        {
            if (mod == null || _incompatibleMods.Contains(mod))
            {
                return;
            }

            mod.ErrorMessage = message;
            mod.LoadedAnywayWarning = null;
            _incompatibleMods.Add(mod);
            UnregisterMod(mod);
            if (mod.Harmony != null)
            {
                // A null id would remove every mod's patches.
                mod.Harmony.UnpatchAll(mod.Harmony.Id);
            }
            if (report)
            {
                BMLogger.Error(message, ex, mod.Name);
            }
        }

        internal static void UnregisterMod(BroMakerMod mod)
        {
            if (mod == null || mod.StoredHeroes.Length == 0)
            {
                return;
            }

            foreach (StoredHero hero in mod.StoredHeroes)
            {
                if (hero != null)
                {
                    RemoveBro(hero);
                }
            }
            mod.StoredHeroes = new StoredHero[0];

            BroSpawnManager.BroStatusChanged();
        }

        internal static void RemoveBro(StoredHero hero)
        {
            _bros.Remove(hero);
            _broNames.Remove(hero.name);
            BroSpawnManager.RemoveBro(hero);
            BroUnlockManager.RemoveBro(hero);

            StoredHero registered;
            if (_brosByName.TryGetValue(hero.name, out registered) && ReferenceEquals(registered, hero))
            {
                _brosByName.Remove(hero.name);
            }

            List<Type> removedTypes = new List<Type>();
            foreach (KeyValuePair<Type, StoredHero> kvp in _brosByType)
            {
                if (ReferenceEquals(kvp.Value, hero))
                {
                    removedTypes.Add(kvp.Key);
                }
            }
            foreach (Type type in removedTypes)
            {
                _brosByType.Remove(type);
            }
        }

        internal static void RegisterModAssembly(Assembly assembly, BroMakerMod mod)
        {
            if (assembly == null || mod == null)
            {
                return;
            }
            _modByAssembly[assembly] = mod;
        }

        private static void LoadMods()
        {
            BMLogger.Debug("Loading Mods");
            string[] jsonFiles = Directory.GetFiles(DirectoriesManager.StorageDirectory, "*.mod.json", SearchOption.AllDirectories);

            List<BroMakerMod> manifests = new List<BroMakerMod>();
            foreach (string jsonFile in jsonFiles)
            {
                try
                {
                    BroMakerMod mod = BroMakerMod.TryLoad(jsonFile);
                    if (mod != null)
                    {
                        manifests.Add(mod);
                    }
                }
                catch (Exception ex)
                {
                    BMLogger.Error($"Unable to load BroMaker Mod at {jsonFile}", ex, new DirectoryInfo(Path.GetDirectoryName(jsonFile)).Name);
                }
            }

            _installedRocketLib = ModCompatibility.InstalledVersion(RocketLibName);
            using (ReferenceScanner scanner = ModCompatibility.CreateScanner())
            {
                foreach (BroMakerMod mod in manifests)
                {
                    List<KeyValuePair<string, string>> mergedOrder = new List<KeyValuePair<string, string>>();
                    Dictionary<string, List<string>> mergedSites = new Dictionary<string, List<string>>();
                    foreach (KeyValuePair<string, string> dll in ListedDlls(mod, false))
                    {
                        ReferenceScanResult result = scanner.Scan(dll.Value);
                        string name = Path.GetFileName(dll.Value);
                        if (!result.Checked)
                        {
                            mod.UncheckedReason = result.Unreadable
                                ? $"its file '{name}' couldn't be read. Cause: {result.UncheckedReason}"
                                : $"BroMaker couldn't check '{name}' for compatibility. Cause: {result.UncheckedReason}";
                            break;
                        }

                        if (result.Unresolved.Count > 0)
                        {
                            mod.ScanLog.Add($"'{mod.Name}' ({name}) has references BroMaker couldn't follow: {string.Join("; ", new List<string>(result.Unresolved).ToArray())}");
                        }

                        List<string> libraries = new List<string>();
                        List<string> targets = new List<string>();
                        foreach (MissingReference missing in result.Missing)
                        {
                            AddToken(mod, "missing:" + missing.Library + ":" + missing.Target);
                            libraries.Add(missing.Library);
                            targets.Add(missing.UsedIn.Count == 0
                                ? missing.Target
                                : missing.Target + " [in " + string.Join("; ", new List<string>(missing.UsedIn).ToArray()) + "]");

                            string key = missing.Library + ":" + missing.Display;
                            List<string> sites;
                            if (!mergedSites.TryGetValue(key, out sites))
                            {
                                mergedSites[key] = new List<string>(missing.UsedIn);
                                mergedOrder.Add(new KeyValuePair<string, string>(missing.Library, missing.Display));
                                continue;
                            }
                            foreach (string site in missing.UsedIn)
                            {
                                if (sites.Count < MaxListedSites && !sites.Contains(site))
                                {
                                    sites.Add(site);
                                }
                            }
                        }

                        Version broMakerLib = result.ReferencedVersion(BroMakerLibName);
                        Version rocketLib = result.ReferencedVersion(RocketLibName);
                        if (targets.Count > 0)
                        {
                            List<string> builtAgainst = new List<string>();
                            if (libraries.Contains(BroMakerLibName))
                            {
                                builtAgainst.Add(BroMakerLibName + " " + broMakerLib);
                            }
                            if (libraries.Contains(RocketLibName))
                            {
                                builtAgainst.Add(RocketLibName + " " + rocketLib);
                            }
                            mod.MissingDetails.Add(
                                $"'{mod.Name}' ({name}, built against {string.Join(" and ", builtAgainst.ToArray())}) references: {string.Join("; ", targets.ToArray())}");
                        }

                        mod.BroMakerLibReference = Highest(mod.BroMakerLibReference, broMakerLib);
                        mod.RocketLibReference = Highest(mod.RocketLibReference, rocketLib);
                    }

                    foreach (KeyValuePair<string, string> item in mergedOrder)
                    {
                        List<string> sites = mergedSites[item.Key + ":" + item.Value];
                        mod.MissingItems.Add(new KeyValuePair<string, string>(item.Key, sites.Count == 0 ? item.Value : item.Value + " in " + string.Join(", ", sites.ToArray())));
                    }
                }
            }

            foreach (BroMakerMod mod in manifests)
            {
                try
                {
                    foreach (string line in mod.ScanLog)
                    {
                        BMLogger.Log(line);
                    }

                    mod.BuiltFor = mod.BroMakerLibReference != null ? mod.BroMakerLibReference.ToString() : mod.UncheckedReason != null ? string.Empty : Info.VERSION;

                    if (mod.UncheckedReason != null)
                    {
                        MarkModIncompatible(mod, $"'{mod.Name}' wasn't loaded because {mod.UncheckedReason}");
                        LogMissing(mod);
                        continue;
                    }

                    if (mod.MissingItems.Count > 0)
                    {
                        List<string> items = mod.MissingItems.ConvertAll(item => item.Value);
                        string closing;
                        string reason = LibraryReason(mod, mod.MissingItems.ConvertAll(item => item.Key), "it uses ", string.Empty, out closing);
                        if (!LoadAnywayOrRefuse(mod, reason, closing, MissingLabel, items))
                        {
                            continue;
                        }
                    }

                    if (GetModByName(mod.Name) is BroMakerMod loaded)
                    {
                        BroMakerMod kept = loaded;
                        BroMakerMod skipped = mod;
                        if (UnityModManager.ParseVersion(mod.Version) > UnityModManager.ParseVersion(loaded.Version))
                        {
                            mod.Initialize();
                            _mods[_mods.IndexOf(loaded)] = mod;
                            kept = mod;
                            skipped = loaded;
                        }
                        BMLogger.Warning($"Two copies of the mod '{mod.Name}' are installed: the one at '{kept.Path}' was loaded and the one at '{skipped.Path}' was not.",
                            mod: mod.Name);
                    }
                    else
                    {
                        mod.Initialize();
                        _mods.Add(mod);
                    }
                }
                catch (Exception ex)
                {
                    BMLogger.Error($"Unable to load BroMaker Mod at {mod.Path}", ex, mod.Name);
                }
            }

            BMLogger.Debug("Finish Loading Mods");
        }

        private static Version Highest(Version first, Version second)
        {
            if (first == null)
            {
                return second;
            }
            return second != null && second > first ? second : first;
        }

        internal static List<KeyValuePair<string, string>> ListedDlls(BroMakerMod mod, bool report)
        {
            List<KeyValuePair<string, string>> dlls = new List<KeyValuePair<string, string>>();
            foreach (string entry in mod.Assemblies)
            {
                if (string.IsNullOrEmpty(entry))
                {
                    if (report)
                    {
                        BMLogger.Warning("The mod's Assemblies list has an empty entry, which was skipped");
                    }
                    continue;
                }

                string path;
                try
                {
                    path = Path.Combine(mod.Path, entry);
                }
                catch (ArgumentException ex)
                {
                    if (report)
                    {
                        BMLogger.Error($"Error loading assembly {entry}", ex);
                    }
                    continue;
                }

                if (!File.Exists(path))
                {
                    if (report)
                    {
                        BMLogger.Warning($"Can't find '{entry}' at '{path}', so it wasn't loaded");
                    }
                    continue;
                }
                dlls.Add(new KeyValuePair<string, string>(entry, path));
            }
            return dlls;
        }

        internal static void AddToken(BroMakerMod mod, string token)
        {
            int index = mod.RefusalTokens.BinarySearch(token, StringComparer.Ordinal);
            if (index < 0)
            {
                mod.RefusalTokens.Insert(~index, token);
            }
        }

        internal static bool TokensAccepted(BroMakerMod mod)
        {
            List<string> accepted;
            if (!Settings.instance.loadAnywayMods.TryGetValue(mod.Name, out accepted) || accepted == null)
            {
                return false;
            }

            foreach (string token in mod.RefusalTokens)
            {
                if (!accepted.Contains(token))
                {
                    return false;
                }
            }
            return true;
        }

        internal static bool LoadAnywayOrRefuse(BroMakerMod mod, string reason, string closing, string label, List<string> items)
        {
            if (TokensAccepted(mod))
            {
                if (mod.LoadedAnywayWarning == null)
                {
                    string text = $"'{mod.Name}' is loaded anyway, though {reason}. It may throw errors or break parts of the game.";
                    bool listed = label == MissingLabel;
                    mod.LoadedAnywayWarning = listed ? text + ListTail(label, items, false) : text;
                    mod.ErrorMessage = listed ? text + ListTail(label, items, true) : text;
                }
                return true;
            }

            mod.CanLoadAnyway = true;
            string refusal = $"'{mod.Name}' wasn't loaded because {reason}. {closing}";
            MarkModIncompatible(mod, refusal + ListTail(label, items, false));
            mod.ErrorMessage = refusal + ListTail(label, items, true);
            LogMissing(mod);
            return false;
        }

        internal static void LogMissing(BroMakerMod mod)
        {
            foreach (string line in mod.MissingDetails)
            {
                BMLogger.Log(line);
            }
        }

        private static string ListTail(string label, List<string> items, bool fullList)
        {
            if (items == null)
            {
                return string.Empty;
            }

            string list;
            if (fullList || items.Count <= MaxListedItems)
            {
                list = string.Join("; ", items.ToArray());
            }
            else
            {
                list = string.Join("; ", items.GetRange(0, MaxListedItems).ToArray()) + "; and " + (items.Count - MaxListedItems) + " more";
            }
            return $" {label}: {list}.";
        }

        internal static string LibraryReason(BroMakerMod mod, List<string> libraries, string lead, string tail, out string closing)
        {
            List<string> parts = new List<string>();
            List<string> names = new List<string>();
            bool allNewer = true;
            foreach (string library in new[] { BroMakerLibName, RocketLibName })
            {
                if (!libraries.Contains(library))
                {
                    continue;
                }

                bool isRocketLib = library == RocketLibName;
                string name = isRocketLib ? RocketLibName : "BroMaker";
                Version builtFor = isRocketLib ? mod.RocketLibReference : mod.BroMakerLibReference;
                Version installed = isRocketLib ? _installedRocketLib : Info.ParsedVersion;
                string current = installed != null ? installed.ToString() : "the installed version";
                bool comparable = builtFor != null && installed != null;
                bool newer = comparable && builtFor > installed;
                allNewer &= newer;
                names.Add(name);
                if (newer)
                {
                    parts.Add($"parts of {name} {builtFor} that {current} doesn't have yet");
                }
                else if (comparable && builtFor < installed)
                {
                    parts.Add($"parts of {name} {builtFor} that {current} no longer has");
                }
                else
                {
                    parts.Add($"parts of {name} that {current} doesn't have");
                }
            }

            closing = allNewer ? $"Update {string.Join(" and ", names.ToArray())} to use it." : AuthorClosing;
            return lead + string.Join(" and ", parts.ToArray()) + tail;
        }

        internal static void PopulateTypesDictionary()
        {
            _brosByType.Clear();

            foreach (StoredHero hero in _bros)
            {
                Type heroType = PresetManager.GetHeroPreset(hero.GetInfo().CharacterPreset);
                if (heroType != null)
                {
                    _brosByType[heroType] = hero;
                }
            }
        }

        public static bool GetStoredHeroByName(string name, out StoredHero hero)
        {
            hero = null;

            if (name.IsNullOrEmpty())
            {
                return false;
            }

            return _brosByName.TryGetValue(name, out hero);
        }

        public static bool GetStoredHeroByCustomHeroType<T>(out StoredHero hero) where T : CustomHero
        {
            return _brosByType.TryGetValue(typeof(T), out hero);
        }

        public static bool GetStoredHeroByCustomHeroType(Type customHeroType, out StoredHero hero)
        {
            hero = null;

            if (customHeroType == null || !typeof(CustomHero).IsAssignableFrom(customHeroType))
            {
                return false;
            }

            return _brosByType.TryGetValue(customHeroType, out hero);
        }

        internal static void StoreCharactersFromMod(BroMakerMod mod)
        {
            if (mod == null || mod.CustomBros == null || mod.CustomBros.Length == 0)
            {
                return;
            }

            List<StoredHero> temp = new List<StoredHero>();
            List<string> names = new List<string>();
            foreach (object broObject in mod.CustomBros)
            {
                if (broObject as string != null && broObject.As<string>().IsNotNullOrEmpty())
                {
                    string path = Path.Combine(mod.Path, broObject as string);
                    if (File.Exists(path))
                    {
                        StoredHero hero = new StoredHero(path, mod);
                        names.Add(hero.name);
                        if (hero.info == null || !TryRegisterBro(hero))
                        {
                            continue;
                        }
                        temp.Add(hero);
                        BMLogger.Debug($"Found file: '{path}'");
                    }
                }
                else if (broObject as CustomBroInfo != null)
                {
                    CustomBroInfo info = broObject as CustomBroInfo;
                    using (BMLogger.Scope(mod.Name, info.name))
                    {
                        try
                        {
                            info.path = mod.Path;
                            info.Mod = mod;
                            foreach (CustomIntroCutscene cutscene in info.Cutscene)
                            {
                                cutscene.path = mod.Path;
                                cutscene.Owner = info;
                            }
                        }
                        catch (Exception ex)
                        {
                            BMLogger.Error($"Failed to load the bro '{info.name}' written in the mod's manifest", ex);
                            continue;
                        }
                        info.WarnUnknownKeys(mod.ManifestFileName);
                    }

                    StoredHero hero = new StoredHero(info, mod);
                    names.Add(hero.name);
                    if (TryRegisterBro(hero))
                    {
                        temp.Add(hero);
                    }
                }
            }

            mod.StoredHeroes = temp.ToArray();
            mod.BrosNames = names.ToArray();
        }

        private static bool TryRegisterBro(StoredHero hero)
        {
            StoredHero existing;
            if (_brosByName.TryGetValue(hero.name, out existing))
            {
                BMLogger.Warning($"Two bros named '{hero.name}' are installed: the one at '{existing.path}' was loaded and the one at '{hero.path}' was not.", mod: hero.mod?.Name,
                    bro: hero.name);
                return false;
            }

            _bros.Add(hero);
            _brosByName[hero.name] = hero;
            _broNames.Add(hero.name);
            BroSpawnManager.AddBroIfEnabled(hero);
            return true;
        }
    }
}
