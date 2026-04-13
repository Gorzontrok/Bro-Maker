using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using Newtonsoft.Json;
using RocketLib.Extensions;
using UnityEngine;

namespace BroMakerLib.Vanilla.Specials
{
    [SpecialPreset("MrAnderbro")]
    public class NebroSpecial : SpecialAbility
    {
        protected override HeroType SourceBroType => HeroType.Nebro;
        public float zoneCooldown = 0.33f;
        public float invulnerableOnUse = 0.2f;

        [JsonIgnore]
        private ProjectileReturnZone returnZonePrefab;
        [JsonIgnore]
        private ProjectileReturnZone currentZone;
#pragma warning disable 649
        [JsonIgnore]
        private float lastReturnZoneTime;
#pragma warning restore 649

        public override void Initialize(TestVanDammeAnim owner)
        {
            base.Initialize(owner);
            var nebro = owner as Nebro;
            if (nebro == null)
            {
                var prefab = HeroController.GetHeroPrefab(HeroType.Nebro);
                nebro = prefab as Nebro;
            }
            if (nebro != null)
            {
                returnZonePrefab = nebro.returnZonePrefab;
            }
        }

        public override void AnimateSpecial()
        {
            hero.SetSpriteOffset(0f, 0f);
            hero.DeactivateGun();
            hero.FrameRate = 0.0334f;
            int column = 16 + Mathf.Clamp(owner.frame, 0, 4);
            hero.Sprite.SetLowerLeftPixel(column * hero.SpritePixelWidth, hero.SpritePixelHeight);
            if (owner.frame == 2)
            {
                UseSpecial();
            }
            if (owner.frame >= 4)
            {
                owner.frame = 0;
                hero.ActivateGun();
                hero.UsingSpecial = false;
            }
        }

        public override void UseSpecial()
        {
            if (owner.SpecialAmmo > 0 && Time.time - lastReturnZoneTime > zoneCooldown
                && (currentZone == null || currentZone.PoolIndex == -1 || currentZone.life <= 0f))
            {
                hero.SetInvulnerable(invulnerableOnUse, false, false);
                owner.SpecialAmmo--;
                HeroController.SetSpecialAmmo(PlayerNum, owner.SpecialAmmo);
                currentZone = EffectsController.InstantiateEffect(returnZonePrefab,
                    owner.transform.position + Vector3.right * owner.transform.localScale.x * 10f + Vector3.up * 7f,
                    Quaternion.identity) as ProjectileReturnZone;
                if (currentZone != null)
                {
                    currentZone.playerNum = PlayerNum;
                    currentZone.firedBy = owner;
                    currentZone.transform.parent = owner.transform;
                }
            }
        }

        public override bool HandleReleaseSpecial()
        {
            if (currentZone != null && currentZone.PoolIndex != -1)
            {
                currentZone.life = Mathf.Clamp(currentZone.life - 1f, 0.01f, returnZonePrefab.life);
            }
            return true;
        }
    }
}
