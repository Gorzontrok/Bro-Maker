using System.Collections.Generic;
using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using BroMakerLib.Extensions;
using Newtonsoft.Json;
using Rogueforce;
using UnityEngine;

namespace BroMakerLib.Vanilla.Specials
{
    /// <summary>Broden's impaling dash with explosive finish.</summary>
    [SpecialPreset("Broden")]
    public class BrodenSpecial : SpecialAbility
    {
        protected override HeroType SourceBroType => HeroType.Broden;

        protected override void CacheSoundsFromPrefab()
        {
            base.CacheSoundsFromPrefab();
            var sourceBro = HeroController.GetHeroPrefab(SourceBroType);
            if (sourceBro == null) return;
            if (special2Sounds == null) special2Sounds = sourceBro.soundHolder.special2Sounds.CloneArray();
        }

        public override void Initialize(TestVanDammeAnim owner)
        {
            base.Initialize(owner);
            var prefab = HeroController.GetHeroPrefab(HeroType.Broden);
            var sourceBro = prefab.GetComponent<TestVanDammeAnim>();
            if (owner.faderSpritePrefab == null)
            {
                owner.faderSpritePrefab = sourceBro.faderSpritePrefab;
            }
        }

        /// <summary>Horizontal speed during the dash.</summary>
        public float dashSpeed = 240f;
        /// <summary>How long the dash lasts in seconds.</summary>
        public float dashDuration = 0.5f;
        /// <summary>Minimum time between consecutive dashes.</summary>
        public float dashCooldown = 0.55f;
        /// <summary>Volume of the dash initiation sound.</summary>
        public float specialSoundVolume = 0.7f;
        /// <summary>Radius within which incoming projectiles are deflected during the dash.</summary>
        public float deflectRange = 16f;
        /// <summary>Damage dealt to terrain per dash tick.</summary>
        public int terrainDamage = 3;
        /// <summary>Damage dealt to terrain by the explosion at the end of the dash.</summary>
        public int explosiveEndDamage = 15;
        /// <summary>Blast radius of the end-of-dash explosion.</summary>
        public float explosiveEndRange = 24f;
        /// <summary>Radius used when checking for impale targets laterally.</summary>
        public float impaleRange = 8f;
        /// <summary>How far ahead of the bro to probe for the next impale target.</summary>
        public float impaleSearchAhead = 12f;
        public float sliceVolume = 0.7f;
        public AudioClip[] special2Sounds;
        /// <summary>Horizontal blast impulse applied to the bro after the dash ends.</summary>
        public float postDashXIBlast = -170f;
        /// <summary>Duration of the upward spin-up phase after the dash ends.</summary>
        public float postDashSpinUpTime = 0.11f;
        /// <summary>Sprite sheet column for the dash spin animation.</summary>
        public int spinSpriteColumn = 11;
        /// <summary>Sprite sheet row for the grounded spin animation after the dash.</summary>
        public int spinSpriteRow = 9;
        /// <summary>Sprite sheet row for the airborne spin animation after the dash.</summary>
        public int spinJumpSpriteRow = 10;

        [JsonIgnore]
        private float dashTime;
        [JsonIgnore]
        private float dashStartTime;
        [JsonIgnore]
        private float dashHeight;
        [JsonIgnore]
        private float dashCounter;
        [JsonIgnore]
        private bool postDashSpinning;
        [JsonIgnore]
        private float postDashSpinningUpTime;
        [JsonIgnore]
        private List<Unit> impaledUnits = new List<Unit>();

        public override void PressSpecial()
        {
            if (owner.SpecialAmmo > 0)
            {
                UnimpaleAll();
                impaledUnits.Clear();
                UseSpecial();
                postDashSpinning = false;
            }
            else
            {
                HeroController.FlashSpecialAmmo(PlayerNum);
            }
        }

        public override void UseSpecial()
        {
            if (owner.hasBeenCoverInAcid || hero.UsingSpecial || owner.health <= 0 || hero.DoingMelee)
            {
                return;
            }
            if (owner.SpecialAmmo > 0 && Time.time - dashStartTime > dashCooldown)
            {
                owner.actionState = ActionState.Jumping;
                hero.StopAirDashing();
                if (owner.attachedToZipline != null)
                {
                    owner.attachedToZipline.DetachUnit(owner);
                }
                Sound.GetInstance().PlaySoundEffectAt(specialAttackSounds, specialSoundVolume, owner.transform.position);
                owner.SpecialAmmo--;
                HeroController.SetSpecialAmmo(PlayerNum, owner.SpecialAmmo);
                hero.UsingSpecial = false;
                dashTime = dashDuration;
                dashStartTime = Time.time;
                dashHeight = Y;
                hero.ChangeFrame();
            }
            else
            {
                HeroController.FlashSpecialAmmo(PlayerNum);
                owner.gunSprite.gameObject.SetActive(true);
                hero.UsingSpecial = false;
            }
        }

