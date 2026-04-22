using System.Collections.Generic;
using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using BroMakerLib.Extensions;
using Newtonsoft.Json;
using RocketLib.Extensions;
using UnityEngine;
using World.Generation.MapGenV4;

namespace BroMakerLib.Vanilla.Specials
{
    /// <summary>The Brolander's Quickening lightning-burst special.</summary>
    [SpecialPreset("TheBrolander")]
    public class TheBrolanderSpecial : SpecialAbility
    {
        protected override HeroType SourceBroType => HeroType.TheBrolander;
        public int loopColumn = 7;

        public TheBrolanderSpecial()
        {
            animationRow = 11;
            animationColumn = 0;
        }

        public int zapRange = 100;
        public int maxSpecialAmmo = 5;
        public int enemiesSlainPerPowerValue = 5;
        /// <summary>Seconds the bro lingers as "immortal dead" before auto-resurrecting.</summary>
        public float immortalDeathDuration = 2.6f;

        [JsonIgnore]
        private ElectricZap zapper;
        [JsonIgnore]
        private AudioSource quickeningAudio;
        [JsonIgnore]
        private float zapAngle;
        [JsonIgnore]
        private int zapCount;
        [JsonIgnore]
        private bool releasedSpecial;
        [JsonIgnore]
        private Unit lastZappedUnit;
        [JsonIgnore]
        private RaycastHit raycastHit;
        [JsonIgnore]
        private float levelUpZapTime;
        [JsonIgnore]
        private float levelUpZapCounter;
        [JsonIgnore]
        private float immortalDeathCounter;
        [JsonIgnore]
        private float leveledUpCounter;
        [JsonIgnore]
        private int leveledUpTrailCount;
        [JsonIgnore]
        private int levelUpZapCount;
        [JsonIgnore]
        private List<Unit> alreadyHit = new List<Unit>();
        [JsonIgnore]
        private int enemiesSlain;
        [JsonIgnore]
        private float originalSpeed;

        public override void Initialize(BroBase owner)
        {
            base.Initialize(owner);

            var brolander = owner as TheBrolander;
            if (brolander == null)
            {
                var prefab = HeroController.GetHeroPrefab(HeroType.TheBrolander);
                brolander = prefab as TheBrolander;
            }
            if (brolander != null)
            {
                if (zapRange == 100) zapRange = brolander.zapRange;
                if (maxSpecialAmmo == 5) maxSpecialAmmo = brolander.maxSpecialAmmo;
                if (enemiesSlainPerPowerValue == 5) enemiesSlainPerPowerValue = brolander.enemiesSlainPerPowerValue;
                zapper = brolander.zapper;
            }

            var ownerBrolander = owner as TheBrolander;
            if (ownerBrolander != null)
            {
                quickeningAudio = ownerBrolander.quickeningAudio;
                enemiesSlain = ownerBrolander.GetFieldValue<int>("enemiesSlain");
                originalSpeed = ownerBrolander.GetFieldValue<float>("originalSpeed");
            }
            else
            {
                originalSpeed = owner.speed;
                if (brolander != null && brolander.quickeningAudio != null)
                {
                    quickeningAudio = owner.gameObject.AddComponent<AudioSource>();
                    quickeningAudio.clip = brolander.quickeningAudio.clip;
                    quickeningAudio.loop = brolander.quickeningAudio.loop;
                    quickeningAudio.volume = 0f;
                    quickeningAudio.playOnAwake = false;
                }
            }

            levelUpZapTime = 0.6f;

            if (owner.faderSpritePrefab == null && brolander != null)
            {
                owner.faderSpritePrefab = brolander.faderSpritePrefab;
            }
        }

        public override void PressSpecial()
        {
            if (owner.hasBeenCoverInAcid || hero.UsingSpecial || owner.health <= 0 || hero.DoingMelee)
            {
                return;
            }
            if (owner.SpecialAmmo > 0)
            {
                owner.frame = 0;
                hero.UsingSpecial = true;
                hero.ChangeFrame();
                releasedSpecial = false;
                zapCount = 0;
                return;
            }
            HeroController.FlashSpecialAmmo(PlayerNum);
            hero.ActivateGun();
        }

