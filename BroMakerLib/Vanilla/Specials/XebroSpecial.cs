using System.Collections.Generic;
using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using BroMakerLib.Extensions;
using Newtonsoft.Json;
using UnityEngine;

namespace BroMakerLib.Vanilla.Specials
{
    [SpecialPreset("Xebro")]
    public class XebroSpecial : SpecialAbility
    {
        protected override HeroType SourceBroType => HeroType.Xebro;
        public float throwSoundVolume = 0.5f;

        [JsonIgnore]
        protected Projectile chakramPrefab;
        [JsonIgnore]
        private List<Chakram> thrownChakram = new List<Chakram>();

        public XebroSpecial()
        {
            animationColumn = 17;
            animationRow = 5;
            animationFrameCount = 8;
            triggerFrame = 4;
            spawnOffsetX = 6.5f;
            spawnOffsetY = 6.5f;
        }

        public override void Initialize(TestVanDammeAnim owner)
        {
            base.Initialize(owner);
            var prefab = HeroController.GetHeroPrefab(HeroType.Xebro);
            var xebro = prefab.GetComponent<Xebro>();
            if (xebro != null)
            {
                chakramPrefab = xebro.chakramPrefab;
            }
        }

        public override void UseSpecial()
        {
            if (owner.SpecialAmmo > 0)
            {
                owner.SpecialAmmo--;
                if (owner.IsMine)
                {
                    ThrowChakram();
                }
            }
            else
            {
                HeroController.FlashSpecialAmmo(PlayerNum);
            }
        }

        private void ThrowChakram()
        {
            Vector3 vector = new Vector3(owner.transform.localScale.x, -1f);
            Vector3 normalized = vector.normalized;
            var chakram = ProjectileController.SpawnProjectileOverNetwork(chakramPrefab, owner, X + Direction * spawnOffsetX, Y + spawnOffsetY, normalized.x, normalized.y, true, PlayerNum, false, false, 0f) as Chakram;
            thrownChakram.Add(chakram);
            Sound.GetInstance().PlaySoundEffectAt(throwSounds, throwSoundVolume, owner.transform.position, 1f, true, false, false, 0f);
            var broBase = owner as BroBase;
            if (broBase != null) broBase.meleeType = BroBase.MeleeType.Custom;
        }

        public void CatchChakram(Chakram chakram)
        {
            var broBase = owner as BroBase;
            if (broBase != null) broBase.meleeType = BroBase.MeleeType.Punch;
            owner.SpecialAmmo++;
            thrownChakram.Remove(chakram);
        }

        public override void HandleAfterDeath()
        {
            KillThrownChakrams();
        }

        public override void HandleAfterRecallBro()
        {
            KillThrownChakrams();
        }

        private void KillThrownChakrams()
        {
            if (owner is Xebro) return;
            for (int i = 0; i < thrownChakram.Count; i++)
            {
                if (thrownChakram[i] != null)
                {
                    thrownChakram[i].Death();
                }
            }
            thrownChakram.Clear();
        }
    }
}
