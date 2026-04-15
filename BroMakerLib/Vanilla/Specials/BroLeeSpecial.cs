using System.Collections.Generic;
using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using BroMakerLib.Extensions;
using Newtonsoft.Json;
using RocketLib.Extensions;
using Rogueforce;
using UnityEngine;

namespace BroMakerLib.Vanilla.Specials
{
    /// <summary>Bro Lee's multi-directional sword combo.</summary>
    [SpecialPreset("BroLee")]
    public class BroLeeSpecial : SpecialAbility
    {
        protected override HeroType SourceBroType => HeroType.BroLee;

        protected override void CacheSoundsFromPrefab()
        {
            base.CacheSoundsFromPrefab();
            var sourceBro = HeroController.GetHeroPrefab(SourceBroType) as BroLee;
            if (sourceBro == null) return;
            if (special4Sounds == null) special4Sounds = sourceBro.soundHolder.special4Sounds.CloneArray();
            if (special2Sounds == null) special2Sounds = sourceBro.soundHolder.special2Sounds.CloneArray();
            if (attack2Sounds == null) attack2Sounds = sourceBro.soundHolder.attack2Sounds.CloneArray();
            if (attack3Sounds == null) attack3Sounds = sourceBro.soundHolder.attack3Sounds.CloneArray();
        }
        /// <summary>Number of combo strikes before the special ends.</summary>
        public int maxHits = 15;
        public int enemySwordDamage = 5;
        public int groundSwordDamage = 1;
        /// <summary>Volume of the special activation sound.</summary>
        public float specialSoundVolume = 0.7f;
        /// <summary>Volume of the special intro vocal sound.</summary>
        public float special4SoundVolume = 0.5f;
        /// <summary>Volume of the per-swing sword sounds.</summary>
        public float attackSoundVolume = 0.6f;
        /// <summary>Intro vocal sound played when the special activates.</summary>
        public AudioClip[] special4Sounds;
        /// <summary>Slice impact sound played on a successful hit.</summary>
        public AudioClip[] special2Sounds;
        /// <summary>Wall-hit sound played when the sword strikes terrain.</summary>
        public AudioClip[] attack2Sounds;
        /// <summary>Bruce Lee vocal played when launching each attack.</summary>
        public AudioClip[] attack3Sounds;
        /// <summary>Animation frame index at which the forward strike deals damage.</summary>
        public int attackForwardsStrikeFrame = 3;
        /// <summary>Animation frame index at which the upward strike deals damage.</summary>
        public int attackUpwardsStrikeFrame = 2;
        /// <summary>Animation frame index at which the downward strike deals damage.</summary>
        public int attackDownwardsStrikeFrame = 3;
        public int attackForwardsRow = 8;
        public int attackUpwardsRow = 10;
        public int attackDownwardsRow = 11;
        /// <summary>Radius used when searching for the nearest enemy to home toward during a special combo.</summary>
        public float targetRange = 64f;

        [JsonIgnore]
        private int specialHitsLeft;
        [JsonIgnore]
        private Unit specialTargetUnit;
        [JsonIgnore]
        private bool attackForwards;
        [JsonIgnore]
        private bool attackUpwards;
        [JsonIgnore]
        private bool attackDownwards;
        [JsonIgnore]
        private bool hasAttackedForwards;
        [JsonIgnore]
        private bool hasAttackedUpwards;
        [JsonIgnore]
        private bool hasAttackedDownwards;
        [JsonIgnore]
        private int attackFrames;
        [JsonIgnore]
        private int attackSpriteRow;
        [JsonIgnore]
        private bool hasHitThisAttack;
        [JsonIgnore]
        private bool hasHitWithWall;
        [JsonIgnore]
        private bool hasHitWithSlice;
        [JsonIgnore]
        private bool attackHasHit;
        [JsonIgnore]
        private bool startNewAttack;
        [JsonIgnore]
        private float postAttackHitPauseTime;
        [JsonIgnore]
        private bool ignoreTimescale;
        [JsonIgnore]
        private float faderTrailDelay;
        [JsonIgnore]
        private float lastBroceLeeSoundTime;
        [JsonIgnore]
        private float lastAttackingTime;
        [JsonIgnore]
        private List<Unit> alreadyHit = new List<Unit>();
        [JsonIgnore]
        private float xIBeforeAddSpeed;
        [JsonIgnore]
        private Shrapnel shrapnelSpark;
        [JsonIgnore]
        private FlickerFader hitPuff;