        public override void AnimateSpecial()
        {
            hero.SetSpriteOffset(0f, 0f);
            hero.DeactivateGun();
            if (owner.frame < 7)
            {
                hero.FrameRate = 0.045f;
                int num = animationColumn + Mathf.Clamp(owner.frame, 0, 8);
                hero.Sprite.SetLowerLeftPixel(num * hero.SpritePixelWidth, hero.SpritePixelHeight * animationRow);
            }
            else
            {
                hero.FrameRate = 0.025f;
                int num2 = loopColumn + (owner.frame - 7) % 3;
                hero.Sprite.SetLowerLeftPixel(num2 * hero.SpritePixelWidth, hero.SpritePixelHeight * animationRow);
            }
            if (owner.frame > 3 && owner.frame % 3 == 0 && owner.SpecialAmmo > 0)
            {
                owner.SetInvulnerable(0.45f, false, false);
                PerformZap();
                if (zapCount % 15 == 0)
                {
                    ReduceSpecialAmmo();
                    if (releasedSpecial)
                    {
                        hero.UsingSpecial = false;
                    }
                }
                if (owner.SpecialAmmo <= 0)
                {
                    hero.UsingSpecial = false;
                }
            }
        }

        public override void UseSpecial()
        {
        }

        public override bool HandleDamage(int damage, DamageType damageType, float xI, float yI, int direction, MonoBehaviour damageSender, float hitX, float hitY)
        {
            if (owner.health <= 0 && CanResurrect())
            {
                immortalDeathCounter = 0f;
            }
            if (hero.UsingSpecial)
            {
                return false;
            }
            return true;
        }

        public override bool HandleReleaseSpecial()
        {
            releasedSpecial = true;
            return true;
        }

        private void PerformZap()
        {
            float num = -zapAngle + zapAngle * 2f * Random.value;
            Vector3 vector = global::Math.Point3OnCircle(num + 1.5707964f, 1f);
            Vector3 vector2 = new Vector3(X, Y + 9f, 5f);
            ActuallyZap(ref vector, ref vector2);
            num = -num;
            vector = global::Math.Point3OnCircle(num + 1.5707964f, 1f);
            ActuallyZap(ref vector, ref vector2);
            zapAngle += 0.14f;
            if (zapAngle > 1.5707964f)
            {
                zapAngle = 1.5707964f;
            }
            zapCount++;
            if (zapCount % 4 == 0)
            {
                owner.X += owner.transform.localScale.x * 3f;
                owner.CallMethod("CreateElectricShockPuff", 2f);
                owner.X -= owner.transform.localScale.x * 3f;
            }
            Sound.GetInstance().PlaySoundEffectAt(specialAttackSounds, 0.5f, owner.transform.position, 1f + owner.pitchShiftAmount, true, false, false, 0f);
            SortOfFollow.Shake(0.2f);
            FullScreenFlashEffect.FlashLightning(0.3f);
            if (zapCount % 3 == 1)
            {
                Map.HitUnits(owner, PlayerNum, 2, 1, DamageType.Plasma, 20f, 16f, X, Y + 4f, 0f, 0f, true, false, true, alreadyHit, false, false);
                alreadyHit.Clear();
            }
        }

