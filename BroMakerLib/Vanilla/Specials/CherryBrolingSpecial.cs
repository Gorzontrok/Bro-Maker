using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using Newtonsoft.Json;
using UnityEngine;

namespace BroMakerLib.Vanilla.Specials
{
    [SpecialPreset("CherryBroling")]
    public class CherryBrolingSpecial : SpecialAbility
    {
        protected override HeroType SourceBroType => HeroType.CherryBroling;
        public float duckingSpeedX = 400f;
        public float duckingRecoilX = 60f;
        public Vector3 fireLeftDirection = new Vector3(-160f, -330f, 0f);
        public Vector3 fireDownDirection = new Vector3(0f, -370f, 0f);
        public Vector3 fireRightDirection = new Vector3(160f, -330f, 0f);

        [JsonIgnore]
        protected Projectile rocketGrenade;

        [JsonIgnore]
        private bool somersaulting;
        [JsonIgnore]
        private int somersaultFrame;

        public CherryBrolingSpecial()
        {
            spawnOffsetX = 10f;
            spawnOffsetY = 6f;
        }

        public override void Initialize(TestVanDammeAnim owner)
        {
            base.Initialize(owner);
            var sourceBro = HeroController.GetHeroPrefab(HeroType.CherryBroling);
            var cherry = sourceBro as CherryBroling;
            if (cherry != null)
            {
                rocketGrenade = cherry.rocketGrenade;
            }
        }

        public override void PressSpecial()
        {
            if (!owner.hasBeenCoverInAcid)
            {
                if (owner.SpecialAmmo > 0)
                {
                    if (owner.health > 0)
                    {
                        if (hero.InvulnerableTime > 0f)
                        {
                            hero.InvulnerableTime = 0f;
                            owner.invulnerable = false;
                        }
                        hero.DoingMelee = false;
                        UseSpecial();
                    }
                }
                else
                {
                    HeroController.FlashSpecialAmmo(PlayerNum);
                }
            }
        }

        public override void AnimateSpecial()
        {
            hero.UsingSpecial = false;
        }

        public override void UseSpecial()
        {
            if (owner.SpecialAmmo > 0)
            {
                Sound.GetInstance().PlaySoundEffectAt(throwSounds, 0.4f, owner.transform.position, 1f + owner.pitchShiftAmount);
                owner.SpecialAmmo--;
                HeroController.SetSpecialAmmo(PlayerNum, owner.SpecialAmmo);
                if ((hero.Ducking || owner.down) && owner.IsOnGround())
                {
                    if (owner.IsMine)
                    {
                        ProjectileController.SpawnProjectileOverNetwork(rocketGrenade, owner, X + Direction * spawnOffsetX, Y + spawnOffsetY, Direction * duckingSpeedX, 0f, false, PlayerNum, false, false, 0f);
                        owner.xIBlast -= Direction * duckingRecoilX;
                    }
                }
                else
                {
                    var cherry = owner as CherryBroling;
                    if (cherry != null)
                    {
                        cherry.SetFieldValue("somersaulting", true);
                        cherry.SetFieldValue("somersaultFrame", 0);
                    }
                    else
                    {
                        somersaulting = true;
                        somersaultFrame = 0;
                    }
                    owner.actionState = ActionState.Jumping;
                    hero.AnimateJumping();
                    if (owner.IsMine)
                    {
                        Vector3 dir;
                        if (owner.left)
                        {
                            dir = fireRightDirection;
                        }
                        else if (owner.right)
                        {
                            dir = fireLeftDirection;
                        }
                        else
                        {
                            dir = fireDownDirection;
                        }
                        owner.SetFieldValue("currentFireDirection", dir);
                        owner.yI -= dir.y * 0.5f;
                        owner.xIBlast -= dir.x * 0.1f;
                        ProjectileController.SpawnProjectileOverNetwork(rocketGrenade, owner, X, Y + 4f, dir.x * 1.2f, dir.y * 1.2f, false, PlayerNum, false, false, 0f);
                    }
                }
            }
            else
            {
                HeroController.FlashSpecialAmmo(PlayerNum);
                hero.ActivateGun();
            }
        }

        public override bool HandleCalculateMovement(ref float xI, ref float yI)
        {
            if (somersaulting && (owner.actionState != ActionState.Jumping || hero.WallDrag))
            {
                somersaulting = false;
                somersaultFrame = 0;
            }
            return true;
        }

        public override bool HandleAnimateActualJumpingFrames()
        {
            if (!somersaulting)
            {
                return true;
            }
            hero.DeactivateGun();
            hero.Sprite.SetLowerLeftPixel((float)(somersaultFrame * hero.SpritePixelWidth), (float)(8 * hero.SpritePixelHeight));
            somersaultFrame++;
            if (somersaultFrame > 10)
            {
                somersaulting = false;
            }
            hero.FrameRate = 0.04f;
            return false;
        }

        public override void HandleAfterLand()
        {
            somersaulting = false;
            somersaultFrame = 0;
        }

        public override bool HandleKnock(DamageType damageType, float xI, float yI, bool forceTumble)
        {
            owner.xI = Mathf.Clamp(owner.xI + xI / 2f, -200f, 200f);
            bool dashing = owner.GetFieldValue<bool>("dashing");
            owner.xIBlast = Mathf.Clamp(owner.xIBlast + xI / (2 + (dashing ? 1 : 0)), -200f, 200f);
            owner.yI = Mathf.Clamp(owner.yI + yI, -20000f, 400f);
            return false;
        }

        public override bool HandleWallDrag(bool value)
        {
            if (value)
            {
                somersaulting = false;
                somersaultFrame = 0;
            }
            return true;
        }
    }
}
