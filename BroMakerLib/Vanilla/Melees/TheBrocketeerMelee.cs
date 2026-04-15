using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using BroMakerLib.Extensions;
using RocketLib.Extensions;
using UnityEngine;

namespace BroMakerLib.Vanilla.Melees
{
    /// <summary>TheBrocketeer's jetpack punch melee.</summary>
    [MeleePreset("Brocketeer")]
    public class TheBrocketeerMelee : MeleeAbility
    {
        protected override HeroType SourceBroType => HeroType.TheBrocketeer;

        /// <summary>Horizontal blast force applied each frame while the jetpack punch is in flight.</summary>
        public float jetpackThrustForce = 200f;
        /// <summary>Force used when kicking doors during the jetpack punch.</summary>
        public float doorKickForce = 50f;

        public TheBrocketeerMelee()
        {
            meleeType = BroBase.MeleeType.JetpackPunch;
            startType = MeleeStartType.Custom;
            animationRow = 9;
            animationColumn = 25;
            hitFrame = 3;
            endFrame = 7;
        }

        public override void StartMelee()
        {
            bool canStart = !hero.DoingMelee || owner.frame > 4;
            if (canStart)
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
            int col = animationColumn + Mathf.Clamp(owner.frame, 0, 8);
            int row = animationRow;
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
            hero.Sprite.SetLowerLeftPixel((float)(col * hero.SpritePixelWidth), (float)(row * hero.SpritePixelHeight));
            if (owner.frame == hitFrame && !hero.MeleeHasHit)
            {
                PerformJetpackPunchAttack(true, true);
            }
            if (owner.frame >= 4 && owner.frame <= 5 && !hero.MeleeHasHit)
            {
                PerformJetpackPunchAttack(true, true);
            }
            if (owner.frame >= endFrame)
            {
                owner.frame = 0;
                hero.CancelMelee();
            }
        }

        public override void RunMeleeMovement()
        {
            hero.ApplyFallingGravity();
            if (owner.frame > 1)
            {
                if (owner.frame < 5 && !hero.MeleeHasHit)
                {
                    float jetPackFlameCounter = owner.GetFieldValue<float>("jetPackFlameCounter");
                    jetPackFlameCounter += hero.DeltaTime;
                    if (jetPackFlameCounter > 0.0225f)
                    {
                        jetPackFlameCounter -= 0.025f;
                        owner.CallMethod("CreateJetpackFlamesHorizontal",
                            new Vector3(-owner.transform.localScale.x * 150f, 0f, 0f));
                    }
                    owner.SetFieldValue("jetPackFlameCounter", jetPackFlameCounter);
                    if (owner.transform.localScale.x > 0f)
                        owner.xIBlast = jetpackThrustForce;
                    else
                        owner.xIBlast = -jetpackThrustForce;
                }
                else if (owner.frame < 7)
                {
                    owner.xIBlast *= 1f - hero.DeltaTime * 14f;
                }
            }
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
                    Unit meleeChosenUnit = hero.MeleeChosenUnit;
                    if (meleeChosenUnit != null)
                    {
                        float offset = 8f;
                        float num = meleeChosenUnit.X - (float)Direction * offset - X;
                        if (!owner.GetFieldValue<bool>("isInQuicksand"))
                        {
                            owner.xI = num / 0.1f;
                            owner.xI = Mathf.Clamp(owner.xI, -owner.speed * 1.7f, owner.speed * 1.7f);
                        }
                    }
                    else
                    {
                        if (!owner.GetFieldValue<bool>("isInQuicksand"))
                        {
                            owner.xI = owner.speed * (float)Direction;
                        }
                        owner.yI = 0f;
                    }
                }
                else if (owner.frame <= 7)
                {
                    owner.xI = 0f;
                }
            }
            else if (Y > owner.groundHeight + 1f)
            {
                hero.CancelMelee();
            }
        }

        private void PerformJetpackPunchAttack(bool shouldTryHitTerrain, bool playMissSound)
        {
            float num = 6f;
            Vector3 vector = new Vector3(X + (float)Direction * (num + 7f), Y + 8f, 0f);
            bool flag;
            Map.DamageDoodads(3, DamageType.Melee, vector.x, vector.y, 0f, 0f, 6f, PlayerNum, out flag, null);
            hero.KickDoors(doorKickForce);
            if (Map.HitClosestUnit(owner, PlayerNum, damage, DamageType.Melee, num + 13f, num * 2f,
                vector.x, vector.y, owner.transform.localScale.x * knockbackX, knockbackY, true, false, owner.IsMine, false, true))
            {
                sound.PlaySoundEffectAt(alternateMeleeHitSounds, 0.5f, owner.transform.position, 1f, true, false, false, 0f);
                hero.MeleeHasHit = true;
                EffectsController.CreateProjectilePopWhiteEffect(X + owner.width * owner.transform.localScale.x, Y + owner.height + 8f);
            }
            else
            {
                if (playMissSound && !hero.HasPlayedMissSound)
                {
                    sound.PlaySoundEffectAt(missSounds, 0.15f, owner.transform.position, 1f, true, false, false, 0f);
                }
                hero.HasPlayedMissSound = true;
            }
            hero.MeleeChosenUnit = null;
            if (!hero.MeleeHasHit && shouldTryHitTerrain && HandleTryMeleeTerrain(0, terrainDamage))
            {
                hero.MeleeHasHit = true;
            }
        }
    }
}
