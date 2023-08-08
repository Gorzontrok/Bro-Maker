using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BroMakerLib.Stats
{
    public class MookState : CharacterStats
    {
        public MookState() { maxHealth = 3; }


        // Hear
        public bool canHear = true;
        public float hearingRangeX = 300f;
        public float hearingRangeY = 200f;

        // Category
        public bool isHellEnemy = false;
        public bool isSkinnedMook = false;

    }
}
