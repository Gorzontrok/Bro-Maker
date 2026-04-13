using System;
using System.Collections.Generic;
using BroMakerLib.CustomObjects;
using BroMakerLib.Infos;
using BroMakerLib.Loggers;
using HarmonyLib;
using Newtonsoft.Json.Linq;

namespace BroMakerLib.Abilities
{
    /// <summary>Creates ability instances from JSON configuration.</summary>
    public static class AbilityFactory
    {
        /// <returns>The instantiated ability, or null on failure.</returns>
        public static SpecialAbility CreateSpecial(JObject config, TestVanDammeAnim owner)
        {
            if (config == null) return null;

            string presetName = config.Value<string>("preset");
            if (string.IsNullOrEmpty(presetName)) return null;

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

        /// <returns>The instantiated ability, or null on failure.</returns>
        public static MeleeAbility CreateMelee(JObject config, TestVanDammeAnim owner)
        {
            if (config == null) return null;

            string presetName = config.Value<string>("preset");
            if (string.IsNullOrEmpty(presetName)) return null;

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

        private static void ApplyJsonOverrides(object ability, JObject config, TestVanDammeAnim owner)
        {
            var dict = config.ToObject<Dictionary<string, object>>();
            dict.Remove("preset");
            if (dict.Count == 0) return;

            string path = (owner as ICustomHero)?.Info?.path ?? "";
            string presetName = config.Value<string>("preset");
            ability.DynamicFieldsValueSetter(dict, null,
                (field, key, value) => CustomBroforceObjectInfo.SetFieldDataStatic(field, key, value, path),
                context: $"{presetName} special/melee override");
        }
    }
}