        private void ActuallyZap(ref Vector3 direction, ref Vector3 start)
        {
            LayerMask groundLayer = hero.GroundLayer;
            Unit nearestEnemyUnit = Map.GetNearestEnemyUnit(PlayerNum, zapRange + 16, zapRange / 2, X - Mathf.Sign(direction.x) * 10f, Y + (float)zapRange * 0.35f, true, (int)Mathf.Sign(direction.x), lastZappedUnit);
            Vector3 vector = ((!(nearestEnemyUnit != null)) ? Vector3.up : (nearestEnemyUnit.transform.position - start));
            float magnitude = vector.magnitude;
            Map.DeflectProjectiles(owner, PlayerNum, 48f, X, Y + 24f, 0f, false);
            if (nearestEnemyUnit != null && nearestEnemyUnit.Y > Y - 16f && Physics.Raycast(start, vector, out raycastHit, magnitude - 8f, groundLayer))
            {
                lastZappedUnit = null;
                zapper.Create(start, raycastHit.point, new DamageObject(5, DamageType.Plasma, 0f, 0f, X, Y, owner), null, raycastHit.collider.gameObject, -1, -1, 0);
            }
            else if (nearestEnemyUnit != null && nearestEnemyUnit.Y > Y - 16f && nearestEnemyUnit != lastZappedUnit && magnitude < (float)(zapRange + 16))
            {
                lastZappedUnit = nearestEnemyUnit;
                zapper.Create(start, lastZappedUnit.transform.position + Vector3.up * 9f, new DamageObject(5, DamageType.Plasma, 0f, 0f, X, Y, owner), lastZappedUnit, null, -1, -1, 0);
            }
            else if (Physics.Raycast(start, direction, out raycastHit, (float)zapRange, groundLayer))
            {
                lastZappedUnit = null;
                zapper.Create(start, raycastHit.point, new DamageObject(5, DamageType.Plasma, 0f, 0f, X, Y, owner), null, raycastHit.collider.gameObject, -1, -1, 0);
            }
            else
            {
                lastZappedUnit = null;
                zapper.Create(start, start + direction * (float)zapRange * 0.8f, new DamageObject(5, DamageType.Plasma, 0f, 0f, X, Y, owner), null, null, -1, -1, 0);
            }
            lastZappedUnit = nearestEnemyUnit;
        }

        private void ReduceSpecialAmmo()
        {
            if (owner.SpecialAmmo > 0)
            {
                owner.SpecialAmmo--;
                enemiesSlain -= enemiesSlainPerPowerValue;
                SyncEnemiesSlain();
                CalculateImmortalValues();
            }
        }

        private void SyncEnemiesSlain()
        {
            var brolander = owner as TheBrolander;
            if (brolander != null)
            {
                brolander.SetFieldValue("enemiesSlain", enemiesSlain);
            }
        }

        private void CalculateImmortalValues()
        {
            var brolander = owner as TheBrolander;
            if (brolander != null)
            {
                brolander.CallMethod("CalculateImmortalValues");
                return;
            }
            int num = hero.SpecialAmmoField * enemiesSlainPerPowerValue;
            owner.speed = originalSpeed * (0.85f + Mathf.Clamp(0.035f * Mathf.Sqrt((float)num) + 0.012f * (float)num, 0f, 0.66f));
            if (!ProcGenGameMode.UseProcGenRules)
            {
                owner.SetFieldValue("jumpForceMultiplier", 0.95f + Mathf.Clamp(0.025f * Mathf.Sqrt((float)num) + 0.002f * (float)num, 0f, 0.5f));
            }
            else
            {
                owner.SetFieldValue("jumpForceMultiplier", 1f + Mathf.Clamp(0.02f * Mathf.Sqrt((float)num) + 0.002f * (float)num, 0f, 0.5f));
            }
            if (num >= enemiesSlainPerPowerValue * 5)
            {
                owner.SetFieldValue("groundSwordDamage", 20);
            }
            else if (num >= enemiesSlainPerPowerValue * 2)
            {
                owner.SetFieldValue("groundSwordDamage", 10);
            }
            else
            {
                owner.SetFieldValue("groundSwordDamage", 5);
            }
        }

        private bool CanResurrect()
        {
            return hero.SpecialAmmoField >= 2;
        }

