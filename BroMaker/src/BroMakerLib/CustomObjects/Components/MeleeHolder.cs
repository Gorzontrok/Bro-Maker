using System;
using System.Collections.Generic;
using BroMakerLib.CustomObjects;
using UnityEngine;

namespace BroMakerLib
{
    public class MeleeHolder : MonoBehaviour
    {
		public ICustomHero character;

        public void StartMelee(BroBase.MeleeType meleeType)
        {
			if(character != null)
			{
				switch (meleeType)
                {
                    case BroBase.MeleeType.Disembowel:
                        PerformDisembowelAtack();
                        break;
                    case BroBase.MeleeType.Tazer:
                        PerformTazerMeleeAttack();
                        break;
                    case BroBase.MeleeType.Punch:
                        PerformPunchAttack();
                        break;
					case BroBase.MeleeType.ChainSaw:
                        PerformChainsawMelee();
                        break;
					case BroBase.MeleeType.ThrowingKnife:
                        PerformThrowingKnifeMelee();
                        break;
                    case BroBase.MeleeType.Knife:
                        PerformKnifeMeleeAttack();
                        break;
					case BroBase.MeleeType.Smash:
                        PerformSmashAttack(); break;
					case BroBase.MeleeType.JetpackPunch:
                    case BroBase.MeleeType.FlipKick:
					case BroBase.MeleeType.Custom:
					case BroBase.MeleeType.ChuckKick:
					case BroBase.MeleeType.VanDammeKick:
					case BroBase.MeleeType.BrobocopPunch:
					case BroBase.MeleeType.PistolWhip:
					case BroBase.MeleeType.HeadButt:
					case BroBase.MeleeType.TeleportStab:
						break;
                }
			}
		}

        protected virtual void Awake()
        {
			character =  GetComponent<ICustomHero>();
        }

        protected virtual void PerformDisembowelAtack()
		{
            // Machete
            /* if (this.meleeChosenUnit)
             {
                 if (this.meleeChosenUnit.CanDisembowel)
                 {
                     this.meleeChosenUnit.ForceFaceDirection(-base.Direction);
                     this.meleeChosenUnit.Damage(25, DamageType.Disembowel, 0f, 0f, base.Direction, this, base.X, base.Y);
                 }
                 else
                 {
                     this.meleeChosenUnit.Damage(4, DamageType.Melee, 0f, 0f, base.Direction, this, base.X, base.Y);
                 }
                 if (!this.meleeHasHit)
                 {
                     EffectsController.CreateProjectilePopWhiteEffect(this.meleeChosenUnit.X - 8f * base.transform.localScale.x, base.Y + this.height);
                 }
                 Map.PanicUnits(base.X, base.Y, 84f, 24f, 2f, true, false);
                 if (!this.meleeHasHit)
                 {
                     this.sound.PlaySoundEffectAt(this.soundHolder.alternateMeleeHitSound, 1f, base.transform.position, 1f, true, false, false, 0f);
                 }
                 this.meleeHasHit = true;
             }
             else
             {
                 if (!this.hasPlayedMissSound)
                 {
                     this.sound.PlaySoundEffectAt(this.soundHolder.missSounds, 0.5f, base.transform.position, 1f, true, false, false, 0f);
                 }
                 this.hasPlayedMissSound = true;
             }
             if (shouldTryHitTerrain && this.TryMeleeTerrain(0, 2))
             {
                 this.meleeHasHit = true;
             }*/
        }
        protected virtual void PerformTazerMeleeAttack()
        {
            // BroDredd
            /* bool flag;
            Map.DamageDoodads(3, DamageType.Shock, base.X + (float)(base.Direction * 4), base.Y, 0f, 0f, 6f, base.playerNum, out flag, null);
            Unit unit = Map.HitClosestUnit(this, base.playerNum, 1, DamageType.Shock, 13f, 24f, base.X + base.transform.localScale.x * 4f, base.Y + 8f, base.transform.localScale.x * 0f, 0f, false, true, base.IsMine, false, true);
            if (unit != null)
            {
                this.meleeHasHit = true;
                if (unit == this.previouslyTasedUnit)
                {
                    this.tasedCount++;
                    if (this.tasedCount > 12)
                    {
                        Debug.Log("Not networked extra plasma damage");
                        unit.Damage(this.tasedCount / 12, DamageType.Plasma, 0f, 0f, base.Direction, this, unit.X, unit.Y + 5f);
                    }
                }
                else
                {
                    this.tasedCount = 0;
                    this.previouslyTasedUnit = unit;
                }
            }
            else if (playMissSound)
            {
                this.sound.PlaySoundEffectAt(this.soundHolder.alternateMeleeHitSound, 0.3f, base.transform.position, UnityEngine.Random.Range(0.9f, 1.1f), true, false, false, 0f);
            }
            if (shouldTryHitTerrain && this.TryMeleeTerrain(0, 2))
            {
                this.meleeHasHit = true;
            }*/
        }

