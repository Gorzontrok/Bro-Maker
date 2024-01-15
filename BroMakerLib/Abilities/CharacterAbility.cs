using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BroMakerLib.Abilities
{
    public class CharacterAbility : Ability<TestVanDammeAnim>
    {
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
    }
}
