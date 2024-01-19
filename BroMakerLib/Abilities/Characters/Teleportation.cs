using BroMakerLib.Attributes;
using Newtonsoft.Json;
using RocketLib.JsonConverters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BroMakerLib.Abilities.Characters
{
    [AbilityPreset(nameof(Teleportation))]
    public class Teleportation : CharacterAbility
    {
        public int teleportationFrame = 0;
        public bool hideGun = true;

        // Animation
        [JsonConverter(typeof(Vector2Converter))]
        public Vector2 startTeleportationCoordinate = Vector2.zero;

        protected bool _isTeleporting = false;

        public void StartTeleporting()
        {
            owner.invulnerable = true;
            _isTeleporting = true;
            if (hideGun)
            {
                owner.CallMethod("DeactivateGun");
            }
        }

        protected override void Update()
        {
            base.Update();
            if (_isTeleporting )
            {
                owner.xI = 0;
                owner.yI = 0;
                if (owner.frame == teleportationFrame)
                {

                }
            }
        }
    }
}
