using System.Collections.Generic;

namespace BroMakerLib.Abilities
{
    /// <summary>A bro's `speed` while its abilities change it: the bro's unchanged speed times the multiplier of every ability holding a change.</summary>
    public class OwnerSpeedState
    {
        private readonly BroBase owner;
        private readonly List<AbilityBase> holders = new List<AbilityBase>();
        private readonly List<float> multipliers = new List<float>();
        private float unchangedSpeed;
        private float lastWrittenSpeed;

        public OwnerSpeedState(BroBase owner)
        {
            this.owner = owner;
        }

        /// <summary>
        /// Takes a `speed` written by anything but the ability speed helpers as the bro's new unchanged speed, keeping the held multipliers on top; the bro calls it once per
        /// frame.
        /// </summary>
        public void Sync()
        {
            if (holders.Count > 0 && owner.speed != lastWrittenSpeed)
            {
                unchangedSpeed = owner.speed;
                Write();
            }
        }

        internal float UnchangedSpeed
        {
            get
            {
                Sync();
                return holders.Count > 0 ? unchangedSpeed : owner.speed;
            }
        }

        internal void Save(AbilityBase ability)
        {
            Sync();
            if (!holders.Contains(ability))
            {
                AddHolder(ability, 1f);
            }
        }

        internal void Change(AbilityBase ability, float multiplier)
        {
            Sync();
            int index = holders.IndexOf(ability);
            if (index < 0)
            {
                AddHolder(ability, multiplier);
            }
            else
            {
                multipliers[index] = multiplier;
            }
            Write();
        }

        internal void Restore(AbilityBase ability)
        {
            Sync();
            int index = holders.IndexOf(ability);
            if (index < 0)
            {
                return;
            }
            holders.RemoveAt(index);
            multipliers.RemoveAt(index);
            Write();
        }

        private void AddHolder(AbilityBase ability, float multiplier)
        {
            if (holders.Count == 0)
            {
                unchangedSpeed = owner.speed;
                lastWrittenSpeed = owner.speed;
            }
            holders.Add(ability);
            multipliers.Add(multiplier);
        }

        private void Write()
        {
            float speed = unchangedSpeed;
            foreach (float multiplier in multipliers)
            {
                speed *= multiplier;
            }
            owner.speed = speed;
            lastWrittenSpeed = speed;
        }
    }
}
