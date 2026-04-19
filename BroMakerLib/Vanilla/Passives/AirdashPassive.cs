using BroMakerLib.Abilities;
using BroMakerLib.Extensions;
using Newtonsoft.Json;
using UnityEngine;

namespace BroMakerLib.Vanilla.Passives
{
    /// <summary>Shared base for airdash passives. Vanilla subclasses: NebroPassive, BroLeePassive.</summary>
    public class AirdashPassive : PassiveAbility
    {
        public float defaultAirdashDelay = 0.15f;
        public float airdashMaxTime = 0.5f;

        [JsonIgnore] private bool originalCanAirdash;
        [JsonIgnore] private float originalAirdashDelay;
        [JsonIgnore] private float originalAirdashMaxTime;

        public override void Initialize(BroBase owner)
        {
            base.Initialize(owner);
            originalCanAirdash = hero.CanAirdash;
            originalAirdashDelay = hero.DefaultAirdashDelay;
            originalAirdashMaxTime = owner.airdashMaxTime;

            hero.CanAirdash = true;
            hero.DefaultAirdashDelay = defaultAirdashDelay;
            owner.airdashMaxTime = airdashMaxTime;
        }

        public override void Cleanup()
        {
            if (IsRedundant || owner == null) return;
            hero.CanAirdash = originalCanAirdash;
            hero.DefaultAirdashDelay = originalAirdashDelay;
            owner.airdashMaxTime = originalAirdashMaxTime;
        }

        public override bool HandlePressHighFiveMelee()
        {
            bool wasHighFive = hero.WasHighFive;
            if (owner.up && CanAirDash(DirectionEnum.Up)) { if (!wasHighFive) Airdash(true); return false; }
            if (owner.right && CanAirDash(DirectionEnum.Right)) { if (!wasHighFive) Airdash(true); return false; }
            if (owner.left && CanAirDash(DirectionEnum.Left)) { if (!wasHighFive) Airdash(true); return false; }
            if (owner.down && CanAirDash(DirectionEnum.Down)) { if (!wasHighFive) Airdash(true); return false; }
            if (hero.AirdashTime > 0f) return false;
            return true;
        }

        protected bool CanAirDash(DirectionEnum direction)
            => owner.CallMethod<bool>("CanAirDash", direction);

        protected void Airdash(bool highFived)
            => owner.CallMethod("Airdash", highFived);
    }
}
