using RocketLib.JsonConverters;
using Newtonsoft.Json;
using RocketLib;
using Newtonsoft.Json.Converters;
using UnityEngine;
using System.IO;

namespace BroMakerLib.Stats
{
    public class WeaponStats
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public WeaponType type = WeaponType.Normal;
        public float rumbleAmountPerShot = 0.3f;
        public float fireRate = 0.0334f;
        public float fireDelay = 0f;
        [JsonConverter(typeof(Vector2Converter))]
        public Vector2 projectileRange = new Vector2(400f, 0f);
        public FloatRange projectileRandomRangeY = FloatRange.ZeroOne;
        [JsonConverter(typeof(Vector2Converter))]
        public Vector2 projectileSpawnPosition = new Vector2(10f, 8f);
        [JsonConverter(typeof(Vector2Converter))]
        public Vector2 pushBackForce = Vector2.zero;
        public bool useShrapnel = true;
        public string shrapnelTexturePath = string.Empty;
        [JsonIgnore]
        public Texture2D shrapnelTexture;

        public void Initialize()
        {
            if (!string.IsNullOrEmpty(shrapnelTexturePath) && File.Exists(Path.Combine(DirectoriesManager.WeaponsDirectory, shrapnelTexturePath)))
            {
                shrapnelTexture = ResourcesController.CreateTexture(File.ReadAllBytes(Path.Combine(DirectoriesManager.WeaponsDirectory, shrapnelTexturePath)));
            }
        }

    }
}