        public override void Initialize(TestVanDammeAnim owner)
        {
            base.Initialize(owner);
            var broLee = owner as BroLee;
            if (broLee == null)
            {
                var prefab = HeroController.GetHeroPrefab(HeroType.BroLee) as BroLee;
                broLee = prefab;
            }
            if (broLee != null)
            {
                shrapnelSpark = broLee.shrapnelSpark;
                hitPuff = broLee.hitPuff;
                if (owner.faderSpritePrefab == null)
                    owner.faderSpritePrefab = broLee.faderSpritePrefab;
            }
        }

        public override void PressSpecial()
        {
            if (!owner.hasBeenCoverInAcid && owner.health > 0 && owner.SpecialAmmo > 0 && !hero.UsingSpecial)
            {
                if (owner.actionState == ActionState.ClimbingLadder)
                {
                    owner.actionState = ActionState.Jumping;
                }
                hero.UsingSpecial = true;
                owner.frame = 0;
                hero.PressSpecialFacingDirection = (int)owner.transform.localScale.x;
                specialHitsLeft = maxHits;
                owner.SpecialAmmo--;
                UseSpecial();
                Sound.GetInstance().PlaySoundEffectAt(special4Sounds, special4SoundVolume, owner.transform.position, 1f + owner.pitchShiftAmount, true, false, false, 0f);
                Sound.GetInstance().PlaySoundEffectAt(specialAttackSounds, specialSoundVolume,
                    owner.transform.position, 1f, true, false, false, 0f);
            }
            else
            {
                HeroController.FlashSpecialAmmo(PlayerNum);
            }
        }

        public override void UseSpecial()
        {
            if (specialHitsLeft > 0)
            {
                if (!owner.up && !owner.down && !owner.left && !owner.right)
                {
                    if (owner.transform.localScale.x > 0f)
                        owner.right = true;
                    else
                        owner.left = true;
                }
                List<Unit> units;
                if (owner.up)
                    units = Map.GetUnitsInRange((int)targetRange, X, Y + targetRange, false);
                else if (owner.down)
                    units = Map.GetUnitsInRange((int)targetRange, X, Y - targetRange, false);
                else if (owner.right)
                    units = Map.GetUnitsInRange((int)targetRange, X + targetRange, Y, false);
                else if (owner.left)
                    units = Map.GetUnitsInRange((int)targetRange, X - targetRange, Y, false);
                else
                    units = Map.GetUnitsInRange((int)targetRange, X, Y, false);

                specialTargetUnit = null;
                float closestDist = float.MaxValue;
                for (int i = 0; i < units.Count; i++)
                {
                    if (units[i].playerNum < 0)
                    {
                        float dist = Vector2.Distance(new Vector2(X, Y), new Vector2(units[i].X, units[i].Y));
                        if (dist < closestDist)
                        {
                            specialTargetUnit = units[i];
                            closestDist = dist;
                        }
                    }
                }
                attackForwards = false;
                attackUpwards = false;
                attackDownwards = false;
                hasAttackedForwards = false;
                hasAttackedUpwards = false;
                hasAttackedDownwards = false;
                StartAttack();
                specialHitsLeft--;
            }
            else
            {
                hero.UsingSpecial = false;
            }
        }

        public override void HandleAfterIncreaseFrame()
        {
            if (attackUpwards || attackDownwards || attackForwards)
            {
                attackFrames++;
            }
        }

        public override bool HandleChangeFrame()
        {
            if (owner.health <= 0) return true;
            if (attackUpwards)
            {
                AnimateAttackUpwards();
                return false;
            }
            if (attackDownwards)
            {
                AnimateAttackDownwards();
                return false;
            }
            if (attackForwards)
            {
                AnimateAttackForwards();
                return false;
            }
            return true;
        }

        public override void AnimateSpecial()
        {
            if (attackDownwards)
                AnimateAttackDownwards();
            else if (attackUpwards)
                AnimateAttackUpwards();
            else if (attackForwards)
                AnimateAttackForwards();
        }

        private void FireFlashAvatar()
        {
            owner.SetFieldValue("avatarGunFireTime", hero.UsingSpecial ? 0.5f : 0.1f);
            HeroController.SetAvatarFireFrame(PlayerNum, Random.Range(5, 8));
        }

