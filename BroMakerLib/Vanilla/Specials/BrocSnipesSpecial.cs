using System.Collections.Generic;
using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using Newtonsoft.Json;
using RocketLib.Extensions;
using UnityEngine;

namespace BroMakerLib.Vanilla.Specials
{
    [SpecialPreset("BrocSnipes")]
    public class BrocSnipesSpecial : SpecialAbility
    {
        public override void Initialize(TestVanDammeAnim owner)
        {
            base.Initialize(owner);
            var prefab = HeroController.GetHeroPrefab(HeroType.Broc);
            var sourceBro = prefab.GetComponent<TestVanDammeAnim>();
            if (specialAttackSounds == null)
            {
                specialAttackSounds = sourceBro.soundHolder.specialAttackSounds;
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
        public float deflectRange = 16f;
        public int terrainDamage = 3;
        public float impaleDamage = 5;
        public float sliceVolume = 0.7f;
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
        private List<Unit> impaledUnits = new List<Unit>();
        [JsonIgnore]
        private bool localChimneyFlip;
        [JsonIgnore]
        private int localChimneyFlipFrames;
        [JsonIgnore]
        private int localChimneyFlipDirection;

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
                owner.CallMethod("ClearCurrentAttackVariables");
                if (owner.attachedToZipline != null)
                {
                    owner.attachedToZipline.DetachUnit(owner);
                }
                owner.CallMethod("StopAirDashing");
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
                bool flag;
                Map.DamageDoodads(terrainDamage, DamageType.Knifed, X + owner.Direction * 4f, Y, 0f, 0f, 6f, PlayerNum, out flag, null);
                if (owner.IsMine)
                {
                    MapController.DamageGround(owner, terrainDamage, DamageType.Melee, 8f,
                        X + owner.transform.localScale.x * 16f, Y + 7f, null, false);
                }
                Unit firstUnit = Map.GetFirstUnit(owner, PlayerNum, 16f,
                    X + owner.transform.localScale.x * 24f, Y + 4f, false, true, impaledUnits);
                if (firstUnit != null)
                {
                    impaledUnits.Add(firstUnit);
                    firstUnit.Impale(owner.transform, Vector3.right * Mathf.Sign(owner.xI), 0, owner.xI, owner.yI,
                        owner.transform.localScale.x * 8f, 0f);
                }
                owner.CallMethod("CreateFaderTrailInstance");
            }

            if (dashTime <= 0f)
            {
                EndDash();
            }

            Map.HurtWildLife(X + owner.transform.localScale.x * 13f, Y + 5f, 16f);
            return true;
        }

        private void EndDash()
        {
            hero.SetGunSprite(0, 0);
            owner.actionState = ActionState.Jumping;
            if (impaledUnits.Count > 0)
            {
                SortOfFollow.Shake(0.6f);
                sound.PlaySoundEffectAt(soundHolder.special2Sounds, sliceVolume * 2f, owner.transform.position, 1f, true, false, false, 0f);
            }
            for (int i = impaledUnits.Count - 1; i >= 0; i--)
            {
                if (impaledUnits[i] != null)
                {
                    impaledUnits[i].Unimpale((int)impaleDamage, DamageType.Melee, 0f, Random.Range(250f, 500f), owner);
                    impaledUnits[i].Knock(DamageType.Normal, Random.Range(-100f, 100f) + owner.xI / 10f, Random.Range(5000f, 10000f), true);
                    impaledUnits[i].yI += Random.Range(0f, 150f);
                }
                impaledUnits.RemoveAt(i);
            }
            owner.yI = owner.jumpForce;
            owner.xI = 0f;
            var brocSnipes = owner as BrocSnipes;
            if (brocSnipes != null)
            {
                owner.SetFieldValue("chimneyFlip", true);
                owner.SetFieldValue("chimneyFlipFrames", 11);
                owner.SetFieldValue("chimneyFlipDirection", 0);
                owner.CallMethod("AnimateChimneyFlip");
            }
            else
            {
                localChimneyFlip = true;
                localChimneyFlipFrames = 11;
                localChimneyFlipDirection = 0;
            }
            hero.ChangeFrame();
        }

        public override bool HandleChangeFrame()
        {
            if (localChimneyFlip)
            {
                if (owner.health <= 0)
                {
                    localChimneyFlip = false;
                    return true;
                }
                hero.DeactivateGun();
                hero.FrameRate = 0.033f;
                if (localChimneyFlipFrames > 0)
                {
                    localChimneyFlipFrames--;
                }
                if (localChimneyFlipFrames <= 0)
                {
                    localChimneyFlip = false;
                    localChimneyFlipFrames = 0;
                }
                int chimneyColumn = 22 - localChimneyFlipFrames;
                hero.Sprite.SetLowerLeftPixel(chimneyColumn * hero.SpritePixelWidth, 7 * hero.SpritePixelHeight);
                return false;
            }
            if (dashTime <= 0f)
            {
                return true;
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

            owner.gunSprite.SetLowerLeftPixel(dashGunColumn * 32f, 32f);
            return false;
        }
    }
}
