using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using Newtonsoft.Json;
using RocketLib.Extensions;
using UnityEngine;

namespace BroMakerLib.Vanilla.Specials
{
    [SpecialPreset("BoondockBro")]
    public class BoondockBroSpecial : SpecialAbility
    {
        [JsonIgnore]
        private BoondockBro trailingBro;
        [JsonIgnore]
        private BoondockBro connollyBro;
        [JsonIgnore]
        private BillyConnolly billyConnollyPrefab;
        [JsonIgnore]
        private float defaultSpeed;
        [JsonIgnore]
        private float defaultFireRate;

        public override void Initialize(TestVanDammeAnim owner)
        {
            base.Initialize(owner);
            defaultSpeed = owner.speed;
            defaultFireRate = owner.GetFieldValue<float>("fireRate");

            var boondock = owner as BoondockBro;
            if (boondock == null)
            {
                var prefab = HeroController.GetHeroPrefab(HeroType.BoondockBros);
                boondock = prefab as BoondockBro;
            }
            if (boondock != null)
            {
                billyConnollyPrefab = boondock.billyConnollyPrefab;
            }
        }

        public override void UseSpecial()
        {
            if (owner.SpecialAmmo > 0)
            {
                if (trailingBro != null && trailingBro.health > 0 && connollyBro == null)
                {
                    owner.SpecialAmmo--;
                    HeroController.SetSpecialAmmo(PlayerNum, owner.SpecialAmmo);
                    if (billyConnollyPrefab != null)
                    {
                        connollyBro = Networking.Networking.InstantiateBuffered<BillyConnolly>(billyConnollyPrefab,
                            owner.transform.position, owner.transform.rotation, false);
                        connollyBro.CallMethod("SetUpConnollyBro", connollyBro, owner as BoondockBro, PlayerNum);
                        trailingBro.connollyBro = connollyBro;
                    }
                }
                else if (trailingBro == null || trailingBro.health <= 0)
                {
                    trailingBro = null;
                    owner.SpecialAmmo--;
                    HeroController.SetSpecialAmmo(PlayerNum, owner.SpecialAmmo);
                    SpawnTrailingBro();
                }
            }
        }

        private void SpawnTrailingBro()
        {
            if (!owner.IsMine) return;
            var ownerBoondock = owner as BoondockBro;
            if (ownerBoondock != null)
            {
                ownerBoondock.SetFieldValue("position", 0);
            }
            var prefab = HeroController.GetHeroPrefab(HeroType.BoondockBros);
            trailingBro = Networking.Networking.InstantiateBuffered<TestVanDammeAnim>(prefab,
                owner.transform.position, owner.transform.rotation, new object[0], false) as BoondockBro;
            if (trailingBro != null)
            {
                trailingBro.CallMethod("SetUpTrailingBro", owner as BoondockBro, PlayerNum, defaultSpeed, defaultFireRate, !owner.usePrimaryAvatar);
                trailingBro.xI = owner.xI;
                trailingBro.yI = owner.yI;
                if (connollyBro != null)
                {
                    trailingBro.CallMethod("SetConnollyBro", connollyBro);
                }
            }
        }

        public override bool HandleDeath()
        {
            // For BoondockBro owners, let vanilla ReduceLives handle the death-transfer
            // mechanic (trailing bro becomes new lead without losing a life).
            // For non-BoondockBro owners, kill companions since there's no transfer.
            if (!(owner is BoondockBro))
            {
                if (trailingBro != null && trailingBro.health > 0)
                {
                    trailingBro.Death(0f, 0f, null);
                }
                if (connollyBro != null && connollyBro.health > 0)
                {
                    connollyBro.Death(0f, 0f, null);
                }
            }
            trailingBro = null;
            connollyBro = null;
            return true;
        }
    }
}