        private void ClearCurrentAttackVariables()
        {
            alreadyHit.Clear();
            hasHitWithSlice = false;
            attackHasHit = false;
            hasHitWithWall = false;
        }

        private void StartAttack()
        {
            startNewAttack = false;
            hasHitWithWall = false;
            hasHitWithSlice = false;
            if (hero.UsingSpecial)
            {
                ignoreTimescale = true;
            }
            owner.SetFieldValue("airdashTime", 0f);
            if (Y < owner.groundHeight + 1f)
            {
                hero.StopAirDashing();
            }

            if (attackForwards || attackDownwards || attackUpwards)
            {
                startNewAttack = true;
            }
            else if (owner.up && !hasAttackedUpwards)
            {
                if (owner.actionState == ActionState.ClimbingLadder)
                    owner.actionState = ActionState.Jumping;
                FireFlashAvatar();
                MakeBroceLeeSound();
                StopAttack();
                if (owner.yI > 50f) owner.yI = 50f;
                hero.JumpTime = 0f;
                hasAttackedUpwards = true;
                attackFrames = 0;
                attackUpwards = true;
                hero.ChangeFrame();
                ClearCurrentAttackVariables();
                groundSwordDamage = 5;
                hero.AirdashDirection = DirectionEnum.Up;
            }
            else if (owner.down && !hasAttackedDownwards)
            {
                owner.actionState = ActionState.Jumping;
                StopAttack();
                FireFlashAvatar();
                if (hero.UsingSpecial)
                {
                    owner.yI = -100f;
                    owner.xI = 0f;
                }
                else
                {
                    owner.yI = 150f;
                    owner.xI = owner.transform.localScale.x * 80f;
                }
                MakeBroceLeeSound();
                hasAttackedDownwards = true;
                attackFrames = 0;
                attackDownwards = true;
                hero.JumpTime = 0f;
                hero.ChangeFrame();
                ClearCurrentAttackVariables();
                groundSwordDamage = 5;
                hero.AirdashDirection = DirectionEnum.Down;
            }
            else if (owner.left && !hasAttackedForwards)
            {
                FireFlashAvatar();
                attackSpriteRow = (attackSpriteRow + 1) % 2;
                if (owner.actionState == ActionState.ClimbingLadder)
                    owner.actionState = ActionState.Jumping;
                if ((attackForwards || attackUpwards || attackDownwards) && !hasHitThisAttack)
                {
                    StopAttack();
                    owner.xIAttackExtra = 20f;
                }
                else if ((attackForwards || attackUpwards || attackDownwards) && hasHitThisAttack)
                {
                    StopAttack();
                    owner.xIAttackExtra = -300f;
                    MakeBroceLeeSound();
                }
                else
                {
                    StopAttack();
                    owner.xIAttackExtra = -200f;
                    MakeBroceLeeSound();
                }
                postAttackHitPauseTime = 0f;
                hasAttackedForwards = true;
                attackFrames = 0;
                owner.yI = 0f;
                attackForwards = true;
                hero.JumpTime = 0f;
                hero.ChangeFrame();
                CreateFaderTrailInstance();
                ClearCurrentAttackVariables();
                groundSwordDamage = 5;
                hero.AirdashDirection = DirectionEnum.Left;
            }
            else if (owner.right && !hasAttackedForwards)
            {
                attackSpriteRow = (attackSpriteRow + 1) % 2;
                FireFlashAvatar();
                if (owner.actionState == ActionState.ClimbingLadder)
                    owner.actionState = ActionState.Jumping;
                if ((attackForwards || attackUpwards || attackDownwards) && !hasHitThisAttack)
                {
                    StopAttack();
                    owner.xIAttackExtra = -20f;
                }
                else if ((attackForwards || attackUpwards || attackDownwards) && hasHitThisAttack)
                {
                    StopAttack();
                    owner.xIAttackExtra = 300f;
                    MakeBroceLeeSound();
                }
                else
                {
                    StopAttack();
                    owner.xIAttackExtra = 200f;
                    MakeBroceLeeSound();
                }
                hasAttackedForwards = true;
                postAttackHitPauseTime = 0f;
                attackFrames = 0;
                owner.yI = 0f;
                attackForwards = true;
                hero.JumpTime = 0f;
                hero.ChangeFrame();
                CreateFaderTrailInstance();
                ClearCurrentAttackVariables();
                groundSwordDamage = 5;
                hero.AirdashDirection = DirectionEnum.Right;
            }
        }

