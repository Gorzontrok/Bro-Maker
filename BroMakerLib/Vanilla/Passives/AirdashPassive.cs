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

        public override void Initialize(TestVanDammeAnim owner)
        {
            base.Initialize(owner);
            originalCanAirdash = owner.GetFieldValue<bool>("canAirdash");
            originalAirdashDelay = owner.GetFieldValue<float>("defaultAirdashDelay");
            originalAirdashMaxTime = owner.airdashMaxTime;

            owner.SetFieldValue("canAirdash", true);
            owner.SetFieldValue("defaultAirdashDelay", defaultAirdashDelay);
            owner.airdashMaxTime = airdashMaxTime;
        }

        public override void Cleanup()
        {
            if (owner == null) return;
            owner.SetFieldValue("canAirdash", originalCanAirdash);
            owner.SetFieldValue("defaultAirdashDelay", originalAirdashDelay);
            owner.airdashMaxTime = originalAirdashMaxTime;
        }

        public override bool HandlePressHighFiveMelee()
        {
            bool wasHighFive = owner.GetFieldValue<bool>("wasHighFive");
            if (owner.up && CanAirDash(DirectionEnum.Up)) { if (!wasHighFive) Airdash(true); return false; }
            if (owner.right && CanAirDash(DirectionEnum.Right)) { if (!wasHighFive) Airdash(true); return false; }
            if (owner.left && CanAirDash(DirectionEnum.Left)) { if (!wasHighFive) Airdash(true); return false; }
            if (owner.down && CanAirDash(DirectionEnum.Down)) { if (!wasHighFive) Airdash(true); return false; }
            if (owner.GetFieldValue<float>("airdashTime") > 0f) return false;
            return true;
        }

        protected bool CanAirDash(DirectionEnum direction)
            => owner.CallMethod<bool>("CanAirDash", direction);

        protected void Airdash(bool highFived)
            => owner.CallMethod("Airdash", highFived);
    }
}
