using BroMakerLib.Attributes;
using Rogueforce;

namespace BroMakerLib.Vanilla.Specials
{
    /// <summary>Bronan's leaping shockwave smash.</summary>
    [SpecialPreset("Bronan")]
    public class BronanSpecial : ShockwaveSpecial
    {
        public BronanSpecial()
        {
            lethal = true;
        }

        public override void PressSpecial()
        {
            if (owner.hasBeenCoverInAcid || hero.UsingSpecial || owner.health <= 0 || hero.DoingMelee)
            {
                return;
            }
            if (owner.SpecialAmmo > 0)
            {
                MapController.DamageGround(owner, ValueOrchestrator.GetModifiedDamage(groundDamage, PlayerNum), DamageType.Normal, groundDamageRange, X, Y, null, false);
                EffectsController.CreateGroundWave(X, Y, 2f);
                if (!hero.UsingSpecial)
                {
                    specialAttackDirection = owner.transform.localScale.x;
                    Sound.GetInstance().PlaySoundEffectAt(specialAttackSounds, pressSpecialSoundVolume, owner.transform.position);
                }
                hero.UsingSpecial = true;
                owner.frame = 0;
                hero.PressSpecialFacingDirection = (int)owner.transform.localScale.x;
                if (owner.actionState == ActionState.ClimbingLadder)
                {
                    owner.actionState = ActionState.Idle;
                }
                if (owner.dashing)
                {
                    owner.yI = jumpForceDashing;
                    owner.xIBlast += owner.transform.localScale.x * blastForceDashing;
                }
                else
                {
                    owner.yI = jumpForceNormal;
                    owner.xIBlast += owner.transform.localScale.x * blastForceNormal;
                }
                readyForBlast = false;
                setupBlastReadiness = false;
                hero.ChangeFrame();
            }
            else
            {
                HeroController.FlashSpecialAmmo(PlayerNum);
                hero.ActivateGun();
            }
        }

        public override bool HandleCheckForTraps()
        {
            return !hero.UsingSpecial;
        }
    }
}