        private void StopAttack()
        {
            hasHitThisAttack = false;
            bool wasUsing = hero.UsingSpecial;
            attackForwards = false;
            attackUpwards = false;
            attackDownwards = false;
            attackFrames = 0;
            owner.frame = 0;
            owner.xIAttackExtra = 0f;
            hero.UsingSpecial = false;
            if (Y > owner.groundHeight + 1f)
                owner.actionState = ActionState.Jumping;
            else if (owner.right || owner.left)
                owner.actionState = ActionState.Running;
            else
                owner.actionState = ActionState.Idle;
            if (startNewAttack)
            {
                startNewAttack = false;
                StartAttack();
            }
            if (Y < owner.groundHeight + 1f)
            {
                hero.StopAirDashing();
                hasAttackedDownwards = false;
                hasAttackedUpwards = false;
                hasAttackedForwards = false;
            }
            alreadyHit.Clear();
            hero.UsingSpecial = wasUsing;
        }

        private void DeflectProjectiles()
        {
            if (Map.DeflectProjectiles(owner, PlayerNum, 10f, X + Direction * 6f, Y + 6f, Direction * 200f, true))
            {
                hasHitWithWall = true;
                attackHasHit = true;
            }
        }

        private void PlaySliceSound()
        {
            sound.PlaySoundEffectAt(special2Sounds, 0.7f, owner.transform.position, 1f, true, false, false, 0f);
        }

        private void OnHitForward()
        {
            if (!hasHitWithSlice) PlaySliceSound();
            hasHitWithSlice = true;
            attackHasHit = true;
            hasAttackedDownwards = false;
            hasAttackedUpwards = false;
            hasAttackedForwards = false;
            hasHitThisAttack = true;
            TimeBump();
            owner.xIAttackExtra = 0f;
            postAttackHitPauseTime = 0.2f;
            if (hero.UsingSpecial)
                HeroController.SetAvatarAngry(PlayerNum, owner.usePrimaryAvatar);
            owner.xI = 0f;
            owner.yI = 0f;
            for (int i = 0; i < alreadyHit.Count; i++)
            {
                if (alreadyHit[i] != null)
                    alreadyHit[i].BackSomersault(false);
            }
        }

        private void OnHitUpward()
        {
            if (!hasHitWithSlice) PlaySliceSound();
            hasHitWithSlice = true;
            attackHasHit = true;
            hasAttackedDownwards = false;
            hasAttackedForwards = false;
            hasHitThisAttack = true;
            TimeBump();
            if (hero.UsingSpecial)
                HeroController.SetAvatarAngry(PlayerNum, owner.usePrimaryAvatar);
        }

        private void OnHitDownward()
        {
            if (!hasHitWithSlice) PlaySliceSound();
            hasHitWithSlice = true;
            attackHasHit = true;
            hasAttackedForwards = false;
            hasAttackedUpwards = false;
            hasHitThisAttack = true;
            owner.xIAttackExtra = 0f;
            TimeBump();
            postAttackHitPauseTime = 0.25f;
            owner.xI = 0f;
            owner.yI = 0f;
            if (hero.UsingSpecial)
                HeroController.SetAvatarAngry(PlayerNum, owner.usePrimaryAvatar);
        }

        private void TimeBump()
        {
            if (hero.UsingSpecial)
            {
                ignoreTimescale = false;
                TimeController.StopTime(2f, 0.01f, 0f, false, false, true);
            }
            else
            {
                TimeController.StopTime(0.025f, 0.1f, 0f, false, false, false);
            }
        }

        private void AnimateAttackForwards()
        {
            hero.DeactivateGun();
            hero.FrameRate = 0.045f;
            if (attackFrames < attackForwardsStrikeFrame + 1)
            {
                CreateFaderTrailInstance();
            }
            if (attackFrames == 8)
            {
                if (!hasHitThisAttack)
                    hero.UsingSpecial = false;
                if (hero.UsingSpecial)
                    UseSpecial();
                else if (startNewAttack)
                {
                    startNewAttack = false;
                    StartAttack();
                }
            }
            if (attackFrames == attackForwardsStrikeFrame)
            {
                FireWeaponGround(X + Direction * 9f, Y + 6f, new Vector3(Direction, 0f, 0f), 8f, Direction * 180f, 80f);
                Sound.GetInstance().PlaySoundEffectAt(attackSounds, attackSoundVolume, owner.transform.position, 1f, true, false, false, 0f);
            }
            if (attackFrames == attackForwardsStrikeFrame + 1)
            {
                owner.xIAttackExtra = 0f;
            }
            if (attackFrames >= 8)
            {
                StopAttack();
            }
            else
            {
                int col = (hero.UsingSpecial ? 0 : 24) + Mathf.Clamp(attackFrames, 0, 7);
                hero.Sprite.SetLowerLeftPixel(col * hero.SpritePixelWidth, (attackForwardsRow + attackSpriteRow) * hero.SpritePixelHeight);
            }
        }

