using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using Newtonsoft.Json;
using RocketLib.Extensions;
using Rogueforce;
using UnityEngine;
using Utility;
using World.Generation.MapGenV4;

namespace BroMakerLib.Vanilla.Specials
{
    [SpecialPreset("Brominator")]
    public class BrominatorSpecial : SpecialAbility
    {
        public float normalDuration = 5.5f;
        public float deathMatchDuration = 4.5f;
        public float procGenDuration = 2f;
        public float speedMultiplier = 0.7f;
        public float knockbackAbsorption = 0.1f;
        public float endInvulnerableTime = 0.5f;

        [JsonIgnore]
        private bool brominatorMode;
        [JsonIgnore]
        private float brominatorTime;
        [JsonIgnore]
        private float originalSpeed;
        [JsonIgnore]
        private Material metalBrominator;
        [JsonIgnore]
        private Material humanBrominator;
        [JsonIgnore]
        private Material metalGunBrominator;
        [JsonIgnore]
        private Material humanGunBrominator;
        [JsonIgnore]
        private Material brominatorRobotAvatar;
        [JsonIgnore]
        private Material brominatorHumanAvatar;
        [JsonIgnore]
        private float miniGunFireDelay;
        [JsonIgnore]
        private AudioSource miniGunAudio;
        [JsonIgnore]
        private Brominator ownerAsBrominator;

        private void SyncModeToOwner(bool mode)
        {
            brominatorMode = mode;
            if (ownerAsBrominator != null)
            {
                ownerAsBrominator.brominatorMode = mode;
            }
        }

        public override void Initialize(TestVanDammeAnim owner)
        {
            base.Initialize(owner);
            originalSpeed = owner.speed;
            ownerAsBrominator = owner as Brominator;

            var brominator = ownerAsBrominator;
            if (brominator == null)
            {
                var prefab = HeroController.GetHeroPrefab(HeroType.Brominator);
                brominator = prefab as Brominator;
            }
            if (brominator != null)
            {
                metalBrominator = brominator.metalBrominator;
                humanBrominator = brominator.humanBrominator;
                metalGunBrominator = brominator.metalGunBrominator;
                humanGunBrominator = brominator.humanGunBrominator;
                brominatorRobotAvatar = brominator.brominatorRobotAvatar;
                brominatorHumanAvatar = brominator.brominatorHumanAvatar;
                miniGunFireDelay = brominator.miniGunFireDelay;
            }

            if (ownerAsBrominator != null)
            {
                miniGunAudio = ownerAsBrominator.GetFieldValue<AudioSource>("miniGunAudio");
                brominatorMode = ownerAsBrominator.brominatorMode;
            }
        }

        public override void UseSpecial()
        {
            if (!brominatorMode)
            {
                if (owner.SpecialAmmo > 0)
                {
                    owner.SpecialAmmo--;
                    HeroController.SetSpecialAmmo(PlayerNum, owner.SpecialAmmo);
                    SyncModeToOwner(true);
                    if (ProcGenGameMode.UseProcGenRules)
                    {
                        brominatorTime = procGenDuration;
                    }
                    else if (!GameModeController.IsDeathMatchMode)
                    {
                        brominatorTime = normalDuration;
                    }
                    else
                    {
                        brominatorTime = deathMatchDuration;
                    }
                    if (metalBrominator != null)
                    {
                        owner.GetComponent<Renderer>().material = metalBrominator;
                    }
                    if (brominatorRobotAvatar != null)
                    {
                        HeroController.SetAvatarMaterial(PlayerNum, brominatorRobotAvatar);
                    }
                    if (metalGunBrominator != null)
                    {
                        owner.gunSprite.GetComponent<Renderer>().material = metalGunBrominator;
                    }
                    owner.speed = originalSpeed * speedMultiplier;
                }
                else
                {
                    HeroController.FlashSpecialAmmo(PlayerNum);
                }
            }
        }

        public override void HandleAfterFireWeapon(float x, float y, float xSpeed, float ySpeed)
        {
            if (ownerAsBrominator != null)
            {
                return;
            }
            float pushBackForceM = owner.GetFieldValue<float>("pushBackForceM");
            if (pushBackForceM == 0f) pushBackForceM = 1f;
            if (!brominatorMode)
            {
                owner.xIBlast -= owner.transform.localScale.x * 4f * pushBackForceM;
                if (y > owner.groundHeight)
                {
                    owner.yI += Mathf.Clamp(3f * pushBackForceM, 3f, 16f);
                }
            }
            else
            {
                owner.xIBlast -= owner.transform.localScale.x * 5f * pushBackForceM;
            }
        }

