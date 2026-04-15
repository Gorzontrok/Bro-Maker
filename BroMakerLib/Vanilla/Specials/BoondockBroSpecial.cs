using System;
using System.Reflection;
using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using HarmonyLib;
using Newtonsoft.Json;
using RocketLib.Extensions;
using UnityEngine;

namespace BroMakerLib.Vanilla.Specials
{
    /// <summary>Boondock Bro's companion-summoning special.</summary>
    [SpecialPreset("BoondockBros")]
    public class BoondockBroSpecial : SpecialAbility
    {
        protected override HeroType SourceBroType => HeroType.BoondockBros;
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

        private static readonly MethodInfo setupTrailingBro = AccessTools.Method(typeof(BoondockBro), "SetUpTrailingBro",
            new Type[] { typeof(BoondockBro), typeof(int), typeof(float), typeof(float), typeof(bool) });
        private static readonly MethodInfo setupConnollyBro = AccessTools.Method(typeof(BoondockBro), "SetUpConnollyBro",
            new Type[] { typeof(BoondockBro), typeof(BoondockBro), typeof(int) });
        private static readonly MethodInfo setConnollyBro = AccessTools.Method(typeof(BoondockBro), "SetConnollyBro",
            new Type[] { typeof(BoondockBro) });

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
                        setupConnollyBro.Invoke(null, new object[] { connollyBro, owner as BoondockBro, PlayerNum });
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
                setupTrailingBro.Invoke(trailingBro, new object[] { owner as BoondockBro, PlayerNum, defaultSpeed, defaultFireRate, !owner.usePrimaryAvatar });
                trailingBro.xI = owner.xI;
                trailingBro.yI = owner.yI;
                if (connollyBro != null)
                {
                    setConnollyBro.Invoke(trailingBro, new object[] { connollyBro });
                }
            }
        }

        private void RecallCompanion(BoondockBro companion)
        {
            if (companion != null && companion.health > 0)
            {
                companion.RecallBro();
            }
        }

        private void DestroyCompanion(ref BoondockBro companion)
        {
            if (companion != null)
            {
                UnityEngine.Object.Destroy(companion.gameObject);
                companion = null;
            }
        }

        private void HideCompanion(BoondockBro companion)
        {
            if (companion == null) return;
            companion.enabled = false;
            companion.GetComponent<Renderer>().enabled = false;
            companion.gunSprite.GetComponent<Renderer>().enabled = false;
            companion.gunSprite.enabled = false;
            companion.invulnerable = true;
            companion.health = 10000;
        }

        private void RestoreCompanion(BoondockBro companion, float x, float y, float xI, float yI)
        {
            if (companion == null) return;
            companion.DischargePilotingUnit(x, y, xI, yI, false);
            companion.GetComponent<Renderer>().enabled = true;
        }

        public override bool HandleDeath()
        {
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

        public override bool HandleRecallBro()
        {
            if (!(owner is BoondockBro))
            {
                RecallCompanion(trailingBro);
                RecallCompanion(connollyBro);
                trailingBro = null;
                connollyBro = null;
            }
            return true;
        }

        public override bool HandleStartPilotingUnit()
        {
            if (!(owner is BoondockBro))
            {
                HideCompanion(trailingBro);
                HideCompanion(connollyBro);
            }
            return true;
        }

        public override void HandleAfterDischargePilotingUnit()
        {
            if (!(owner is BoondockBro))
            {
                RestoreCompanion(trailingBro, owner.X, owner.Y, owner.xI, owner.yI);
                RestoreCompanion(connollyBro, owner.X, owner.Y, owner.xI, owner.yI);
            }
        }

        public override void HandleDestroyUnit()
        {
            if (!(owner is BoondockBro))
            {
                DestroyCompanion(ref trailingBro);
                DestroyCompanion(ref connollyBro);
            }
        }

        public override void Cleanup()
        {
            if (!(owner is BoondockBro))
            {
                DestroyCompanion(ref trailingBro);
                DestroyCompanion(ref connollyBro);
            }
            base.Cleanup();
        }
    }
}