        private void AnimateAttackUpwards()
        {
            hero.DeactivateGun();
            if (attackFrames < attackUpwardsStrikeFrame)
                hero.FrameRate = 0.0667f;
            else
                hero.FrameRate = 0.045f;

            if (attackFrames == attackUpwardsStrikeFrame)
            {
                owner.xI = Direction * 50f;
                owner.yI = 240f;
                Sound.GetInstance().PlaySoundEffectAt(attackSounds, attackSoundVolume, owner.transform.position, 1f, true, false, false, 0f);
            }
            if (attackFrames < attackUpwardsStrikeFrame + 2)
            {
                CreateFaderTrailInstance();
            }
            if (startNewAttack && attackFrames == attackUpwardsStrikeFrame + 1)
            {
                startNewAttack = false;
                StartAttack();
            }
            if (hasHitThisAttack && attackFrames == 6)
            {
                owner.xIAttackExtra = 0f;
                postAttackHitPauseTime = 0.25f;
                owner.xI = 0f;
                owner.yI = 0f;
            }
            if (attackFrames == 8 && hero.UsingSpecial)
            {
                if (!hasHitThisAttack)
                    hero.UsingSpecial = false;
                if (hero.UsingSpecial)
                    UseSpecial();
            }
            if (attackFrames >= 10 || (attackFrames == 8 && startNewAttack))
            {
                StopAttack();
                hero.ChangeFrame();
            }
            else
            {
                int col = (hero.UsingSpecial ? 0 : 24) + Mathf.Clamp(attackFrames, 0, 7);
                hero.Sprite.SetLowerLeftPixel(col * hero.SpritePixelWidth, attackUpwardsRow * hero.SpritePixelHeight);
            }
        }

        private void AnimateAttackDownwards()
        {
            hero.DeactivateGun();
            if (attackFrames < attackDownwardsStrikeFrame)
                hero.FrameRate = 0.0667f;
            else if (attackFrames <= 5)
                hero.FrameRate = 0.045f;
            else
                hero.FrameRate = 0.066f;

            if (attackFrames < attackDownwardsStrikeFrame + 2)
            {
                CreateFaderTrailInstance();
            }
            if (startNewAttack && attackFrames == attackDownwardsStrikeFrame + 1)
            {
                startNewAttack = false;
                StartAttack();
            }
            if (attackFrames == attackDownwardsStrikeFrame)
            {
                if (!hero.UsingSpecial || !hasHitThisAttack)
                    owner.yI = -250f;
                owner.xI = Direction * 60f;
                Sound.GetInstance().PlaySoundEffectAt(attackSounds, attackSoundVolume, owner.transform.position, 1f, true, false, false, 0f);
            }
            if (attackFrames >= 9 || (attackFrames == 6 && startNewAttack))
            {
                StopAttack();
                hero.ChangeFrame();
            }
            else
            {
                int col = 24 + Mathf.Clamp(attackFrames, 0, 7);
                hero.Sprite.SetLowerLeftPixel(col * hero.SpritePixelWidth, attackDownwardsRow * hero.SpritePixelHeight);
            }
            if (attackFrames == 7 && hero.UsingSpecial)
            {
                if (!hasHitThisAttack)
                    hero.UsingSpecial = false;
                if (hero.UsingSpecial)
                    UseSpecial();
            }
        }