        public override void HandleAfterAddSpeedLeft()
        {
            if (ownerAsBrominator != null)
            {
                return;
            }
            if (brominatorMode)
            {
                if (owner.xIBlast > owner.speed * 0.12f)
                {
                    owner.xIBlast = owner.speed * 0.12f;
                }
            }
            else if (owner.xIBlast > owner.speed * 1.6f)
            {
                owner.xIBlast = owner.speed * 1.6f;
            }
        }

        public override void HandleAfterAddSpeedRight()
        {
            if (ownerAsBrominator != null)
            {
                return;
            }
            if (brominatorMode)
            {
                if (owner.xIBlast < owner.speed * -0.12f)
                {
                    owner.xIBlast = owner.speed * -0.12f;
                }
            }
            else if (owner.xIBlast < owner.speed * -1.6f)
            {
                owner.xIBlast = owner.speed * -1.6f;
            }
        }

        public override bool HandleCanBeImpaledByGroundSpikes(ref bool result)
        {
            if (brominatorMode)
            {
                result = false;
                return false;
            }
            return true;
        }

        public override bool HandleDamage(int damage, DamageType damageType, float xI, float yI, int direction, MonoBehaviour damageSender, float hitX, float hitY)
        {
            if (!brominatorMode)
            {
                return true;
            }

            Helicopter helicopter = damageSender as Helicopter;
            if (helicopter)
            {
                helicopter.Damage(new DamageObject(helicopter.health, DamageType.Explosion, 0f, 0f, X, Y, owner));
            }
            SawBlade sawBlade = damageSender as SawBlade;
            if (sawBlade != null)
            {
                sawBlade.Damage(new DamageObject(sawBlade.health, DamageType.Explosion, 0f, 0f, X, Y, owner));
            }
            MookDog mookDog = damageSender as MookDog;
            if (mookDog != null)
            {
                mookDog.Panic((int)Mathf.Sign(xI) * -1, 2f, true);
            }
            owner.xIBlast += xI * knockbackAbsorption + damage * 0.03f;
            owner.yI += yI * knockbackAbsorption + damage * 0.03f;
            return false;
        }

        public override void Update()
        {
            if (brominatorMode)
            {
                if (brominatorTime > 0f)
                {
                    brominatorTime -= hero.DeltaTime;
                }
                else
                {
                    SyncModeToOwner(false);
                    if (humanBrominator != null)
                    {
                        owner.GetComponent<Renderer>().material = humanBrominator;
                    }
                    if (humanGunBrominator != null)
                    {
                        owner.gunSprite.GetComponent<Renderer>().material = humanGunBrominator;
                    }
                    if (brominatorHumanAvatar != null)
                    {
                        HeroController.SetAvatarMaterial(PlayerNum, brominatorHumanAvatar);
                    }
                    if (!GameModeController.IsDeathMatchMode)
                    {
                        owner.CallMethod("SetInvulnerable", endInvulnerableTime, true, false);
                    }
                    owner.speed = originalSpeed;
                }
            }
        }

        public override bool HandleRunFiring()
        {
            if (!owner.fire)
            {
                if (!brominatorMode)
                {
                    if (ProcGenGameMode.isEnabled || ProcGenGameMode.ProcGenTestBuild)
                    {
                        int primaryFireLevel = HeroController.GetPrimaryFireLevel(PlayerNum);
                        owner.fireDelay = miniGunFireDelay * (1.5f - (float)primaryFireLevel * 0.33f);
                    }
                    else
                    {
                        owner.fireDelay = miniGunFireDelay;
                    }
                }
                else
                {
                    owner.fireDelay = miniGunFireDelay / 3f;
                }
            }
            return true;
        }

        public override bool HandleDeath()
        {
            if (ownerAsBrominator == null && brominatorMode)
            {
                AchievementManager.AwardAchievement(Achievement.illbeback, PlayerNum);
            }
            return true;
        }

        public override void HandleAfterDeath()
        {
            if (miniGunAudio != null)
            {
                miniGunAudio.Stop();
            }
        }

        public override bool HandleGib(DamageType damageType, float xI, float yI)
        {
            if (ownerAsBrominator == null && brominatorMode)
            {
                AchievementManager.AwardAchievement(Achievement.illbeback, PlayerNum);
            }
            return true;
        }
    }
}