        private void CreateFaderTrailInstance()
        {
            if (owner.faderSpritePrefab == null) return;
            FaderSprite component = owner.faderSpritePrefab.GetComponent<FaderSprite>();
            if (component == null) return;
            FaderSprite fader = EffectsController.InstantiateEffect(component, owner.transform.position, owner.transform.rotation) as FaderSprite;
            if (fader != null)
            {
                fader.transform.localScale = owner.transform.localScale;
                fader.SetMaterial(owner.GetComponent<Renderer>().material, hero.Sprite.lowerLeftPixel, hero.Sprite.pixelDimensions, hero.Sprite.offset);
            }
        }

        private void RunQuickeningAudio()
        {
            if (quickeningAudio == null) return;
            if (!quickeningAudio.isPlaying)
            {
                quickeningAudio.volume = 0f;
                quickeningAudio.Play();
            }
            else
            {
                quickeningAudio.volume += hero.DeltaTime * 2f;
                if (quickeningAudio.volume > 0.5f)
                {
                    quickeningAudio.volume = 0.5f;
                }
            }
        }

        public override void Update()
        {
            if (owner.health > 0)
            {
                if (hero.UsingSpecial)
                {
                    RunQuickeningAudio();
                }
                else if (quickeningAudio != null && quickeningAudio.isPlaying)
                {
                    quickeningAudio.volume -= hero.DeltaTime * 2f;
                    if (quickeningAudio.volume <= 0f)
                    {
                        quickeningAudio.Stop();
                    }
                }
            }
            else if (CanResurrect())
            {
                RunQuickeningAudio();
                immortalDeathCounter += hero.DeltaTime;
                levelUpZapTime = 1f;
                if (immortalDeathCounter > immortalDeathDuration - 0.1f || Time.time - hero.DeathTime > 5f)
                {
                    owner.health = 1;
                    owner.yI = 260f;
                    owner.actionState = ActionState.Jumping;
                    owner.SetInvulnerable(0.5f, true, false);
                    owner.CallMethod("CreateElectricShockPuff", 2f);
                    owner.SetFieldValue("deadTimeCounter", 0f);
                    hero.SpecialAmmoField = 0;
                    enemiesSlain = 0;
                    SyncEnemiesSlain();
                    CalculateImmortalValues();
                    if (enemiesSlain <= 0)
                    {
                        enemiesSlain = 0;
                        SyncEnemiesSlain();
                    }
                    hero.CurrentDeathType = DeathType.Unassigned;
                }
            }
            else if (quickeningAudio != null && quickeningAudio.isPlaying)
            {
                quickeningAudio.Stop();
            }
            if (levelUpZapTime > 0f)
            {
                levelUpZapTime -= hero.DeltaTime;
                levelUpZapCounter += hero.DeltaTime * 0.5f + ((owner.health <= 0) ? 0f : (hero.DeltaTime * 0.5f));
                if (levelUpZapCounter > 0.2f)
                {
                    if (!owner.invulnerable && owner.health > 0)
                    {
                        owner.SetInvulnerable(0.25f, true, false);
                    }
                    if (owner.health <= 0 && immortalDeathCounter > 0.5f)
                    {
                        float num = -1.2566371f + 2.5132742f * Random.value;
                        Vector3 vector = global::Math.Point3OnCircle(num + 1.5707964f, 1f);
                        Vector3 vector2 = new Vector3(X, Y + 9f, 5f);
                        zapper.Create(vector2 + vector * 100f, vector2, new DamageObject(5, DamageType.Plasma, 0f, 0f, X, Y, owner), null, null, -1, -1, 0);
                    }
                    levelUpZapCounter -= 0.2f;
                    Sound.GetInstance().PlaySoundEffectAt(specialAttackSounds, 0.23f, owner.transform.position, 1f + owner.pitchShiftAmount, true, false, false, 0f);
                    owner.CallMethod("CreateElectricShockPuff", 3f);
                    levelUpZapCount++;
                    if (levelUpZapCount % 2 == 0)
                    {
                        Map.HitUnits(owner, PlayerNum, 3, 1, DamageType.Plasma, 14f, 10f, X, Y + 4f, 0f, 0f, true, false, true, alreadyHit, false, false);
                        alreadyHit.Clear();
                    }
                }
            }
            if (owner.health > 0 && hero.SpecialAmmoField >= maxSpecialAmmo)
            {
                leveledUpCounter += hero.DeltaTime;
                if (leveledUpCounter > 0.045f)
                {
                    leveledUpCounter -= 0.045f;
                    CreateFaderTrailInstance();
                    leveledUpTrailCount++;
                    if (leveledUpTrailCount % 15 == 0)
                    {
                        Sound.GetInstance().PlaySoundEffectAt(specialAttackSounds, 0.13f, owner.transform.position, 1f + owner.pitchShiftAmount, true, false, false, 0f);
                        owner.CallMethod("CreateElectricShockPuff", 3f);
                        Map.HitUnits(owner, PlayerNum, 3, 1, DamageType.Plasma, 14f, 10f, X, Y + 4f, 0f, 0f, true, false, true, alreadyHit, false, false);
                        alreadyHit.Clear();
                    }
                }
            }
            else if (owner.health > 0 && hero.SpecialAmmoField >= maxSpecialAmmo - 1)
            {
                leveledUpCounter += hero.DeltaTime;
                if (leveledUpCounter > 0.1f)
                {
                    leveledUpCounter -= 0.1f;
                    CreateFaderTrailInstance();
                    leveledUpTrailCount++;
                    if (leveledUpTrailCount % 25 == 0)
                    {
                        Sound.GetInstance().PlaySoundEffectAt(specialAttackSounds, 0.13f, owner.transform.position, 1f + owner.pitchShiftAmount, true, false, false, 0f);
                        owner.CallMethod("CreateElectricShockPuff", 3f);
                        Map.HitUnits(owner, PlayerNum, 3, 1, DamageType.Plasma, 14f, 10f, X, Y + 4f, 0f, 0f, true, false, true, alreadyHit, false, false);
                        alreadyHit.Clear();
                    }
                }
            }
        }