        private void FireWeaponGround(float x, float y, Vector3 dir, float distance, float xSpeed, float ySpeed)
        {
            RaycastHit hit;
            if (Physics.Raycast(new Vector3(x, y, 0f), dir, out hit, distance, hero.GroundLayer))
            {
                if (!hasHitWithWall)
                {
                    SortOfFollow.Shake(0.15f);
                    EffectsController.CreateShrapnel(shrapnelSpark, hit.point.x + hit.normal.x * 3f, hit.point.y + hit.normal.y * 3f, 4f, 30f, 3f, hit.normal.x * 60f, hit.normal.y * 30f);
                    EffectsController.CreateEffect(hitPuff, hit.point.x + hit.normal.x * 3f, hit.point.y + hit.normal.y * 3f);
                }
                MapController.Damage_Networked(owner, hit.collider.gameObject, groundSwordDamage, DamageType.Blade,
                    owner.xI, 0f, hit.point.x, hit.point.y);
                hasHitWithWall = true;
                attackHasHit = true;
                sound.PlaySoundEffectAt(attack2Sounds, 0.3f, owner.transform.position, 1f, true, false, false, 0f);
            }
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
                fader.fadeM = 0.15f;
                fader.maxLife = 0.15f;
                fader.moveForwards = true;
            }
        }

        private void MakeBroceLeeSound()
        {
            if (Time.time - lastBroceLeeSoundTime > 0.3f || hero.UsingSpecial)
            {
                lastBroceLeeSoundTime = Time.time;
                Sound.GetInstance().PlaySoundEffectAt(attack3Sounds, attackSoundVolume,
                    owner.transform.position, 1f, true, false, false, 0f);
            }
        }

        public override bool HandleRunFiring()
        {
            owner.specialAttackXIBoost = 0f;
            owner.specialAttackYIBoost = 0f;
            if (!attackUpwards && !attackForwards && !attackDownwards)
            {
                return true;
            }
            if (hero.UsingSpecial)
            {
                MapController.DamageGround(owner, ValueOrchestrator.GetModifiedDamage(10, PlayerNum),
                    DamageType.Bullet, 8f, X, Y + 4f, null, false);
            }
            if (!attackHasHit && hero.UsingSpecial && specialTargetUnit != null)
            {
                float dx = specialTargetUnit.X - X;
                float dy = specialTargetUnit.Y - Y;
                float xBoost = 0f, yBoost = 0f;
                if (dx > 8f) xBoost = 128f;
                if (dx < -8f) xBoost = -128f;
                if (dy > 8f) yBoost = 128f;
                if (dy < -8f) yBoost = -128f;
                owner.specialAttackXIBoost = xBoost;
                owner.specialAttackYIBoost = yBoost;
            }
            if (!attackHasHit)
            {
                if ((attackForwards && attackFrames >= attackForwardsStrikeFrame - 1) ||
                    (attackUpwards && attackFrames >= attackUpwardsStrikeFrame - 1) ||
                    (attackDownwards && attackFrames >= attackDownwardsStrikeFrame - 1))
                {
                    DeflectProjectiles();
                }
            }
            if (attackForwards && attackFrames >= attackForwardsStrikeFrame && attackFrames <= 5)
            {
                lastAttackingTime = Time.time;
                bool flag;
                Map.DamageDoodads(3, DamageType.Knifed, X + owner.Direction * 4f, Y, 0f, 0f, 6f, PlayerNum, out flag, null);
                if (Map.HitUnits(owner, PlayerNum, enemySwordDamage, 1, DamageType.Blade, 13f,
                    X + Direction * 7f, Y + 7f, Direction * 420f, 160f, true, true, true, alreadyHit, false))
                {
                    OnHitForward();
                }
                if (!attackHasHit)
                {
                    DeflectProjectiles();
                    FireWeaponGround(X + Direction * 3f, Y + 6f, new Vector3(Direction, 0f, 0f), 9f, Direction * 180f, 80f);
                    FireWeaponGround(X + Direction * 3f, Y + 12f, new Vector3(Direction, 0f, 0f), 9f, Direction * 180f, 80f);
                }
            }
            if (attackUpwards && attackFrames >= attackUpwardsStrikeFrame && attackFrames <= 5)
            {
                lastAttackingTime = Time.time;
                bool flag;
                Map.DamageDoodads(3, DamageType.Knifed, X + owner.Direction * 4f, Y, 0f, 0f, 6f, PlayerNum, out flag, null);
                if (Map.HitUnits(owner, PlayerNum, enemySwordDamage, 1, DamageType.Blade, 13f,
                    X + Direction * 6f, Y + 12f, Direction * 80f, 1100f, true, true, true, alreadyHit, false))
                {
                    OnHitUpward();
                }
                if (!attackHasHit)
                {
                    DeflectProjectiles();
                    FireWeaponGround(X + Direction * 3f, Y + 6f, new Vector3(Direction * 0.5f, 1f, 0f), 12f, Direction * 80f, 280f);
                    FireWeaponGround(X + Direction * 3f, Y + 6f, new Vector3(Direction, 0.5f, 0f), 12f, Direction * 80f, 280f);
                }
            }
            if (attackDownwards && attackFrames >= attackDownwardsStrikeFrame && attackFrames <= 6)
            {
                lastAttackingTime = Time.time;
                bool flag;
                Map.DamageDoodads(3, DamageType.Knifed, X + owner.Direction * 4f, Y, 0f, 0f, 6f, PlayerNum, out flag, null);
                if (Map.HitUnits(owner, PlayerNum, enemySwordDamage, 1, DamageType.Blade, 13f,
                    X + Direction * 6f, Y + 4f, Direction * 120f, 100f, true, true, true, alreadyHit, false))
                {
                    OnHitDownward();
                }
                if (!attackHasHit)
                {
                    DeflectProjectiles();
                    FireWeaponGround(X + Direction * 3f, Y + 6f, new Vector3(Direction * 0.4f, -1f, 0f), 14f, Direction * 80f, -180f);
                    FireWeaponGround(X + Direction * 3f, Y + 6f, new Vector3(Direction * 0.8f, -0.2f, 0f), 12f, Direction * 80f, -180f);
                }
            }
            return false;
        }

