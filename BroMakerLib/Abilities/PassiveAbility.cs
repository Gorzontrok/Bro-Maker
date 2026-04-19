using UnityEngine;

namespace BroMakerLib.Abilities
{
    /// <summary>Base class for passive abilities, hook-driven behaviors with no dedicated button.</summary>
    public abstract class PassiveAbility : AbilityBase
    {
        /// <summary>Vanilla bro whose prefab is the source of this passive's sounds.</summary>
        protected virtual HeroType SourceBroType => HeroType.Rambro;

        /// <summary>True when the owner's base class already provides this passive's behavior; redundant passives are discarded without being attached.</summary>
        public bool IsRedundant { get; private set; }

        /// <summary>Override to return true when the owner's own class already hosts this passive's
        /// behavior (e.g. TheBrocketeerPassive on TheBrocketeer). Typically implemented as `owner is SomeBroClass`.</summary>
        protected virtual bool IsOwnerRedundant(BroBase owner) => false;

        /// <summary>Called once when the bro spawns. Sets `IsRedundant` and, if not redundant, calls `CacheSoundsFromPrefab`.</summary>
        public override void Initialize(BroBase owner)
        {
            base.Initialize(owner);
            IsRedundant = IsOwnerRedundant(owner);
            if (IsRedundant) return;
            CacheSoundsFromPrefab();
        }

        /// <summary>Override to load passive-specific sound clips from the SourceBroType prefab.
        /// Null-guard each assignment so JSON overrides aren't stomped.</summary>
        protected virtual void CacheSoundsFromPrefab()
        {
        }
    }
}
