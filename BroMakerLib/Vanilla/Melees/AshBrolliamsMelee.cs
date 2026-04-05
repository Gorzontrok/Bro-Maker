using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using Newtonsoft.Json;
using RocketLib.Extensions;
using UnityEngine;

namespace BroMakerLib.Vanilla.Melees
{
    [MeleePreset("AshBrolliams")]
    public class AshBrolliamsMelee : MeleeAbility
    {
        [JsonIgnore]
        private AudioClip chainsawStart;
        [JsonIgnore]
        private AudioClip chainsawSpin;
        [JsonIgnore]
        private AudioClip chainsawWindDown;
        [JsonIgnore]
        private AudioClip[] alternateMeleeHitSounds2;

        [JsonIgnore]
        private AudioSource chainsawAudio;
        [JsonIgnore]
        private int initialDirection;
        [JsonIgnore]
        private int chainSawMeleeLoopCounter;
        [JsonIgnore]
        private float lastChainSawMeleeDamageTimeStamp;
        [JsonIgnore]
        private Unit chainSawMeleedUnit;

        public override void Initialize(TestVanDammeAnim owner)
        {
            base.Initialize(owner);

            var ashBrolliams = owner as AshBrolliams;
            if (ashBrolliams == null)
            {
                var prefab = HeroController.GetHeroPrefab(HeroType.AshBrolliams);
                ashBrolliams = prefab as AshBrolliams;
            }
            if (ashBrolliams != null)
            {
                chainsawStart = ashBrolliams.chainsawStart;
                chainsawSpin = ashBrolliams.chainsawSpin;
                chainsawWindDown = ashBrolliams.chainsawWindDown;
                alternateMeleeHitSounds2 = ashBrolliams.soundHolder.alternateMeleeHitSound;
            }

            chainsawAudio = owner.GetComponent<AudioSource>();
            if (chainsawAudio == null)
            {
                chainsawAudio = owner.gameObject.AddComponent<AudioSource>();
                chainsawAudio.rolloffMode = AudioRolloffMode.Linear;
                chainsawAudio.dopplerLevel = 0.1f;
                chainsawAudio.minDistance = 500f;
                chainsawAudio.volume = 0.4f;
            }
        }

        public override void StartMelee()
        {
            if (owner.GetFieldValue<bool>("onRampage"))
            {
                return;
            }
            if (!hero.DoingMelee)
            {
                owner.frame = 0;
                owner.counter = -0.05f;
                AnimateMelee();
            }
            else
            {
                hero.MeleeFollowUp = true;
            }
            initialDirection = owner.Direction;
            hero.StartMeleeCommon();
            owner.SetFieldValue("cancelMeleeOnChangeDirection", true);
            if (!chainsawAudio.isPlaying || chainsawAudio.clip != chainsawSpin)
            {
                chainsawAudio.loop = true;
                chainsawAudio.clip = chainsawSpin;
                chainsawAudio.Play();
                chainsawAudio.pitch = 0.8f;
            }
            chainSawMeleeLoopCounter = 0;
            owner.counter = Mathf.Clamp(owner.counter - 0.05f, -0.05f, 0f);
            AnimateMelee();
        }

        public override void AnimateMelee()
        {
            chainsawAudio.pitch = Mathf.Lerp(chainsawAudio.pitch, 0.7f + Random.value * 0.5f, Time.deltaTime * 8f);
            hero.AnimateMeleeCommon();
            if (owner.frame > 1)
            {
                hero.KickDoors(25f);
            }
            if (hero.JumpingMelee)
            {
                if (owner.frame == 5)
                {
                    owner.counter -= 0.166f;
                }
            }
            else if (owner.frame >= 4)
            {
                if (chainSawMeleeLoopCounter > 4)
                {
                    if (owner.frame < 7)
                    {
                        owner.frame = 7;
                    }
                }
                else
                {
                    if (owner.frame > 5)
                    {
                        owner.frame = 4;
                        chainSawMeleeLoopCounter++;
                    }
                    if (owner.frame == 4)
                    {
                        owner.counter -= 0.0334f;
                    }
                    else if (owner.frame == 5)
                    {
                        owner.counter -= 0.0334f;
                    }
                }
            }
            int num = 9;
            if (hero.JumpingMelee)
            {
                num = 10;
            }
            int num2 = 24 + owner.frame;
            hero.Sprite.SetLowerLeftPixel((float)(num2 * hero.SpritePixelWidth), (float)(num * hero.SpritePixelHeight));
            if (owner.frame == 5)
            {
                PerformChainsawMelee();
            }
            if (hero.JumpingMelee)
            {
                if (!owner.IsOnGround())
                {
                    if (owner.GetFieldValue<bool>("highFive") && owner.frame > 5)
                    {
                        owner.frame = 5;
                    }
                }
                else
                {
                    hero.CancelMelee();
                }
            }
            if (owner.frame > 7)
            {
                owner.frame = 7;
                hero.CancelMelee();
            }
        }

