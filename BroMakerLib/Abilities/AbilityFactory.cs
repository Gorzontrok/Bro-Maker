using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BroMakerLib.Attributes;
using BroMakerLib.CustomObjects;
using BroMakerLib.Infos;
using BroMakerLib.Loggers;
using HarmonyLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BroMakerLib.Abilities
{
    /// <summary>Creates ability instances from JSON configuration.</summary>
    public static class AbilityFactory
    {
        /// <summary>Instantiates a `SpecialAbility` from a JSON config containing a `"preset"` key.</summary>
        /// <returns>The instantiated ability, or null on failure.</returns>
        public static SpecialAbility CreateSpecial(JToken config, BroBase owner)
        {
            JObject configObject = GetConfigObject(config, "special");
            if (configObject == null)
            {
                return null;
            }

            string presetName;
            if (!TryGetPresetName(configObject, "The 'special' ability config", out presetName))
            {
                return null;
            }

            Type type = PresetManager.GetSpecialPreset(presetName);
            if (type == null)
            {
                BMLogger.Warning($"Special preset '{presetName}' doesn't exist, so it was skipped. If it comes from another mod, that mod isn't installed or wasn't loaded.");
                return null;
            }

            try
            {
                SpecialAbility ability = (SpecialAbility)Activator.CreateInstance(type);
                ApplyJsonOverrides(ability, configObject, owner);
                ability.Initialize(owner);
                return ability;
            }
            catch (Exception ex)
            {
                BMLogger.Error($"Failed to create special preset '{presetName}', so the bro keeps its own special", ex);
                return null;
            }
        }

        /// <summary>Instantiates a `MeleeAbility` from a JSON config containing a `"preset"` key.</summary>
        /// <returns>The instantiated ability, or null on failure.</returns>
        public static MeleeAbility CreateMelee(JToken config, BroBase owner)
        {
            JObject configObject = GetConfigObject(config, "melee");
            if (configObject == null)
            {
                return null;
            }

            string presetName;
            if (!TryGetPresetName(configObject, "The 'melee' ability config", out presetName))
            {
                return null;
            }

            Type type = PresetManager.GetMeleePreset(presetName);
            if (type == null)
            {
                BMLogger.Warning($"Melee preset '{presetName}' doesn't exist, so it was skipped. If it comes from another mod, that mod isn't installed or wasn't loaded.");
                return null;
            }

            try
            {
                MeleeAbility ability = (MeleeAbility)Activator.CreateInstance(type);
                ApplyJsonOverrides(ability, configObject, owner);
                ability.Initialize(owner);
                return ability;
            }
            catch (Exception ex)
            {
                BMLogger.Error($"Failed to create melee preset '{presetName}', so the bro keeps its own melee", ex);
                return null;
            }
        }

        /// <summary>
        /// Instantiates the passives in a bro's `"passives"` JSON list, dropping redundant and duplicate ones, and any that clash under a `ConflictsWithPreset` declaration unless
        /// their JSON sets `"allowConflict": true`.
        /// </summary>
        public static List<PassiveAbility> CreatePassives(JToken configs, BroBase owner)
        {
            List<PassiveAbility> result = new List<PassiveAbility>();
            if (!HasAbilityConfig(configs))
            {
                return result;
            }
            JArray entries = configs as JArray;
            if (entries == null)
            {
                BMLogger.Warning(
                    $"The 'passives' ability config is {Describe(configs)}, which should be a list of objects like [{{\"preset\": \"{GetExampleName(configs)}\"}}], so it was skipped");
                return result;
            }

            HashSet<Type> seenTypes = new HashSet<Type>();
            List<KeyValuePair<string, PassiveAbility>> accepted = new List<KeyValuePair<string, PassiveAbility>>();

            foreach (JToken entry in entries)
            {
                if (entry.Type == JTokenType.Null)
                {
                    continue;
                }
                JObject config = entry as JObject;
                if (config == null)
                {
                    BMLogger.Warning(
                        $"An entry in the 'passives' list is {Describe(entry)}, which should be an object like {{\"preset\": \"{GetExampleName(entry)}\"}}, so it was skipped");
                    continue;
                }
                string presetName;
                if (!TryGetPresetName(config, "An entry in the 'passives' list", out presetName))
                {
                    continue;
                }
                bool allowConflict = false;
                JToken allowConflictToken = config["allowConflict"];
                if (allowConflictToken != null && allowConflictToken.Type != JTokenType.Null)
                {
                    if (allowConflictToken.Type != JTokenType.Boolean)
                    {
                        BMLogger.Warning(
                            $"Passive preset '{presetName}' has 'allowConflict' set to {Describe(allowConflictToken)}, which should be true or false without quotes, so the passive was skipped");
                        continue;
                    }
                    allowConflict = (bool)allowConflictToken;
                }

                Type type = PresetManager.GetPassivePreset(presetName);
                if (type == null)
                {
                    BMLogger.Warning($"Passive preset '{presetName}' doesn't exist, so it was skipped. If it comes from another mod, that mod isn't installed or wasn't loaded.");
                    continue;
                }
                PassiveAbility ability;
                try
                {
                    ability = (PassiveAbility)Activator.CreateInstance(type);
                    ApplyJsonOverrides(ability, config, owner);
                    ability.Initialize(owner);
                    if (ability.IsRedundant)
                    {
                        BMLogger.Warning($"Passive preset '{presetName}' is already built into the bro's characterPreset, so it was skipped");
                        ability.Detach();
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    BMLogger.Error($"Failed to create passive preset '{presetName}', so it was left out", ex);
                    continue;
                }

                if (seenTypes.Contains(ability.GetType()))
                {
                    BMLogger.Warning($"Passive preset '{presetName}' is listed more than once in 'passives', so this entry was skipped");
                    ability.Detach();
                    continue;
                }

                if (!allowConflict)
                {
                    string nativeConflict = null;
                    HashSet<string> related = new HashSet<string>(GetConflictingPresets(ability.GetType(), ConflictScope.All, ConflictScope.NativeOnly));
                    foreach (KeyValuePair<string, Type> pair in PresetManager.GetAllPassivePresets())
                    {
                        if (GetConflictingPresets(pair.Value, ConflictScope.All).Contains(presetName))
                        {
                            related.Add(pair.Key);
                        }
                    }
                    foreach (string name in related)
                    {
                        Type relatedType = PresetManager.GetPassivePreset(name);
                        if (relatedType == null || relatedType == ability.GetType())
                        {
                            continue;
                        }
                        try
                        {
                            if (((PassiveAbility)Activator.CreateInstance(relatedType)).IsRedundantOn(owner))
                            {
                                nativeConflict = name;
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            BMLogger.Log($"Skipping the built-in check of passive preset '{name}' against '{presetName}': {ex.GetBaseException().Message}");
                        }
                    }
                    if (nativeConflict != null)
                    {
                        BMLogger.Warning($"Passive preset '{presetName}' conflicts with '{nativeConflict}', which is built into the bro's characterPreset, so it was skipped. " +
                                         $"To use it anyway, add \"allowConflict\": true to the '{presetName}' entry in 'passives'");
                        ability.Detach();
                        continue;
                    }

                    string listedConflict = null;
                    HashSet<string> candidateConflicts = new HashSet<string>(GetConflictingPresets(ability.GetType(), ConflictScope.All, ConflictScope.ListedOnly));
                    foreach (KeyValuePair<string, PassiveAbility> pair in accepted)
                    {
                        if (candidateConflicts.Contains(pair.Key) || GetConflictingPresets(pair.Value.GetType(), ConflictScope.All, ConflictScope.ListedOnly).Contains(presetName))
                        {
                            listedConflict = pair.Key;
                            break;
                        }
                    }
                    if (listedConflict != null)
                    {
                        BMLogger.Warning($"Passive preset '{presetName}' conflicts with '{listedConflict}' earlier in the list, so it was skipped. " +
                                         $"To use it anyway, add \"allowConflict\": true to the '{presetName}' entry in 'passives'");
                        ability.Detach();
                        continue;
                    }
                }

                seenTypes.Add(ability.GetType());
                accepted.Add(new KeyValuePair<string, PassiveAbility>(presetName, ability));
                result.Add(ability);
            }

            return result;
        }

        internal static bool HasAbilityConfig(JToken config)
        {
            if (config == null || config.Type == JTokenType.Null)
            {
                return false;
            }
            return (config.Type != JTokenType.Object && config.Type != JTokenType.Array) || config.HasValues;
        }

        private static JObject GetConfigObject(JToken config, string key)
        {
            if (!HasAbilityConfig(config))
            {
                return null;
            }
            JObject configObject = config as JObject;
            if (configObject == null)
            {
                BMLogger.Warning(
                    $"The '{key}' ability config is {Describe(config)}, which should be an object like {{\"preset\": \"{GetExampleName(config)}\"}}, so it was skipped");
            }
            return configObject;
        }

        private static bool TryGetPresetName(JObject config, string subject, out string presetName)
        {
            presetName = null;
            JToken token = config["preset"];
            if (token != null && token.Type != JTokenType.String)
            {
                BMLogger.Warning($"{subject} has 'preset' set to {Describe(token)}, which should be a preset name in quotes, so it was skipped");
                return false;
            }
            presetName = (string)token;
            if (string.IsNullOrEmpty(presetName))
            {
                BMLogger.Warning($"{subject} doesn't give a preset name in 'preset', so it was skipped");
                return false;
            }
            return true;
        }

        private static string Describe(JToken token)
        {
            return token.ToString(Formatting.None);
        }

        private static string GetExampleName(JToken token)
        {
            return token.Type == JTokenType.String ? (string)token : "PresetName";
        }

        private static IEnumerable<string> GetConflictingPresets(Type type, params ConflictScope[] scopes)
        {
            foreach (ConflictsWithPresetAttribute attr in type.GetCustomAttributes(typeof(ConflictsWithPresetAttribute), inherit: true))
            {
                if (Array.IndexOf(scopes, attr.Scope) < 0)
                {
                    continue;
                }
                foreach (string name in attr.PresetNames)
                {
                    yield return name;
                }
            }
        }

        private static void ApplyJsonOverrides(object ability, JObject config, BroBase owner)
        {
            Dictionary<string, object> dict = config.ToObject<Dictionary<string, object>>();
            dict.Remove("preset");
            dict.Remove("allowConflict");
            Type abilityType = ability.GetType();
            List<string> pending = dict.Keys.Where(key => dict[key] is JObject).ToList();
            while (pending.Count > 0)
            {
                List<string> next = new List<string>();
                foreach (string key in pending)
                {
                    JObject jObject = dict[key] as JObject;
                    if (jObject == null)
                    {
                        continue;
                    }
                    Type fieldType = abilityType;
                    foreach (string segment in key.Split('.'))
                    {
                        FieldInfo field = AccessTools.Field(fieldType, segment);
                        if (field == null)
                        {
                            fieldType = null;
                            break;
                        }
                        fieldType = field.FieldType;
                    }
                    bool isStruct = fieldType != null && fieldType.IsValueType && !fieldType.IsPrimitive && !fieldType.IsEnum;
                    if (isStruct)
                    {
                        continue;
                    }
                    dict.Remove(key);
                    foreach (JProperty property in jObject.Properties())
                    {
                        string newKey = $"{key}.{property.Name}";
                        JToken value = property.Value;
                        if (value is JObject nestedJObject)
                        {
                            dict[newKey] = nestedJObject;
                            next.Add(newKey);
                        }
                        else if (value is JArray jArray)
                        {
                            dict[newKey] = jArray;
                        }
                        else
                        {
                            dict[newKey] = ((JValue)value).Value;
                        }
                    }
                }
                pending = next;
            }
            if (dict.Count == 0)
            {
                return;
            }

            CustomBroInfo info = (owner as ICustomHero)?.Info;
            string presetName = config.Value<string>("preset");
            CustomBroInfo.ApplyFieldValues(ability, dict, info?.path ?? "", info?.name, $"the {presetName} ability override", $"{presetName} ability override",
                (key, message, problem) =>
                {
                    object value = dict[key];
                    if (problem == null)
                    {
                        BMLogger.Warning(message);
                    }
                    else
                    {
                        BMLogger.Error($"Failed to set field '{key}' (value: {value})", problem);
                    }
                });
        }
    }
}
