using System.Collections.Generic;
using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using BroMakerLib.Extensions;
using Newtonsoft.Json;
using RocketLib.Extensions;
using UnityEngine;

namespace BroMakerLib.Vanilla.Specials
{
    [SpecialPreset("BroniversalSoldier")]
    public class BroniversalSoldierSpecial : SpecialAbility
    {
        protected override HeroType SourceBroType => HeroType.BroniversalSoldier;

        protected override void CacheSoundsFromPrefab()
        {
            base.CacheSoundsFromPrefab();
            var sourceBro = HeroController.GetHeroPrefab(SourceBroType);
            if (sourceBro == null) return;
            if (special4Sounds == null) special4Sounds = sourceBro.soundHolder.special4Sounds.CloneArray();
        }
        public float serumDuration = 6.5f;
        public float deathGracePeriod = 0.66f;
        public float reviveRadius = 15f;
        public float reviveInterval = 0.1f;
        public float reviveSoundVolume = 0.6f;
        public int overkillThreshold = 35;
        public AudioClip[] special4Sounds;

        [JsonIgnore]
        private bool serumFrenzy;
        [JsonIgnore]
        private float serumTime;
        [JsonIgnore]
        private float serumCounter;
        [JsonIgnore]
        private float cannotGibTime;
        [JsonIgnore]
        private int overkillDamage;
        [JsonIgnore]
        private bool fullyDead;
        [JsonIgnore]
        private float originalSpeed;
        [JsonIgnore]
        private Material armedGunMaterial;
        [JsonIgnore]
        private ReviveBlast reviveBlastPrefab;
        [JsonIgnore]
#pragma warning disable 618
        private ParticleEmitter serumParticles;
#pragma warning restore 618
        public AudioClip[] reviveClips;
        [JsonIgnore]
        private List<ReviveBlast> reviveQueue = new List<ReviveBlast>();

        private float OwnerDeathTime
        {
            get => hero.DeathTime;
            set => hero.DeathTime = value;
        }

        public override void Initialize(TestVanDammeAnim owner)
        {
            base.Initialize(owner);
            originalSpeed = owner.speed;

            var soldier = owner as BroniversalSoldier;
            if (soldier == null)
            {
                var prefab = HeroController.GetHeroPrefab(HeroType.BroniversalSoldier);
                soldier = prefab as BroniversalSoldier;
            }
            if (soldier != null)
            {
                reviveBlastPrefab = soldier.reviveBlastPrefab;
                if (reviveClips == null)
                    reviveClips = soldier.reviveClips;
            }

            var ownerSoldier = owner as BroniversalSoldier;
            if (ownerSoldier != null)
            {
                serumParticles = ownerSoldier.serumParticles;
            }
            else if (soldier != null && soldier.serumParticles != null)
            {
                serumParticles = Object.Instantiate(soldier.serumParticles, owner.transform);
                serumParticles.transform.localPosition = soldier.serumParticles.transform.localPosition;
            }
            if (serumParticles != null)
            {
                serumParticles.emit = false;
            }
        }

        public override void PressSpecial()
        {
            if (owner.health <= 0)
            {
                if (Time.time - OwnerDeathTime < deathGracePeriod - 0.0333f && owner.SpecialAmmo > 0)
                {
                    UseSpecial();
                    if (owner.hasBeenCoverInAcid)
                    {
                        owner.RemoveAcid();
                    }
                }
            }
            else
            {
                base.PressSpecial();
            }
        }

        public override void UseSpecial()
        {
            if (owner.SpecialAmmo > 0)
            {
                Sound.GetInstance().PlaySoundEffectAt(special4Sounds, 0.33f, owner.transform.position);
                owner.SpecialAmmo--;
                if (owner.IsMine && reviveBlastPrefab != null)
                {
                    ReviveBlast reviveBlast = Networking.Networking.Instantiate<ReviveBlast>(reviveBlastPrefab,
                        new Vector3(owner.transform.position.x, owner.transform.position.y + 8f, -9f), Quaternion.identity, false);
                    reviveQueue.Add(reviveBlast);
                    reviveBlast.Setup(PlayerNum, owner);
                }
                cannotGibTime = serumDuration;
                owner.canGib = false;
                if (armedGunMaterial == null)
                {
                    armedGunMaterial = owner.gunSprite.GetComponent<Renderer>().material;
                }
                serumFrenzy = true;
                if (serumParticles != null)
                {
                    serumParticles.emit = true;
                }
                serumTime = serumDuration;
                HeroController.OneBroUseSteroids(PlayerNum);
            }
            else
            {
                HeroController.FlashSpecialAmmo(PlayerNum);
                hero.ActivateGun();
            }
        }

        public override bool HandleDamage(int damage, DamageType damageType, float xI, float yI, int direction, MonoBehaviour damageSender, float hitX, float hitY)
        {
            if (owner.health <= 0 && damageType != DamageType.SelfEsteem)
            {
                overkillDamage += damage;
                if (overkillDamage > overkillThreshold)
                {
                    serumFrenzy = false;
                    if (serumParticles != null)
                    {
                        serumParticles.emit = false;
                    }
                    owner.health = -9;
                }
            }
            return true;
        }