        public override void RunMeleeMovement()
        {
            owner.ForceFaceDirection(initialDirection);
            if (hero.JumpingMelee)
            {
                owner.CallMethod("ApplyFallingGravity");
                if (owner.yI < owner.maxFallSpeed)
                {
                    owner.yI = owner.maxFallSpeed;
                }
                if (owner.frame >= 4 && !hero.MeleeHasHit)
                {
                    PerformJumpingChainsawMelee();
                }
                if (owner.IsOnGround() && owner.frame < 5)
                {
                    owner.frame = 5;
                    AnimateMelee();
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
                        float num = hero.MeleeChosenUnit.X - (float)owner.Direction * 12f - owner.X;
                        owner.xI = num / 0.1f;
                        owner.xI = Mathf.Clamp(owner.xI, -owner.speed * 1.7f, owner.speed * 1.7f);
                    }
                    else
                    {
                        owner.xI = 0f;
                        owner.yI = 0f;
                    }
                }
                else if (owner.frame <= 7)
                {
                    owner.xI = 0f;
                }
                else
                {
                    owner.CallMethod("ApplyFallingGravity");
                }
            }
            else
            {
                owner.xI = 0f;
                if (owner.Y > owner.groundHeight + 1f)
                {
                    hero.CancelMelee();
                }
            }
        }

        public override void CancelMelee()
        {
            StopChainsawAudio();
            if (chainSawMeleedUnit != null && chainSawMeleedUnit.canDisembowel)
            {
                chainSawMeleedUnit.GibNow(DamageType.ChainsawImpale, (float)(owner.Direction * 100), 100f);
            }
        }

        private void StopChainsawAudio()
        {
            if (chainsawAudio.isPlaying && chainsawAudio.clip == chainsawSpin)
            {
                chainsawAudio.loop = false;
                chainsawAudio.clip = chainsawWindDown;
                chainsawAudio.Play();
            }
        }

        private void PerformChainsawMelee()
        {
            float num = 22f;
            float num2 = 8f;
            chainSawMeleedUnit = Map.GetNearestEnemyUnit(PlayerNum, (int)num, 16, X + (float)owner.Direction * num2, Y, true, owner.Direction, owner);
            if (chainSawMeleedUnit == null)
            {
                chainSawMeleedUnit = Map.GetNearestEnemyUnit(PlayerNum, (int)num, 16, X + (float)owner.Direction * num2, Y, true, -owner.Direction, owner);
            }
            if (lastChainSawMeleeDamageTimeStamp + 0.1f > Time.time)
            {
                return;
            }
            lastChainSawMeleeDamageTimeStamp = Time.time;
            if (chainSawMeleedUnit != null)
            {
                if (initialDirection > 0 && chainSawMeleedUnit.X < X + 8f)
                {
                    chainSawMeleedUnit.X = X + 8f;
                    chainSawMeleedUnit.SetPosition();
                }
                if (initialDirection < 0 && chainSawMeleedUnit.X > X - 8f)
                {
                    chainSawMeleedUnit.X = X - 8f;
                    chainSawMeleedUnit.SetPosition();
                }
                EffectsController.CreateBloodParticles(chainSawMeleedUnit.bloodColor, chainSawMeleedUnit.X, chainSawMeleedUnit.Y + 8f, 6, 8f, 8f, 60f, (float)(((NetworkedUnit)owner).DirectionSynced * 5), 100f + Random.value * 50f);
            }
            bool flag;
            Map.DamageDoodads(3, DamageType.Knifed, X + (float)(owner.Direction * 6), Y, 0f, 0f, 6f, PlayerNum, out flag, null);
            if (chainSawMeleedUnit != null)
            {
                chainSawMeleedUnit.Damage(4, DamageType.ChainsawImpale, 0f, 0f, owner.Direction, owner, X, Y);
                Map.PanicUnits(X, Y, 80f, 24f, 2f, true, false);
            }
            hero.TryMeleeTerrain(8, 2);
        }

        private void PerformJumpingChainsawMelee()
        {
            float num = 12f;
            Vector3 vector = new Vector3(X + (float)owner.Direction * num, Y - 4f, 0f);
            bool flag;
            Map.DamageDoodads(3, DamageType.Melee, vector.x, vector.y, 0f, 0f, 6f, PlayerNum, out flag, null);
            if (Map.HitClosestUnit(owner, PlayerNum, 4, DamageType.Melee, num, num * 2f, vector.x, vector.y, owner.transform.localScale.x * 250f, 250f, true, false, owner.IsMine, false, true))
            {
                sound.PlaySoundEffectAt(alternateMeleeHitSounds2, 0.5f, owner.transform.position, 1f, true, false, false, 0f);
                hero.MeleeHasHit = true;
            }
            hero.MeleeChosenUnit = null;
            if (!hero.MeleeHasHit && hero.TryMeleeTerrain(0, 2))
            {
                hero.MeleeHasHit = true;
            }
        }
    }
}
