using System;
using System.Collections.Generic;
using BroMakerLib.CustomObjects;
using BroMakerLib.Infos;
using BroMakerLib.Loggers;
using HarmonyLib;
using Newtonsoft.Json.Linq;

namespace BroMakerLib.Abilities
{
    /// <summary>
    /// Factory for instantiating ability instances from JSON configuration.
    /// Reads the "preset" field to determine the concrete type, creates the instance via
    /// <see cref="Activator.CreateInstance"/> (which runs the constructor for bro-specific defaults),
    /// then applies JSON overrides using <see cref="CustomBroforceObjectInfo.SetFieldDataStatic"/>
    /// for full Unity type support (AudioClip, Texture, Material, Projectile, Grenade, etc.).
    /// </summary>
    public static class AbilityFactory
    {
        /// <summary>
        /// Creates a <see cref="SpecialAbility"/> from JSON config.
        /// Returns null if config is null, preset is missing, preset type is not registered, or instantiation fails.
        /// </summary>
        /// <param name="config">JSON object containing a "preset" key and optional parameter overrides.</param>
        /// <param name="owner">The bro instance that will own this ability.</param>
        /// <returns>The instantiated and initialized ability, or null on failure.</returns>
        public static SpecialAbility CreateSpecial(JObject config, TestVanDammeAnim owner)
        {
            if (config == null) return null;

            string presetName = config.Value<string>("preset");
            if (string.IsNullOrEmpty(presetName)) return null;

            Type type = PresetManager.GetSpecialPreset(presetName);
            if (type == null)
            {
                BMLogger.Warning($"Special preset '{presetName}' not found");
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

        /// <summary>
        /// Creates a <see cref="MeleeAbility"/> from JSON config.
        /// Returns null if config is null, preset is missing, preset type is not registered, or instantiation fails.
        /// </summary>
        /// <param name="config">JSON object containing a "preset" key and optional parameter overrides.</param>
        /// <param name="owner">The bro instance that will own this ability.</param>
        /// <returns>The instantiated and initialized ability, or null on failure.</returns>
        public static MeleeAbility CreateMelee(JObject config, TestVanDammeAnim owner)
        {
            if (config == null) return null;

            string presetName = config.Value<string>("preset");
            if (string.IsNullOrEmpty(presetName)) return null;

            Type type = PresetManager.GetMeleePreset(presetName);
            if (type == null)
            {
                BMLogger.Warning($"Melee preset '{presetName}' not found");
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

        /// <summary>
        /// Applies JSON overrides to an ability instance using the type-aware field setter.
        /// Skips the "preset" key. Resolves file paths relative to the bro's mod directory.
        /// </summary>
        /// <param name="ability">The ability instance to apply overrides to.</param>
        /// <param name="config">JSON config object.</param>
        /// <param name="owner">The bro owner, used to resolve the mod path for file references.</param>
        private static void ApplyJsonOverrides(object ability, JObject config, TestVanDammeAnim owner)
        {
            var dict = config.ToObject<Dictionary<string, object>>();
            dict.Remove("preset");
            if (dict.Count == 0) return;

            string path = (owner as ICustomHero)?.Info?.path ?? "";
            ability.DynamicFieldsValueSetter(dict, null,
                (field, key, value) => CustomBroforceObjectInfo.SetFieldDataStatic(field, key, value, path));
        }
    }
}
