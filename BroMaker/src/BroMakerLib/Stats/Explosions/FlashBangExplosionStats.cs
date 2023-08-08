using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BroMakerLib.Stats.Explosions
{
    public class FlashBangExplosionStats : ExplosionStats
    {
        public bool rotateExplosionSprite;
        public bool rotateExplosionSpriteToGround;
        public bool canMoveIntoAir = true;
        public int totalExplosions = 40;
        public float explosionRate = 0.06f;
        public bool blindUnits = true;
        public float blindTime = 9f;
        public bool tearGasUnits;
        public float tearGasTime = 4f;
        public bool damageUnits;
        public bool damageUnitsContinuously;
        public float maxTimeOverride;
        public bool alertUnits;
        public bool assasinateUnits;
        public int damageAmount = 3;
        public bool knockUnits = true;
        public bool burnGround;
        public bool damageGround;
        public int groundDamageAmount;
        public float groundDamageRange = 16f;
        public bool screenShake = true;
        public float forceDamageGroundChance = 0.1f;
        public int puffExplosionsCount = 1;
        public float maxPuffDelay;
        public bool extendedExplosion;
        public float yExplosionOffset = 4f;
        public bool constantAttackSound;
        public bool moveThroughDoors;
        public int seed = 1;
        public bool freezeUnits;
        public bool isScary;
        public bool randomExtraGrow = true;
        public bool bloodySurroundingBlocks;
        [JsonConverter(typeof(StringEnumConverter))]
        public BloodColor bloodColor;

    }
}