        public override bool HandleRunMovement()
        {
            if (owner.health <= 0)
            {
                owner.xIAttackExtra = 0f;
            }
            if (dashTime <= 0f)
            {
                return true;
            }

            dashTime -= hero.DeltaTime;
            if (owner.health > 0)
            {
                owner.invulnerable = true;
                hero.InvulnerableTime = 0.3f;
                owner.xI = dashSpeed * owner.transform.localScale.x;
                owner.yI = 0f;
                owner.Y = dashHeight;
                Map.DeflectProjectiles(owner, PlayerNum, deflectRange,
                    X + Mathf.Sign(owner.transform.localScale.x) * 1f, Y + 6f,
                    Mathf.Sign(owner.transform.localScale.x) * 200f, true);

                dashCounter += hero.DeltaTime;
                if (dashCounter > 0f)
                {
                    dashCounter -= 0.0333f;
                    bool flag;
                    Map.DamageDoodads(terrainDamage, DamageType.Knifed, X + owner.Direction * 4f, Y, 0f, 0f, 6f, PlayerNum, out flag, null);
                    if (owner.IsMine && MapController.DamageGround(owner, terrainDamage, DamageType.Melee, ValueOrchestrator.GetModifiedDamage(12, PlayerNum), X + owner.transform.localScale.x * 4f, Y + 7f, null, false) && Time.time - dashStartTime > 0.2f)
                    {
                        dashTime = 0f;
                    }
                    Unit firstUnit = Map.GetFirstUnit(owner, PlayerNum, impaleRange,
                        X + owner.transform.localScale.x * impaleSearchAhead, Y + 4f, true, true, impaledUnits);
                    if (firstUnit != null)
                    {
                        impaledUnits.Add(firstUnit);
                        firstUnit.Impale(owner.transform, Vector3.right * Mathf.Sign(owner.xI), 0, owner.xI, owner.yI,
                            owner.transform.localScale.x * 8f, 0f);
                    }
                    hero.CreateFaderTrailInstance();
                }

                if (dashTime <= 0f)
                {
                    EndDash();
                }

                Map.HurtWildLife(X + owner.transform.localScale.x * 13f, Y + 5f, 16f);
            }
            return true;
        }

        private void EndDash()
        {
            hero.SetGunSprite(0, 0);
            owner.actionState = ActionState.Jumping;
            if (MapController.DamageGround(owner, ValueOrchestrator.GetModifiedDamage(explosiveEndDamage, PlayerNum), DamageType.Melee, explosiveEndRange, X + owner.transform.localScale.x * 6f, Y + 7f, null, false))
            {
                EffectsController.CreateWhiteFlashPopSmall(X + owner.transform.localScale.x * 6f, Y + 7f);
                SortOfFollow.Shake(1f);
            }
            if (impaledUnits.Count > 0)
            {
                SortOfFollow.Shake(0.6f);
                sound.PlaySoundEffectAt(special2Sounds, sliceVolume * 2f, owner.transform.position, 1f, true, false, false, 0f);
            }
            UnimpaleAll();
            owner.yI = owner.jumpForce;
            postDashSpinningUpTime = postDashSpinUpTime;
            owner.xIBlast = owner.transform.localScale.x * postDashXIBlast;
            owner.xI = 0f;
            postDashSpinning = true;
            hero.ChangeFrame();
        }

        public override bool HandleChangeFrame()
        {
            if (dashTime > 0f)
            {
                hero.Sprite.SetLowerLeftPixel(spinSpriteColumn * hero.SpritePixelWidth, spinSpriteRow * hero.SpritePixelHeight);
                hero.DeactivateGun();
                return false;
            }
            return true;
        }

        public override bool HandleDamage(int damage, DamageType damageType, float xI, float yI, int direction, MonoBehaviour damageSender, float hitX, float hitY)
        {
            return dashTime <= 0f;
        }

        public override bool HandleCalculateMovement(ref float xI, ref float yI)
        {
            postDashSpinningUpTime -= hero.DeltaTime;
            return true;
        }

        public override void HandleAfterCalculateMovement()
        {
            if (dashTime <= 0f)
            {
                if (owner.Y + owner.headHeight > owner.GetFieldValue<float>("ceilingHeight") - 2f)
                {
                    postDashSpinningUpTime = 0f;
                }
                if (postDashSpinning && postDashSpinningUpTime > 0f)
                {
                    owner.yI = owner.jumpForce;
                }
            }
        }

        public override bool HandleIsOverLadder()
        {
            return dashTime <= 0f;
        }

        public override bool HandleWallDrag(bool value)
        {
            if (value)
            {
                postDashSpinning = false;
            }
            return true;
        }

        public override bool HandleAnimateActualJumpingFrames()
        {
            if (postDashSpinning)
            {
                hero.FrameRate = 0.025f;
                int column = 11 + (9 - owner.frame % 10);
                hero.Sprite.SetLowerLeftPixel(column * hero.SpritePixelWidth, spinJumpSpriteRow * hero.SpritePixelHeight);
                hero.DeactivateGun();
                return false;
            }
            return true;
        }

        public override void HandleAfterLand()
        {
            postDashSpinning = false;
        }

        private void UnimpaleAll()
        {
            for (int i = impaledUnits.Count - 1; i >= 0; i--)
            {
                if (impaledUnits[i] != null)
                {
                    int damage = 5 + (impaledUnits[i].IsHeavy() ? 10 : 0);
                    impaledUnits[i].Unimpale(damage, DamageType.Melee, 0f, Random.Range(250f, 500f), owner);
                    impaledUnits[i].Knock(DamageType.Normal, Random.Range(-100f, 100f) + owner.xI / 10f, Random.Range(5000f, 10000f), true);
                    impaledUnits[i].yI += Random.Range(0f, 150f);
                    impaledUnits[i].SetFriendlyExplosion();
                }
                impaledUnits.RemoveAt(i);
            }
            impaledUnits.Clear();
        }
    }
}
