using System;
using System.Collections.Generic;
using BroMakerLib.Attributes;
using BroMakerLib.CustomObjects;
using BroMakerLib.Infos;
using BroMakerLib.Loggers;
using Newtonsoft.Json.Linq;

namespace BroMakerLib.Abilities
{
    /// <summary>Creates ability instances from JSON configuration.</summary>
    public static class AbilityFactory
    {
        /// <summary>Instantiates a `SpecialAbility` from a JSON config block containing a `"preset"` key.</summary>
        /// <returns>The instantiated ability, or null on failure.</returns>
        public static SpecialAbility CreateSpecial(JObject config, BroBase owner)
        {
            if (config == null) return null;

            string presetName = config.Value<string>("preset");
            if (string.IsNullOrEmpty(presetName))
            {
                BMLogger.Warning("Special ability config is missing a 'preset' field, skipped.");
                return null;
            }

            Type type = PresetManager.GetSpecialPreset(presetName);
            if (type == null)
            {
                BMLogger.Warning($"Special preset '{presetName}' not found. Valid presets: {string.Join(", ", new List<string>(PresetManager.GetAllSpecialPresets().Keys).ToArray())}");
                return null;
            }

            try
            {
                var ability = (SpecialAbility)Activator.CreateInstance(type);
                ApplyJsonOverrides(ability, config, owner);
                ability.Initialize(owner);
                return ability;
            }
            catch (Exception ex)
            {
                BMLogger.ExceptionLog($"Failed to create special preset '{presetName}'", ex);
                return null;
            }
        }

        /// <summary>Instantiates a `MeleeAbility` from a JSON config block containing a `"preset"` key.</summary>
        /// <returns>The instantiated ability, or null on failure.</returns>
        public static MeleeAbility CreateMelee(JObject config, BroBase owner)
        {
            if (config == null) return null;

            string presetName = config.Value<string>("preset");
            if (string.IsNullOrEmpty(presetName))
            {
                BMLogger.Warning("Melee ability config is missing a 'preset' field, skipped.");
                return null;
            }

            Type type = PresetManager.GetMeleePreset(presetName);
            if (type == null)
            {
                BMLogger.Warning($"Melee preset '{presetName}' not found. Valid presets: {string.Join(", ", new List<string>(PresetManager.GetAllMeleePresets().Keys).ToArray())}");
                return null;
            }

            try
            {
                var ability = (MeleeAbility)Activator.CreateInstance(type);
                ApplyJsonOverrides(ability, config, owner);
                ability.Initialize(owner);
                return ability;
            }
            catch (Exception ex)
            {
                BMLogger.ExceptionLog($"Failed to create melee preset '{presetName}'", ex);
                return null;
            }
        }

        /// <summary>Instantiates all passive abilities from a bro's `"passives"` JSON list, filtering
        /// redundant passives, deduplicating by concrete type, and enforcing `ConflictsWithPreset`
        /// declarations (bypassable via `"allowConflict": true` on an individual passive's JSON).</summary>
        public static List<PassiveAbility> CreatePassives(List<JObject> configs, BroBase owner)
        {
            var result = new List<PassiveAbility>();
            if (configs == null) return result;

            var seenTypes = new Dictionary<Type, string>();
            var accepted = new List<KeyValuePair<string, PassiveAbility>>();

            foreach (var config in configs)
            {
                if (config == null) continue;
                string presetName = config.Value<string>("preset");
                var ability = CreatePassiveInternal(config, presetName, owner);
                if (ability == null) continue;

                if (seenTypes.ContainsKey(ability.GetType()))
                {
                    string existingPreset = seenTypes[ability.GetType()];
                    BMLogger.Warning($"Passive preset '{presetName}' is a duplicate of already-attached preset '{existingPreset}', skipping.");
                    ability.Cleanup();
                    continue;
                }
                seenTypes.Add(ability.GetType(), presetName);

                bool allowConflict = config.Value<bool?>("allowConflict") ?? false;
                string conflictWith;
                if (!allowConflict && HasConflict(ability, presetName, accepted, out conflictWith))
                {
                    BMLogger.Warning(
                        $"Passive preset '{presetName}' conflicts with already-attached '{conflictWith}'. Skipping. " +
                        $"Add \"allowConflict\": true to this passive's JSON to override.");
                    ability.Cleanup();
                    continue;
                }

                accepted.Add(new KeyValuePair<string, PassiveAbility>(presetName, ability));
                result.Add(ability);
            }

            return result;
        }

        private static PassiveAbility CreatePassiveInternal(JObject config, string presetName, BroBase owner)
        {
            if (string.IsNullOrEmpty(presetName))
            {
                BMLogger.Warning("Passive ability config entry is missing a 'preset' field, skipped.");
                return null;
            }

            Type type = PresetManager.GetPassivePreset(presetName);
            if (type == null)
            {
                BMLogger.Warning($"Passive preset '{presetName}' not found. Valid presets: {string.Join(", ", new List<string>(PresetManager.GetAllPassivePresets().Keys).ToArray())}");
                return null;
            }

            try
            {
                var ability = (PassiveAbility)Activator.CreateInstance(type);
                ApplyJsonOverrides(ability, config, owner);
                ability.Initialize(owner);
                if (ability.IsRedundant)
                {
                    BMLogger.Warning($"Passive preset '{presetName}' is redundant. This behavior is already built into the base bro. Skipping.");
                    ability.Cleanup();
                    return null;
                }
                return ability;
            }
            catch (Exception ex)
            {
                BMLogger.ExceptionLog($"Failed to create passive preset '{presetName}'", ex);
                return null;
            }
        }

        private static bool HasConflict(
            PassiveAbility candidate, string candidatePreset,
            List<KeyValuePair<string, PassiveAbility>> accepted,
            out string conflictWith)
        {
            var candidateConflicts = new HashSet<string>(GetConflictingPresets(candidate.GetType()));
            foreach (var pair in accepted)
            {
                if (candidateConflicts.Contains(pair.Key))
                {
                    conflictWith = pair.Key;
                    return true;
                }
                foreach (var name in GetConflictingPresets(pair.Value.GetType()))
                {
                    if (name == candidatePreset)
                    {
                        conflictWith = pair.Key;
                        return true;
                    }
                }
            }
            conflictWith = null;
            return false;
        }

        private static IEnumerable<string> GetConflictingPresets(Type type)
        {
            foreach (var attr in type.GetCustomAttributes(typeof(ConflictsWithPresetAttribute), inherit: true))
            {
                yield return ((ConflictsWithPresetAttribute)attr).PresetName;
            }
            foreach (var attr in type.GetCustomAttributes(typeof(ConflictsWithPresetsAttribute), inherit: true))
            {
                foreach (var name in ((ConflictsWithPresetsAttribute)attr).PresetNames)
                {
                    yield return name;
                }
            }
        }

        private static void ApplyJsonOverrides(object ability, JObject config, BroBase owner)
        {
            var dict = config.ToObject<Dictionary<string, object>>();
            dict.Remove("preset");
            dict.Remove("allowConflict");  // factory-consumed, not an ability field
            if (dict.Count == 0) return;

            string path = (owner as ICustomHero)?.Info?.path ?? "";
            string presetName = config.Value<string>("preset");
            ability.DynamicFieldsValueSetter(dict, null,
                (field, key, value) => CustomBroforceObjectInfo.SetFieldDataStatic(field, key, value, path),
                context: $"{presetName} ability override");
        }
    }
}
