using BroMakerLib.Infos;
using BroMakerLib.Stats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BroMakerLib.CustomObjects.Components
{
    public class CharacterExtended : MonoBehaviour
    {
        public CustomCharacterInfo info { get; set; }

        public float multiplierOfBouncyJumpMultiplyer = 1f;
        public float gravityMultiplier = 1f;
        public float acidMeltTimer = 1f;
        public float acidParticleTimer = 0.1f;
        public int maxSpurtCount = 5;

        public virtual void LoadStats(CharacterStats stats)
        {
            multiplierOfBouncyJumpMultiplyer = stats.multiplierOfBouncyJumpMultiplyer;
            gravityMultiplier = stats.gravityMultiplier;
            acidMeltTimer = stats.acid.acidMeltTimer;
            acidParticleTimer = stats.acid.acidParticleTimer;
            maxSpurtCount = stats.blood.maxSpurtCount;
        }
    }
}