        public override bool HandleCanReduceLives(ref bool result)
        {
            if (serumFrenzy)
            {
                result = false;
                return false;
            }
            return true;
        }

        public override void HandleAfterDeath()
        {
            if (serumFrenzy)
            {
                OwnerDeathTime = Time.time;
                Map.ForgetPlayer(PlayerNum, false, true);
                if (serumTime < 1f)
                {
                    serumTime = 1f;
                }
                owner.SetFieldValue("hasReportedKill", false);
            }
        }

        public override bool HandleGib(DamageType damageType, float xI, float yI)
        {
            if (serumFrenzy && damageType != DamageType.OutOfBounds)
            {
                return false;
            }
            if (hero.CurrentDeathType == DeathType.Unassigned || Time.time - OwnerDeathTime < 0.33f)
            {
                owner.CallMethod("SetDeathType", damageType, -100);
                owner.CallMethod("NotifyDeathType");
            }
            return true;
        }

        public override void Update()
        {
            if (owner.health >= 0 && serumTime > 0f)
            {
                serumTime -= hero.DeltaTime;
                if (serumTime <= 0f)
                {
                    StopSerumFrenzy();
                }
                serumCounter += hero.DeltaTime;
                if (serumCounter >= reviveInterval)
                {
                    serumCounter -= reviveInterval;
                    if (Map.ReviveDeadUnits(X, Y, reviveRadius, PlayerNum, 1, PlayerNum >= 0, owner, false))
                    {
                        Sound.GetInstance().PlaySoundEffectAt(reviveClips, reviveSoundVolume, owner.transform.position,
                            0.9f + Random.value * 0.2f, true, false, false, 0f);
                    }
                }
            }
            for (int i = reviveQueue.Count - 1; i >= 0; i--)
            {
                if (reviveQueue[i] != null)
                {
                    reviveQueue.RemoveAt(i);
                }
            }
            if (cannotGibTime > 0f)
            {
                cannotGibTime -= hero.DeltaTime;
                if (cannotGibTime <= 0f)
                {
                    owner.canGib = true;
                }
            }
            if (owner.health <= 0 && !serumFrenzy && !fullyDead && Time.time - OwnerDeathTime > deathGracePeriod)
            {
                fullyDead = true;
                owner.actionState = ActionState.Idle;
                owner.Death(owner.xI * 3.3334f, owner.yI * 3.3334f, null);
            }
            if (owner.health <= 0 && serumFrenzy && !fullyDead && Time.time - OwnerDeathTime >= deathGracePeriod)
            {
                owner.SetInvulnerable(0.2f, true, false);
                cannotGibTime = 0.2f;
                serumTime = 0.2f;
                owner.yI = 110f;
                owner.health = 1;
                owner.actionState = ActionState.Jumping;
                hero.ChangeFrame();
                OwnerDeathTime = -10f;
                owner.SetFieldValue("performanceEnhancedTime", 0.02f);
                HeroController.SetAvatarCalm(PlayerNum, owner.usePrimaryAvatar);
                owner.SetFieldValue("isZombie", false);
                owner.CallMethod("SetSyncingInternal", true);
                owner.deathNotificationSent = false;
            }
        }

        public override bool HandleIsAlive(ref bool result)
        {
            if (Time.time - OwnerDeathTime <= deathGracePeriod)
            {
                result = true;
                return false;
            }
            return true;
        }

        public override bool HandleRevive(int playerNum, bool isUnderPlayerControl, TestVanDammeAnim reviveSource)
        {
            OwnerDeathTime = -10f;
            overkillDamage = 0;
            serumTime = 0.25f;
            return true;
        }

        public override void HandleAfterRevive(int playerNum, bool isUnderPlayerControl, TestVanDammeAnim reviveSource)
        {
            if (owner.health > 0)
            {
                fullyDead = false;
            }
            if (reviveSource == owner)
            {
                HeroController.SetAvatarCalm(playerNum, owner.usePrimaryAvatar);
                owner.SetFieldValue("isZombie", false);
            }
        }

        public override void HandleAfterUseSteroids()
        {
            if (serumFrenzy)
            {
                owner.SetFieldValue("performanceEnhancedTime", serumTime);
            }
        }

        public override bool HandleCheckNotifyDeathType()
        {
            if (Time.time - OwnerDeathTime <= deathGracePeriod)
            {
                return false;
            }
            return true;
        }

        private void StopSerumFrenzy()
        {
            if (serumParticles != null)
            {
                serumParticles.emit = false;
            }
            owner.speed = originalSpeed;
            serumFrenzy = false;
            serumTime = 0f;
            owner.gunSprite.GetComponent<Renderer>().material = armedGunMaterial;
        }

        public override void Cleanup()
        {
            if (serumParticles != null && !(owner is BroniversalSoldier))
            {
                Object.Destroy(serumParticles.gameObject);
            }
        }
    }
}