        public override bool HandleApplyFallingGravity()
        {
            return postAttackHitPauseTime < 0f && !hero.UsingSpecial;
        }

        public override bool HandleJump(bool wallJump)
        {
            return !hero.UsingSpecial;
        }

        public override bool HandleStartFiring()
        {
            if (attackForwards || attackUpwards || attackDownwards)
            {
                startNewAttack = true;
                return false;
            }
            return true;
        }

        public override bool HandleStartMelee()
        {
            return !hero.UsingSpecial && !attackForwards && !attackUpwards && !attackDownwards;
        }

        private bool IsAttacking()
        {
            return (owner.fire && hero.GunFrame > 1) || Time.time - lastAttackingTime < 0.0445f ||
                ((attackDownwards || attackForwards || attackUpwards) && attackFrames > 1 && attackFrames < attackForwardsStrikeFrame + 2);
        }

        public override bool HandleDamage(int damage, DamageType damageType, float xI, float yI, int direction, MonoBehaviour damageSender, float hitX, float hitY)
        {
            if ((damageType == DamageType.Drill || damageType == DamageType.Melee || damageType == DamageType.Knifed) &&
                IsAttacking() &&
                !(Mathf.Sign(owner.transform.localScale.x) == Mathf.Sign(xI) && damageType != DamageType.Drill))
            {
                return false;
            }
            return true;
        }

        public override bool HandleLand()
        {
            if (hero.UsingSpecial)
            {
                return false;
            }
            if (attackDownwards)
            {
                if (!attackHasHit && attackFrames < 7)
                {
                    FireWeaponGround(X + Direction * 16.5f, Y + 16.5f, Vector3.down, 18f + Mathf.Abs(owner.yI * hero.DeltaTime), Direction * 80f, 100f);
                }
                if (!attackHasHit && attackFrames < 7)
                {
                    FireWeaponGround(X + Direction * 5.5f, Y + 16.5f, Vector3.down, 18f + Mathf.Abs(owner.yI * hero.DeltaTime), Direction * 80f, 100f);
                }
                attackDownwards = false;
                attackFrames = 0;
            }
            return true;
        }

        public override bool HandleConstrainToFloor()
        {
            return !hero.UsingSpecial;
        }

        public override bool HandleConstrainToWalls()
        {
            return !hero.UsingSpecial;
        }

        public override bool HandleConstrainToCeiling()
        {
            return !hero.UsingSpecial;
        }

        public override bool HandleHitCeiling()
        {
            if (hero.UsingSpecial)
            {
                return false;
            }
            return true;
        }

        public override void HandleAfterHitCeiling()
        {
            if (!hero.UsingSpecial && attackUpwards && !attackHasHit && attackFrames < 7)
            {
                FireWeaponGround(X + Direction * 16.5f, Y + 2f, Vector3.up, owner.headHeight + Mathf.Abs(owner.yI * hero.DeltaTime), Direction * 80f, 100f);
                FireWeaponGround(X + Direction * 4.5f, Y + 2f, Vector3.up, owner.headHeight + Mathf.Abs(owner.yI * hero.DeltaTime), Direction * 80f, 100f);
                attackUpwards = false;
                attackFrames = 0;
            }
        }

