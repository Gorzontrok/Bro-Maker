using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using Newtonsoft.Json;
using Rogueforce;
using UnityEngine;

namespace BroMakerLib.Vanilla.Specials
{
    [SpecialPreset("Blade")]
    public class BladeSpecial : SpecialAbility
    {
        public override void Initialize(TestVanDammeAnim owner)
        {
            base.Initialize(owner);
            var prefab = HeroController.GetHeroPrefab(HeroType.Blade);
            var sourceBro = prefab.GetComponent<TestVanDammeAnim>();
            if (specialAttackSounds == null)
            {
                specialAttackSounds = sourceBro.soundHolder.specialAttackSounds;
            }
            if (attackSounds == null)
            {
                attackSounds = sourceBro.soundHolder.attackSounds;
            }
            if (owner.faderSpritePrefab == null)
            {
                owner.faderSpritePrefab = sourceBro.faderSpritePrefab;
            }
        }

        public float dashSpeed = 240f;
        public float dashDuration = 0.5f;
        public float dashCooldown = 0.55f;
        public float specialSoundVolume = 0.7f;
        public int dashDamage = 5;
        public float deflectRange = 16f;
        public float hitRange = 10f;
        public float terrainDamageRange = 24f;
        public int terrainDamage = 3;
        public int dashSpriteRow = 1;
        public int dashSpriteColumn = 23;
        public int dashGunColumn = 11;

        [JsonIgnore]
        private float dashTime;
        [JsonIgnore]
        private float dashStartTime;
        [JsonIgnore]
        private float dashHeight;
        [JsonIgnore]
        private float dashCounter;
        [JsonIgnore]
        private bool showSlashEffect;

        public override void AnimateSpecial()
        {
            if (owner.frame >= 0)
            {
                UseSpecial();
            }
        }

        public override void UseSpecial()
        {
            if (owner.SpecialAmmo > 0 && Time.time - dashStartTime > dashCooldown)
            {
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
                hero.FrameRate = 0.025f;
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
            if (dashTime <= 0f)
            {
                return true;
            }

            owner.invulnerable = true;
            hero.InvulnerableTime = 0.3f;
            dashTime -= hero.DeltaTime;
            owner.xI = dashSpeed * owner.transform.localScale.x;
            owner.yI = 0f;
            owner.Y = dashHeight;

            Map.DeflectProjectiles(owner, PlayerNum, deflectRange,
                X + Mathf.Sign(owner.transform.localScale.x) * 6f, Y + 6f,
                Mathf.Sign(owner.transform.localScale.x) * 200f, true);

            dashCounter += hero.DeltaTime;
            if (dashCounter > 0f)
            {
                dashCounter -= 0.0333f;
                bool hitUnit = Map.HitUnits(owner, owner, PlayerNum, dashDamage, DamageType.Melee, hitRange,
                    X, Y, owner.transform.localScale.x * 420f, 360f, true, true);
                bool flag2;
                Map.DamageDoodads(terrainDamage, DamageType.Knifed, X + owner.Direction * 4f, Y, 0f, 0f, 6f, PlayerNum, out flag2, null);
                if (owner.IsMine)
                {
                    hitUnit = MapController.DamageGround(owner, ValueOrchestrator.GetModifiedDamage(terrainDamage, PlayerNum), DamageType.Melee, terrainDamageRange,
                        X + owner.transform.localScale.x * 16f, Y + 7f, null, false) || hitUnit;
                }
                if (hitUnit)
                {
                    showSlashEffect = true;
                }
                owner.CallMethod("CreateFaderTrailInstance");
            }

            if (dashTime <= 0f)
            {
                hero.SetGunSprite(0, 0);
                owner.actionState = ActionState.Jumping;
                hero.ChangeFrame();
            }

            Map.HurtWildLife(X + owner.transform.localScale.x * 13f, Y + 5f, 16f);
            return true;
        }

        public override bool HandleChangeFrame()
        {
            if (dashTime <= 0f)
            {
                return true;
            }

            hero.FrameRate = 0.025f;
            if (owner.frame % 4 == 0)
            {
                sound.PlaySoundEffectAt(attackSounds, 0.3f, owner.transform.position, 1.5f, true, false, false, 0f);
                if (showSlashEffect)
                {
                    EffectsController.CreateMeleeStrikeLargeEffect(X + Mathf.Sign(owner.xI) * 22f, Y + 8f, -owner.xI * 0.2f, 0f);
                    showSlashEffect = false;
                }
            }
            owner.gunSprite.gameObject.SetActive(true);
            owner.actionState = ActionState.Jumping;
            int column = dashSpriteColumn + owner.frame % 2;
            hero.Sprite.SetLowerLeftPixel(column * hero.SpritePixelWidth, dashSpriteRow * hero.SpritePixelHeight);
            return false;
        }

        public override bool HandleRunGun()
        {
            if (dashTime <= 0f)
            {
                return true;
            }

            int specialAttackFrames = 8;
            owner.gunSprite.SetLowerLeftPixel(dashGunColumn * 32f + owner.frame % specialAttackFrames * 32f, 32f);
            return false;
        }

    }
}