        protected virtual void PerformPunchAttack()
        {
            //BaBroracus
            /*float num = 6f;
            Vector3 vector = new Vector3(base.X + (float)base.Direction * (num + 1f), base.Y + 8f + 4f, 0f);
            bool flag;
            Map.DamageDoodads(3, DamageType.Melee, vector.x, vector.y, 0f, 0f, 6f, base.playerNum, out flag, null);
            base.KickDoors(25f);
            if (Map.HitClosestUnit(this, base.playerNum, 6, DamageType.Melee, num + 13f, num * 2f, vector.x, vector.y, base.transform.localScale.x * 250f, 250f, true, false, base.IsMine, false, true))
            {
                this.sound.PlaySoundEffectAt(this.soundHolder.alternateMeleeHitSound, 0.5f, base.transform.position, 1f, true, false, false, 0f);
                this.meleeHasHit = true;
                EffectsController.CreateProjectilePopWhiteEffect(base.X + this.width * base.transform.localScale.x, base.Y + this.height + 8f);
            }
            else
            {
                if (playMissSound && !this.hasPlayedMissSound)
                {
                    this.sound.PlaySoundEffectAt(this.soundHolder.missSounds, 0.15f, base.transform.position, 1f, true, false, false, 0f);
                }
                this.hasPlayedMissSound = true;
            }
            this.meleeChosenUnit = null;
            if (!this.meleeHasHit && shouldTryHitTerrain && this.TryMeleeTerrain(0, 2))
            {
                this.meleeHasHit = true;
            }*/
        }

        protected virtual void PerformChainsawMelee()
        {
            // ChainsawMelee
            /*float num = 22f;
            float num2 = 8f;
            this.chainSawMeleedUnit = Map.GetNearestEnemyUnit(base.playerNum, (int)num, 16, base.X + (float)base.Direction * num2, base.Y, true, base.Direction, this);
            if (this.chainSawMeleedUnit == null)
            {
                this.chainSawMeleedUnit = Map.GetNearestEnemyUnit(base.playerNum, (int)num, 16, base.X + (float)base.Direction * num2, base.Y, true, -base.Direction, this);
            }
            if (this.lastChainSawMeleeDamageTimeStamp + 0.1f > Time.time)
            {
                return;
            }
            this.lastChainSawMeleeDamageTimeStamp = Time.time;
            if (this.chainSawMeleedUnit != null)
            {
                if (this.initialDirection > 0 && this.chainSawMeleedUnit.X < base.X + 8f)
                {
                    this.chainSawMeleedUnit.X = base.X + 8f;
                    this.chainSawMeleedUnit.SetPosition();
                }
                if (this.initialDirection < 0 && this.chainSawMeleedUnit.X > base.X - 8f)
                {
                    this.chainSawMeleedUnit.X = base.X - 8f;
                    this.chainSawMeleedUnit.SetPosition();
                }
                EffectsController.CreateBloodParticles(this.chainSawMeleedUnit.bloodColor, this.chainSawMeleedUnit.X, this.chainSawMeleedUnit.Y + 8f, 6, 8f, 8f, 60f, (float)(this.DirectionSynced * 5), 100f + UnityEngine.Random.value * 50f);
            }
            bool flag;
            Map.DamageDoodads(3, DamageType.Knifed, base.X + (float)(base.Direction * 6), base.Y, 0f, 0f, 6f, base.playerNum, out flag, null);
            if (this.chainSawMeleedUnit != null)
            {
                this.chainSawMeleedUnit.Damage(4, DamageType.ChainsawImpale, 0f, 0f, base.Direction, this, base.X, base.Y);
                Map.PanicUnits(base.X, base.Y, 80f, 24f, 2f, true, false);
            }
            this.TryMeleeTerrain(8, 2);*/
        }

