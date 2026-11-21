namespace BroMakerLib.Abilities
{
    /// <summary>Base class for passive abilities, hook-driven behaviors with no dedicated button.</summary>
    public abstract class PassiveAbility : AbilityBase
    {
        /// <summary>True when the owner's base class already provides this passive's behavior; redundant passives are discarded without being attached.</summary>
        public bool IsRedundant { get; private set; }

        /// <summary>
        /// Returns true when the owner's own class already hosts this passive's behavior.
        /// Typically implemented as `owner is SomeBroClass`.
        /// Also called on a constructed but uninitialized instance to find conflicts with the owner's native abilities, so it must depend only on `owner`.
        /// That probe runs the constructor and field initializers on every spawn of a bro listing a related passive, so they must have no side effects.
        /// </summary>
        protected virtual bool IsOwnerRedundant(BroBase owner) => false;

        internal bool IsRedundantOn(BroBase owner) => IsOwnerRedundant(owner);

        /// <summary>Called once when the bro spawns. Sets `IsRedundant` and, if not redundant, calls `CacheSoundsFromPrefab`.</summary>
        public override void Initialize(BroBase owner)
        {
            base.Initialize(owner);
            IsRedundant = IsOwnerRedundant(owner);
            if (IsRedundant)
            {
                return;
            }
            CacheSoundsFromPrefab();
        }

        /// <summary>Loads the passive's own sound clips; null-guard each assignment so JSON overrides aren't stomped.</summary>
        protected virtual void CacheSoundsFromPrefab()
        {
        }
    }
}
