using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

namespace BroMakerLib.Stats
{
    public class GrenadeStats
    {
        #region Broforce Variables  
        public int damage = 5;
        public float range = 48f;
        public float blastForce = 50f;
        [JsonConverter(typeof(StringEnumConverter))]
        public DamageType damageType = DamageType.Bullet;
        [JsonConverter(typeof(StringEnumConverter))]
        public TrailType trailType = TrailType.None;
        public bool useAngularFriction;
        public float angularFrictionM = 1f;
        public float minVelocityBounceSound = 33f;
        public float maxVelocityBounceVolume = 210f;
        public bool largeWarning = false;
        public float weight = 1f;
        public bool bounceOffEnemies = false;
        public bool bounceOffEnemiesMultiple = false;
        public float bounceYOffset = 2f;
        public bool shootable = false;
        public bool friendlyFire = true;
        public bool dontMakeEffects = false;
        public bool hugeExplosion = false;
        public float hugeExplosionLowPassM = 0.1f;
        #endregion

        [JsonIgnore]
        public NetworkObject spawnedObject;
    }
}