        protected virtual void PerformThrowingKnifeMelee()
        {
            // Brade
            /*this.knifeThrown = true;
            this.PlayAttackSound(0.44f);
            ProjectileController.SpawnProjectileLocally(this.throwingKnife, this, base.X + (float)(16 * base.Direction), base.Y + 10f, this.xI + (float)(250 * base.Direction), 0f, base.playerNum);
        */
        }

        protected virtual void PerformKnifeMeleeAttack()
        {
            // BroBase
           /* bool flag;
            Map.DamageDoodads(3, DamageType.Knifed, base.X + (float)(base.Direction * 4), base.Y, 0f, 0f, 6f, base.playerNum, out flag, null);
            this.KickDoors(24f);
            if (Map.HitClosestUnit(this, base.playerNum, 4, DamageType.Knifed, 14f, 24f, base.X + base.transform.localScale.x * 8f, base.Y + 8f, base.transform.localScale.x * 200f, 500f, true, false, base.IsMine, false, true))
            {
                this.sound.PlaySoundEffectAt(this.soundHolder.meleeHitSound, 1f, base.transform.position, 1f, true, false, false, 0f);
                this.meleeHasHit = true;
            }
            else if (playMissSound)
            {
                this.sound.PlaySoundEffectAt(this.soundHolder.missSounds, 0.3f, base.transform.position, 1f, true, false, false, 0f);
            }
            this.meleeChosenUnit = null;
            if (shouldTryHitTerrain && this.TryMeleeTerrain(0, 2))
            {
                this.meleeHasHit = true;
            }
            this.TriggerBroMeleeEvent();*/
        }
        protected virtual void PerformSmashAttack()
        {
            // BroBase
            /*if (!this.hasPlayedMissSound)
            {
                this.sound.PlaySoundEffectAt(this.soundHolder.alternateMeleeMissSound, 0.3f, base.transform.position, 1f, true, false, false, 0f);
                this.hasPlayedMissSound = true;
            }
            float num = 8f;
            Vector3 vector = new Vector3(base.X + (float)base.Direction * num, base.Y + 8f, 0f);
            bool flag;
            Map.DamageDoodads(3, DamageType.Melee, vector.x, vector.y, 0f, 0f, 6f, base.playerNum, out flag, null);
            if (Map.HitClosestUnit(this, base.playerNum, 10, DamageType.Crush, num, num * 2f, vector.x, vector.y, base.transform.localScale.x * 20f, 50f, true, false, base.IsMine, true, true))
            {
                if (!this.meleeHasHit)
                {
                    this.sound.PlaySoundEffectAt(this.soundHolder.alternateMeleeHitSound, 0.5f, base.transform.position, 1f, true, false, false, 0f);
                }
                this.meleeHasHit = true;
            }
            this.meleeChosenUnit = null;
            if (!this.meleeHasHit && this.TryMeleeTerrain(0, 2))
            {
                this.meleeHasHit = true;
            }
            this.TriggerBroMeleeEvent();*/
        }
    }
}