        public override void HandleAfterCalculateMovement()
        {
            if (hero.UsingSpecial)
            {
                owner.canWallClimb = false;
                owner.xI *= 1f - hero.DeltaTime * 12f;
            }
            else
            {
                owner.canWallClimb = true;
            }
        }

        public override bool HandleIsOverLadder()
        {
            if (hero.UsingSpecial)
            {
                return false;
            }
            return true;
        }

        public override bool HandleCheckForTraps()
        {
            if (hero.UsingSpecial)
            {
                return false;
            }
            return true;
        }

        public override bool HandleDeath(float xI, float yI, DamageObject damage)
        {
            if (quickeningAudio != null)
            {
                quickeningAudio.Stop();
            }
            hero.UsingSpecial = false;
            if (CanResurrect())
            {
                immortalDeathCounter = 0f;
                levelUpZapTime = immortalDeathDuration + 0.5f;
                levelUpZapCounter = 0f;
            }
            return true;
        }

        public override void HandleAfterDeath(float xI, float yI, DamageObject damage)
        {
            Map.ForgetPlayer(PlayerNum, false, true);
        }

        public override bool HandleIsAlive(ref bool result)
        {
            if (CanResurrect())
            {
                result = true;
                return false;
            }
            return true;
        }

        public override bool HandleCanReduceLives(ref bool result)
        {
            if (CanResurrect())
            {
                result = false;
                return false;
            }
            return true;
        }

        public override bool HandleCheckNotifyDeathType()
        {
            if (Time.time - hero.DeathTime > immortalDeathDuration || !CanResurrect())
            {
                return true;
            }
            return false;
        }

        public override bool HandleIsInStealthMode(ref bool result)
        {
            if (owner.health <= 0 && CanResurrect())
            {
                result = true;
                return false;
            }
            return true;
        }

        public override bool HandleGib(DamageType damageType, float xI, float yI)
        {
            return true;
        }

        public override void Cleanup()
        {
            if (quickeningAudio != null && !(owner is TheBrolander))
            {
                Object.Destroy(quickeningAudio);
            }
        }
    }
}