        public override void Update()
        {
            postAttackHitPauseTime -= hero.DeltaTime;
            if ((attackForwards || attackUpwards || attackDownwards) && owner.xIAttackExtra != 0f)
            {
                faderTrailDelay -= hero.DeltaTime / Time.timeScale;
                if (faderTrailDelay < 0f)
                {
                    CreateFaderTrailInstance();
                    faderTrailDelay = 0.034f;
                }
            }
        }

        public override bool HandleSetDeltaTime()
        {
            if (hero.UsingSpecial)
            {
                SortOfFollow.instance.ignoreTimescale = false;
                owner.SetFieldValue("lastT", hero.DeltaTime);
                if (!ignoreTimescale)
                {
                    SortOfFollow.instance.ignoreTimescale = true;
                    float dt = Mathf.Clamp(Time.deltaTime, 0f, 0.0334f);
                    owner.SetFieldValue("t", (dt / Time.timeScale + dt) / 2f);
                }
                else
                {
                    owner.SetFieldValue("t", Mathf.Clamp(Time.deltaTime, 0f, 0.0334f) / Time.timeScale);
                }
                return false;
            }
            return true;
        }

        public override bool HandleCalculateMovement(ref float xI, ref float yI)
        {
            xIBeforeAddSpeed = owner.xI;
            if ((attackDownwards || attackUpwards) || postAttackHitPauseTime >= 0f)
            {
                xI = owner.xI;
            }
            return true;
        }

        public override void HandleAfterAddSpeedLeft()
        {
            if (attackDownwards || attackUpwards || postAttackHitPauseTime >= 0f)
            {
                owner.xI = xIBeforeAddSpeed;
            }
            else if (attackForwards && attackFrames > 4 && owner.xI < -owner.speed * 0.5f)
            {
                owner.xI = -owner.speed * 0.5f;
            }
        }

        public override void HandleAfterAddSpeedRight()
        {
            if (attackDownwards || attackUpwards || postAttackHitPauseTime >= 0f)
            {
                owner.xI = xIBeforeAddSpeed;
            }
            else if (attackForwards && attackFrames > 4 && owner.xI > owner.speed * 0.5f)
            {
                owner.xI = owner.speed * 0.5f;
            }
        }

        public override bool HandleRunAvatarFiring()
        {
            if (owner.health <= 0) return true;
            float avatarGunFireTime = owner.GetFieldValue<float>("avatarGunFireTime");
            float avatarAngryTime = owner.GetFieldValue<float>("avatarAngryTime");
            if (avatarGunFireTime > 0f)
            {
                avatarGunFireTime -= hero.DeltaTime * (hero.UsingSpecial ? Time.timeScale : 1f);
                owner.SetFieldValue("avatarGunFireTime", avatarGunFireTime);
                if (avatarGunFireTime <= 0f)
                {
                    if (avatarAngryTime > 0f)
                        HeroController.SetAvatarAngry(PlayerNum, owner.usePrimaryAvatar);
                    else
                        HeroController.SetAvatarCalm(PlayerNum, owner.usePrimaryAvatar);
                }
            }
            if (owner.fire || hero.UsingSpecial)
            {
                if (!owner.wasFire && avatarGunFireTime <= 0f)
                {
                    HeroController.SetAvatarAngry(PlayerNum, owner.usePrimaryAvatar);
                }
                if (owner.fire)
                {
                    if (hero.GunFrame > 0 || attackFrames > 0)
                        owner.SetFieldValue("avatarAngryTime", 0.1f);
                    else
                    {
                        owner.SetFieldValue("avatarAngryTime", 0f);
                        HeroController.SetAvatarCalm(PlayerNum, owner.usePrimaryAvatar);
                    }
                }
            }
            else if (avatarAngryTime > 0f)
            {
                avatarAngryTime -= hero.DeltaTime;
                owner.SetFieldValue("avatarAngryTime", avatarAngryTime);
                if (avatarAngryTime <= 0f)
                {
                    HeroController.SetAvatarCalm(PlayerNum, owner.usePrimaryAvatar);
                }
            }
            return false;
        }
    }
}
