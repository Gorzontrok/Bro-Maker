using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using Newtonsoft.Json;
using RocketLib.Extensions;
using UnityEngine;

namespace BroMakerLib.Vanilla.Melees
{
    [MeleePreset("TheBrolander")]
    public class TheBrolanderMelee : MeleeAbility
    {
        [JsonIgnore]
        private ElectricZap zapper;

        public override void Initialize(TestVanDammeAnim owner)
        {
            base.Initialize(owner);
            meleeType = BroBase.MeleeType.Punch;

            TheBrolander theBrolander = owner as TheBrolander;
            if (theBrolander == null)
            {
                var prefab = HeroController.GetHeroPrefab(HeroType.TheBrolander);
                theBrolander = prefab as TheBrolander;
            }
            if (theBrolander != null)
            {
                zapper = theBrolander.zapper;
                meleeHitSounds = theBrolander.soundHolder.meleeHitSound;
                missSounds = theBrolander.soundHolder.missSounds;
                alternateMeleeHitSounds = theBrolander.soundHolder.alternateMeleeHitSound;
                meleeHitTerrainSounds = theBrolander.soundHolder.meleeHitTerrainSound;
            }
        }

        public override void StartMelee()
        {
            if (!hero.DoingMelee || owner.frame > 4)
            {
                owner.frame = 0;
                owner.counter = -0.05f;
                AnimateMelee();
            }
            else if (hero.DoingMelee)
            {
                hero.MeleeFollowUp = true;
            }
            hero.StartMeleeCommon();
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

        public override void RunMeleeMovement()
        {
            owner.CallMethod("ApplyFallingGravity");
            if (hero.JumpingMelee)
            {
                if (owner.yI < owner.maxFallSpeed)
                {
                    owner.yI = owner.maxFallSpeed;
                }
            }
            else if (hero.DashingMelee)
            {
                if (owner.frame < 2)
                {
                    owner.xI = 0f;
                    owner.yI = 0f;
                }
                else if (owner.frame <= 4)
                {
                    if (hero.MeleeChosenUnit != null)
                    {
                        float num = 8f;
                        if (owner.GetFieldValue<BroBase.MeleeType>("currentMeleeType") == BroBase.MeleeType.Disembowel)
                        {
                            num = 14f;
                        }
                        float num2 = hero.MeleeChosenUnit.X - (float)owner.Direction * num - owner.X;
                        if (!owner.GetFieldValue<bool>("isInQuicksand"))
                        {
                            owner.xI = num2 / 0.1f;
                        }
                        if (!owner.GetFieldValue<bool>("isInQuicksand"))
                        {
                            owner.xI = Mathf.Clamp(owner.xI, -owner.speed * 1.7f, owner.speed * 1.7f);
                        }
                    }
                    else
                    {
                        if (!owner.GetFieldValue<bool>("isInQuicksand"))
                        {
                            owner.xI = owner.speed * Direction;
                        }
                        owner.yI = 0f;
                    }
                }
                else if (owner.frame <= 7)
                {
                    owner.xI = 0f;
                }
            }
            else if (owner.Y > owner.groundHeight + 1f)
            {
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
                if (playMissSound && !owner.GetFieldValue<bool>("hasPlayedMissSound"))
                {
                    sound.PlaySoundEffectAt(missSounds, 0.15f, owner.transform.position, 1f, true, false, false, 0f);
                }
                owner.SetFieldValue("hasPlayedMissSound", true);
            }
            float electricPunchCharge = owner.GetFieldValue<float>("electricPunchCharge");
            if (electricPunchCharge > 1.5f)
            {
                owner.SetFieldValue("electricPunchCharge", 0f);
                FullScreenFlashEffect.FlashLightning(0.3f);
                if (!Map.HitAllLivingUnits(owner, owner.playerNum, 1, DamageType.Shock, 22f, 14f, vector.x, vector.y, (float)(owner.Direction * 100), 50f, true, false))
                {
                    if (playMissSound && !owner.GetFieldValue<bool>("hasPlayedMissSound"))
                    {
                        if (playMissSound && !owner.GetFieldValue<bool>("hasPlayedMissSound"))
                        {
                            sound.PlaySoundEffectAt(missSounds, 0.15f, owner.transform.position, 1f, true, false, false, 0f);
                        }
                        owner.SetFieldValue("hasPlayedMissSound", true);
                    }
                }
                if (zapper != null)
                {
                    zapper.Create(vector + Vector3.up * 10f, vector + new Vector3((float)(owner.Direction * 17), 4f, 0f), new DamageObject(1, DamageType.Shock, 0f, 0f, owner.X, owner.Y, owner), null, null, -1, -1, 0);
                }
            }
            hero.MeleeChosenUnit = null;
            if (!hero.MeleeHasHit && shouldTryHitTerrain && hero.TryMeleeTerrain(0, 2))
            {
                hero.MeleeHasHit = true;
            }
        }
    }
}
