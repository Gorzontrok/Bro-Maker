using System;
using System.Collections.Generic;
using BroMakerLib.CustomObjects;
using BroMakerLib.Infos;
using BroMakerLib.Loaders;
using BroMakerLib.Loggers;
using Newtonsoft.Json;
using UnityEngine;

namespace BroMakerLib.Abilities
{
    /// <summary>
    /// Grenade throw settings for an ability, whose prefab clones made for a `damage` or `sprite` override are shared by every bro using the same settings for the rest of the
    /// session.
    /// </summary>
    public class GrenadeConfig
    {
        /// <summary>Name of the vanilla grenade prefab to throw.</summary>
        public string name;

        /// <summary>Damage each grenade deals. When unset, the prefab's own damage is kept.</summary>
        public int? damage;

        /// <summary>Image file to swap onto the grenade, relative to the bro's folder. When unset, the prefab's own sprite is kept.</summary>
        public string sprite;

        /// <summary>How many grenades are thrown per use.</summary>
        public int count = 1;

        /// <summary>Total angle in degrees that the fan of grenades covers when more than one is thrown.</summary>
        public float spread;

        /// <summary>Horizontal spawn offset from the bro, in its facing direction.</summary>
        public float offsetX = 8f;

        /// <summary>Vertical spawn offset from the bro.</summary>
        public float offsetY = 8f;

        /// <summary>Horizontal throw speed.</summary>
        public float throwForceX = 200f;

        /// <summary>Vertical throw speed.</summary>
        public float throwForceY = 150f;

        /// <summary>Horizontal spawn offset when throwing from a ducked position.</summary>
        public float duckOffsetX = 6f;

        /// <summary>Vertical spawn offset when throwing from a ducked position.</summary>
        public float duckOffsetY = 3f;

        /// <summary>Horizontal throw speed when throwing from a ducked position.</summary>
        public float duckThrowForceX = 30f;

        /// <summary>Vertical throw speed when throwing from a ducked position.</summary>
        public float duckThrowForceY = 70f;

        [JsonIgnore] internal Grenade basePrefab;

        /// <summary>Prefab to throw once `Initialize` has run.</summary>
        [JsonIgnore]
        public Grenade Prefab { get { return basePrefab; } }

        /// <summary>
        /// Returns the throw vector for one grenade of a multi-grenade throw, fanned so `count` grenades cover `spread` degrees centered on the base vector.
        /// </summary>
        /// <param name="forceX">Base horizontal throw speed, already signed by the thrower's facing direction.</param>
        /// <param name="forceY">Base vertical throw speed.</param>
        /// <param name="index">Zero-based index of this grenade within the throw.</param>
        public Vector2 GetThrowVector(float forceX, float forceY, int index)
        {
            if (count <= 1 || spread == 0f)
            {
                return new Vector2(forceX, forceY);
            }

            float angle = spread * ((index / (float)(count - 1)) - 0.5f) * Mathf.Deg2Rad;
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);
            return new Vector2((forceX * cos) - (forceY * sin), (forceX * sin) + (forceY * cos));
        }

        private const int MaxCount = 100;

        private static readonly Dictionary<string, Grenade> clonedPrefabs = new Dictionary<string, Grenade>();

        /// <summary>Resolves the prefab to throw, baking any `damage` or `sprite` override into a shared clone so the vanilla prefab is never mutated.</summary>
        /// <param name="owner">Bro the ability is attached to, whose folder `sprite` is resolved against.</param>
        /// <param name="fallbackPrefab">Prefab to use when `name` is unset, for abilities whose grenade isn't in the vanilla name registry.</param>
        public void Initialize(BroBase owner, Grenade fallbackPrefab = null)
        {
            CustomBroInfo info = (owner as ICustomHero)?.Info;
            string modPath = info?.path ?? string.Empty;
            string broName = info?.name;
            string mod = info?.Mod?.Name;

            if (count < 1)
            {
                BMLogger.Warning($"Grenade 'count' {count} is below 1 and would throw nothing while still using ammo, so it was raised to 1", mod: mod, bro: broName);
                count = 1;
            }
            else if (count > MaxCount)
            {
                BMLogger.Warning($"Grenade 'count' {count} is above the maximum of {MaxCount}, so it was lowered to {MaxCount}", mod: mod, bro: broName);
                count = MaxCount;
            }

            Grenade resolvedPrefab;
            if (!string.IsNullOrEmpty(name) && !name.Contains(":") && !LoadBroforceObjects.GrenadesVanilla.ContainsKey(name))
            {
                BMLogger.Warning($"Unknown grenade '{name}', using the default grenade", mod: mod, bro: broName);
                resolvedPrefab = LoadBroforceObjects.GetGrenadeFromName("Default");
            }
            else
            {
                using (BMLogger.Scope(mod, broName))
                {
                    resolvedPrefab = !string.IsNullOrEmpty(name) ? LoadBroforceObjects.GetGrenadeFromName(name) : fallbackPrefab;
                }
            }
            if (resolvedPrefab == null)
            {
                if (string.IsNullOrEmpty(name))
                {
                    BMLogger.Warning("No grenade 'name' is set and the ability has no grenade of its own, using the default grenade", mod: mod, bro: broName);
                }
                else
                {
                    BMLogger.Warning($"Grenade '{name}' couldn't be loaded as a usable grenade, using the default grenade", mod: mod, bro: broName);
                }
                resolvedPrefab = LoadBroforceObjects.GetGrenadeFromName("Default");
            }

            if (!damage.HasValue && string.IsNullOrEmpty(sprite))
            {
                basePrefab = resolvedPrefab;
                return;
            }

            string cacheKey = $"{resolvedPrefab.GetInstanceID()}|{damage}|{sprite ?? string.Empty}|{modPath}";
            Grenade cachedClone;
            if (clonedPrefabs.TryGetValue(cacheKey, out cachedClone) && cachedClone != null)
            {
                basePrefab = cachedClone;
                return;
            }

            Texture2D texture = null;
            if (!string.IsNullOrEmpty(sprite))
            {
                try
                {
                    texture = ResourcesController.LoadTexture(modPath, sprite);
                }
                catch (Exception ex)
                {
                    BMLogger.Warning($"Couldn't load grenade sprite '{sprite}', so the grenade keeps its own sprite", ex, mod, broName);
                }
            }

            Grenade clone = BroMakerUtilities.ClonePrefab(resolvedPrefab);

            if (damage.HasValue)
            {
                clone.damage = damage.Value;
            }
            if (texture != null)
            {
                BroMakerUtilities.SetCloneTexture(clone, texture);

                // Swapped in at runtime while the grenade blinks, which would otherwise drop the custom texture.
                if (clone.otherMaterial != null)
                {
                    clone.otherMaterial = new Material(clone.otherMaterial);
                    clone.otherMaterial.mainTexture = texture;
                }
            }

            clonedPrefabs[cacheKey] = clone;
            basePrefab = clone;
        }
    }
}
