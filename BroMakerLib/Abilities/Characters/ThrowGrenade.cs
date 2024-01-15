using BroMakerLib.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BroMakerLib.Abilities.Characters
{
    [AbilityPreset("GrenadeThrow")]
    public class ThrowGrenade : CharacterAbility
    {
        public bool useCharacterSpecialGrenade = true;
        public Grenade grenade = null;

        public float spawnPositionXMultiplier = 8f;
        public float spawnPositionYAddition = 8f;
        public Vector2 spawnForce = new Vector2(200f, 150f);
        public float lifeM = 1f;

        public bool canThrowAtFeet = true;
        public float spawnFeetPositionXMultiplier = 6f;
        public float spawnFeetPositionYAddition = 3f;
        public Vector2 spawnForceFeet = new Vector2(30f, 70f);
        public float lifeMAtFeet = 1f;

        public void ThrowTheGrenade()
        {
            Grenade grenade = owner.specialGrenade;
            if (!useCharacterSpecialGrenade && grenade != null)
                grenade = this.grenade;

            if (canThrowAtFeet && owner.down && owner.IsOnGround() && owner.IsDucking)
            {
                ProjectileController.SpawnGrenadeOverNetwork(grenade, owner,
                    owner.X + Mathf.Sign(owner.transform.localScale.x) * spawnFeetPositionXMultiplier, // x
                    owner.Y + spawnFeetPositionYAddition, // y
                    0.001f, 0.011f, // radius and force
                    Mathf.Sign(owner.transform.localScale.x) * spawnForceFeet.x, // xI
                    spawnForceFeet.y, // yI
                    owner.playerNum,
                    lifeMAtFeet
                    );
            }
            else
            {
                ProjectileController.SpawnGrenadeOverNetwork(grenade, owner,
                    owner.X + Mathf.Sign(owner.transform.localScale.x) * spawnPositionXMultiplier, // x
                    owner.Y + spawnPositionYAddition, // y
                    0.001f, 0.011f, // radius and force
                    Mathf.Sign(owner.transform.localScale.x) * spawnForce.x, // xI
                    spawnForce.y, // yI
                    owner.playerNum,
                    lifeM
                    );
            }
        }

        public override void All(string calledFromMethod, params object[] objects)
        {
            if (calledFromMethod == "Update" || calledFromMethod == "UseSpecial" || calledFromMethod == "Awake" || calledFromMethod == "Start")
                return;

            ThrowTheGrenade();
        }

        public override void UseSpecial()
        {
            ThrowTheGrenade();
        }
    }
}
