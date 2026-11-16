using System;
using System.Collections.Generic;
using System.Reflection;
using BroMakerLib.Attributes;
using BroMakerLib.Cutscenes;
using BroMakerLib.Loggers;
using BroMakerLib.Unlocks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace BroMakerLib.Infos
{
    public class CustomBroInfo : CustomBroforceObjectInfo
    {
        protected new string _defaultName = "BRO";

        public CustomBroInfo() : base()
        {
        }

        public CustomBroInfo(string name) : base(name)
        {
        }

        public string CharacterPreset = "CustomHero";

        [JsonConverter(typeof(CutsceneConverter))]
        public List<CustomIntroCutscene> Cutscene = new List<CustomIntroCutscene> { new CustomIntroCutscene() };

        public BroUnlockConfig UnlockConfig { get; set; } = new BroUnlockConfig();

        [JsonIgnore] public List<string> SpritePath = new List<string>();
        [JsonIgnore] public List<string> GunSpritePath = new List<string>();
        [JsonIgnore] public List<Vector2> GunSpriteOffset = new List<Vector2> { Vector2.zero };
        [JsonIgnore] public List<List<Material>> SpecialMaterials = new List<List<Material>> { new List<Material>() };
        [JsonIgnore] public List<Vector2> SpecialMaterialOffset = new List<Vector2> { Vector2.zero };
        [JsonIgnore] public List<float> SpecialMaterialSpacing = new List<float> { 0f };
        [JsonIgnore] public List<Material> FirstAvatar = new List<Material> { null };
        [JsonIgnore] public int VariantCount = 1;

        [JsonExtensionData(WriteData = false)] private Dictionary<string, JToken> _unknownKeys = new Dictionary<string, JToken>();

        public override string SerializeJSON()
        {
            return SerializeJSON(DirectoriesManager.BrosDirectory);
        }

        internal void WarnUnknownKeys(string fileName)
        {
            if (_unknownKeys.Count == 0)
            {
                return;
            }

            List<string> knownKeys = new List<string>();
            JsonObjectContract contract = JsonSerializer.CreateDefault().ContractResolver.ResolveContract(GetType()) as JsonObjectContract;
            if (contract != null)
            {
                foreach (JsonProperty property in contract.Properties)
                {
                    if (!property.Ignored)
                    {
                        knownKeys.Add(property.PropertyName);
                    }
                }
            }

            List<string> parameterNames = new List<string>();
            foreach (MethodInfo method in typeof(Parameters).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
            {
                if (method.GetCustomAttributes(typeof(ParameterAttribute), true).Length > 0)
                {
                    parameterNames.Add(method.Name);
                }
            }

            foreach (string key in _unknownKeys.Keys)
            {
                string message = $"Key '{key}' in {fileName} is not a bro setting and was ignored";
                string parameterName = parameterNames.Find(name => string.Equals(name, key, StringComparison.OrdinalIgnoreCase));
                if (parameterName != null)
                {
                    BMLogger.Warning($"{message}. '{parameterName}' is a parameter, so it belongs inside 'parameters'");
                    continue;
                }

                string suggestion = FindSimilarKey(key, knownKeys);
                BMLogger.Warning(suggestion != null ? $"{message}. Did you mean '{ToJsonKey(suggestion)}'?" : message);
            }
        }

        private static string ToJsonKey(string memberName)
        {
            return char.ToLowerInvariant(memberName[0]) + memberName.Substring(1);
        }

        private static string FindSimilarKey(string key, List<string> knownKeys)
        {
            string keyLower = key.ToLower();
            int maxDistance = key.Length <= 4 ? 1 : key.Length <= 8 ? 2 : 3;
            string bestMatch = null;
            int bestDistance = int.MaxValue;
            foreach (string knownKey in knownKeys)
            {
                int distance = EditDistance(keyLower, knownKey.ToLower());
                if (distance < bestDistance && distance <= maxDistance)
                {
                    bestDistance = distance;
                    bestMatch = knownKey;
                }
            }
            return bestMatch;
        }

        // Levenshtein distance that counts swapping two neighbouring letters as one edit.
        private static int EditDistance(string a, string b)
        {
            int[,] d = new int[a.Length + 1, b.Length + 1];
            for (int i = 0; i <= a.Length; i++)
            {
                d[i, 0] = i;
            }
            for (int j = 0; j <= b.Length; j++)
            {
                d[0, j] = j;
            }
            for (int i = 1; i <= a.Length; i++)
            {
                for (int j = 1; j <= b.Length; j++)
                {
                    int cost = a[i - 1] == b[j - 1] ? 0 : 1;
                    d[i, j] = Mathf.Min(Mathf.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
                    if (i > 1 && j > 1 && a[i - 1] == b[j - 2] && a[i - 2] == b[j - 1])
                    {
                        d[i, j] = Mathf.Min(d[i, j], d[i - 2, j - 2] + 1);
                    }
                }
            }
            return d[a.Length, b.Length];
        }
    }
}
