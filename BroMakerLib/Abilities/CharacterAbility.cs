using UnityEngine;

namespace BroMakerLib.Abilities
{
    /// <summary>
    /// Base class for Ability made for Characters (<see cref="TestVanDammeAnim"/>)
    /// </summary>
    public class CharacterAbility : Ability<TestVanDammeAnim>
    {
        public int PlayerNum
        {
            get { return owner.playerNum; }
        }

        public float OwnerDirection
        {
            get { return Mathf.Sign(owner.transform.localScale.x); }
        }

        public virtual void ActivateGun()
        { }
        public virtual void FireWeapon(float x, float y, float xSpeed, float ySpeed)
        { }
        public virtual void UseSpecial()
        { }
        public virtual void Melee()
        { }
        public virtual void Jump(bool wallJump)
        { }
        public virtual void RecallBro()
        { }
        public virtual void Land()
        { }
        public virtual void StartFiring()
        { }
        public virtual void StopFiring()
        { }
        public virtual void UseFire()
        { }
    }
}
