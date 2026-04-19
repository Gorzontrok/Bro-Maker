using BroMakerLib.Attributes;
using BroMakerLib.Extensions;
using Newtonsoft.Json;
using UnityEngine;

namespace BroMakerLib.Vanilla.Melees
{
    /// <summary>TheBrolander's electric punch melee.</summary>
    [MeleePreset("TheBrolander")]
    public class TheBrolanderMelee : PunchMelee
    {
        protected override HeroType SourceBroType => HeroType.TheBrolander;

        /// <summary>Charge accumulated per second toward the electric punch threshold.</summary>
        public float electricPunchChargeRate = 1f;
        /// <summary>Charge level required to trigger the electric burst on punch.</summary>
        public float electricPunchChargeThreshold = 1.5f;
        /// <summary>Damage dealt by the electric burst to each unit hit.</summary>
        public int electricPunchDamage = 1;
        /// <summary>Horizontal range of the electric burst hit detection.</summary>
        public float electricPunchRangeX = 22f;
        /// <summary>Vertical range of the electric burst hit detection.</summary>
        public float electricPunchRangeY = 14f;
        /// <summary>Damage dealt by the punch when special ammo is 2 or more (charged state).</summary>
        public int chargedDamage = 15;
        /// <summary>Horizontal knockback applied to the target when punching in charged state.</summary>
        public float chargedKnockbackX = 250f;
        /// <summary>Vertical knockback applied to the target when punching in charged state.</summary>
        public float chargedKnockbackY = 250f;
        /// <summary>Vertical knockback applied to the target on a normal (uncharged) punch.</summary>
        public float normalKnockbackY = 100f;

        public TheBrolanderMelee()
        {
            normalDamage = 5;
            normalKnockbackX = 120f;
        }

        [JsonIgnore]
        private ElectricZap zapper;
        [JsonIgnore]
        private float electricPunchCharge = 1f;

        protected override void CacheSoundsFromPrefab()
        {
            base.CacheSoundsFromPrefab();

            var sourceBro = HeroController.GetHeroPrefab(HeroType.TheBrolander) as TheBrolander;
            if (sourceBro != null)
            {
                zapper = sourceBro.zapper;
            }
        }

        public override void Update()
        {
            electricPunchCharge += hero.DeltaTime * electricPunchChargeRate;
        }

        protected override void PerformPunchAttack(bool shouldTryHitTerrain, bool playMissSound)
        {
            float num = 8f;
            Vector3 vector = new Vector3(owner.X + (float)owner.Direction * num, owner.Y, 0f);
            bool flag;
            Map.DamageDoodads(3, DamageType.Melee, vector.x, vector.y, 0f, 0f, 6f, owner.playerNum, out flag, null);
            hero.KickDoors(normalKickRange);
            int specialAmmo = hero.SpecialAmmoField;
            int num2 = ((specialAmmo < 2) ? normalDamage : chargedDamage);
            DamageType damageType = ((specialAmmo < 2) ? parsedDamageType : DamageType.Plasma);
            float num3 = (float)((specialAmmo < 2) ? normalKnockbackX : chargedKnockbackX);
            float kbY = (specialAmmo < 2) ? normalKnockbackY : chargedKnockbackY;
            if (Map.HitClosestUnit(owner, owner.playerNum, num2, damageType, num + 6f, num * 2f, vector.x + (float)(owner.Direction * 5), vector.y, owner.transform.localScale.x * num3, kbY, true, false, owner.IsMine, false, true))
            {
                sound.PlaySoundEffectAt(alternateMeleeHitSounds, hitSoundVolume, owner.transform.position, 1f, true, false, false, 0f);
                hero.MeleeHasHit = true;
                EffectsController.CreateProjectilePopWhiteEffect(owner.X + (owner.width + 4f) * owner.transform.localScale.x, owner.Y + owner.height + 4f);
            }
            else
            {
                if (playMissSound && !hero.HasPlayedMissSound)
                {
                    sound.PlaySoundEffectAt(missSounds, missSoundVolume, owner.transform.position, 1f, true, false, false, 0f);
                }
                hero.HasPlayedMissSound = true;
            }
            if (electricPunchCharge > electricPunchChargeThreshold)
            {
                electricPunchCharge = 0f;
                FullScreenFlashEffect.FlashLightning(0.3f);
                if (!Map.HitAllLivingUnits(owner, owner.playerNum, electricPunchDamage, DamageType.Shock, electricPunchRangeX, electricPunchRangeY, vector.x, vector.y, (float)(owner.Direction * 100), 50f, true, false))
                {
                    if (playMissSound && !hero.HasPlayedMissSound)
                    {
                        if (playMissSound && !hero.HasPlayedMissSound)
                        {
                            sound.PlaySoundEffectAt(missSounds, missSoundVolume, owner.transform.position, 1f, true, false, false, 0f);
                        }
                        hero.HasPlayedMissSound = true;
                    }
                }
                if (zapper != null)
                {
                    zapper.Create(vector + Vector3.up * 10f, vector + new Vector3((float)(owner.Direction * 17), 4f, 0f), new DamageObject(1, DamageType.Shock, 0f, 0f, owner.X, owner.Y, owner), null, null, -1, -1, 0);
                }
            }
            hero.MeleeChosenUnit = null;
            if (!hero.MeleeHasHit && shouldTryHitTerrain && TryMeleeTerrain(0, normalTerrainDamage))
            {
                hero.MeleeHasHit = true;
            }
        }
    }
}
