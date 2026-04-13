using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using BroMakerLib.Extensions;
using Newtonsoft.Json;
using UnityEngine;

namespace BroMakerLib.Vanilla.Melees
{
    [MeleePreset("TheBrolander")]
    public class TheBrolanderMelee : MeleeAbility
    {
        protected override HeroType SourceBroType => HeroType.TheBrolander;

        [JsonIgnore]
        private ElectricZap zapper;

        public TheBrolanderMelee()
        {
            meleeType = BroBase.MeleeType.Punch;
            startType = MeleeStartType.Custom;
            moveType = MeleeMoveType.Punch;
            restartFrame = 0;
        }

        public override void Initialize(TestVanDammeAnim owner)
        {
            base.Initialize(owner);

            var sourceBro = HeroController.GetHeroPrefab(HeroType.TheBrolander) as TheBrolander;
            if (sourceBro != null)
            {
                zapper = sourceBro.zapper;
            }
        }

        public override void AnimateMelee()
        {
            hero.AnimateMeleeCommon();
            int num = 25 + Mathf.Clamp(owner.frame, 0, 8);
            int num2 = 9;
            if (owner.frame == 5)
            {
                owner.counter -= 0.0334f;
                owner.counter -= 0.0334f;
                owner.counter -= 0.0334f;
            }
            if (owner.frame == 3)
            {
                owner.counter -= 0.0334f;
            }
            hero.Sprite.SetLowerLeftPixel((float)(num * hero.SpritePixelWidth), (float)(num2 * hero.SpritePixelHeight));
            if (owner.frame == 3 && !hero.MeleeHasHit)
            {
                PerformPunchAttack(true, true);
            }
            if (owner.GetFieldValue<BroBase.MeleeType>("currentMeleeType") == BroBase.MeleeType.JetpackPunch && owner.frame >= 4 && owner.frame <= 5 && !hero.MeleeHasHit)
            {
                PerformPunchAttack(true, true);
            }
            if (owner.frame >= 7)
            {
                owner.frame = 0;
                hero.CancelMelee();
            }
        }

        private void PerformPunchAttack(bool shouldTryHitTerrain, bool playMissSound)
        {
            float num = 8f;
            Vector3 vector = new Vector3(owner.X + (float)owner.Direction * num, owner.Y, 0f);
            bool flag;
            Map.DamageDoodads(3, DamageType.Melee, vector.x, vector.y, 0f, 0f, 6f, owner.playerNum, out flag, null);
            hero.KickDoors(25f);
            int specialAmmo = owner.GetFieldValue<int>("_specialAmmo");
            int num2 = ((specialAmmo < 2) ? 5 : 15);
            DamageType damageType = ((specialAmmo < 2) ? DamageType.Melee : DamageType.Plasma);
            float num3 = (float)((specialAmmo < 2) ? 120 : 250);
            if (Map.HitClosestUnit(owner, owner.playerNum, num2, damageType, num + 6f, num * 2f, vector.x + (float)(owner.Direction * 5), vector.y, owner.transform.localScale.x * num3, (float)((specialAmmo < 2) ? 100 : 250), true, false, owner.IsMine, false, true))
            {
                sound.PlaySoundEffectAt(alternateMeleeHitSounds, 0.5f, owner.transform.position, 1f, true, false, false, 0f);
                hero.MeleeHasHit = true;
                EffectsController.CreateProjectilePopWhiteEffect(owner.X + (owner.width + 4f) * owner.transform.localScale.x, owner.Y + owner.height + 4f);
            }
            else
            {
                if (playMissSound && !hero.HasPlayedMissSound)
                {
                    sound.PlaySoundEffectAt(missSounds, 0.15f, owner.transform.position, 1f, true, false, false, 0f);
                }
                hero.HasPlayedMissSound = true;
            }
            float electricPunchCharge = owner.GetFieldValue<float>("electricPunchCharge");
            if (electricPunchCharge > 1.5f)
            {
                owner.SetFieldValue("electricPunchCharge", 0f);
                FullScreenFlashEffect.FlashLightning(0.3f);
                if (!Map.HitAllLivingUnits(owner, owner.playerNum, 1, DamageType.Shock, 22f, 14f, vector.x, vector.y, (float)(owner.Direction * 100), 50f, true, false))
                {
                    if (playMissSound && !hero.HasPlayedMissSound)
                    {
                        if (playMissSound && !hero.HasPlayedMissSound)
                        {
                            sound.PlaySoundEffectAt(missSounds, 0.15f, owner.transform.position, 1f, true, false, false, 0f);
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
            if (!hero.MeleeHasHit && shouldTryHitTerrain && HandleTryMeleeTerrain(0, terrainDamage))
            {
                hero.MeleeHasHit = true;
            }
        }
    }
}
