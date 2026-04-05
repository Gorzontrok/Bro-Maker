using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using Newtonsoft.Json;
using UnityEngine;

namespace BroMakerLib.Vanilla.Specials
{
    [SpecialPreset("BroInBlack")]
    public class BrononymousSpecial : SpecialAbility
    {
        public float specialSoundVolume = 0.3f;

        [JsonIgnore]
        protected Neuraliser neuraliserPrefab;

        public BrononymousSpecial()
        {
            animationColumn = 17;
            animationRow = 5;
            animationFrameCount = 8;
            triggerFrame = 4;
        }

        public override void Initialize(TestVanDammeAnim owner)
        {
            base.Initialize(owner);
            var prefab = HeroController.GetHeroPrefab(HeroType.Brononymous);
            var brononymous = prefab.GetComponent<Brononymous>();
            if (brononymous != null)
            {
                neuraliserPrefab = brononymous.neuraliserPrefab;
            }
            if (specialAttackSounds == null)
            {
                var sourceBro = prefab.GetComponent<TestVanDammeAnim>();
                specialAttackSounds = sourceBro.soundHolder.specialAttackSounds;
            }
        }

        public override void UseSpecial()
        {
            if (owner.SpecialAmmo > 0)
            {
                owner.SpecialAmmo--;
                HeroController.SetSpecialAmmo(PlayerNum, owner.SpecialAmmo);
                Sound.GetInstance().PlaySoundEffectAt(specialAttackSounds, specialSoundVolume, owner.transform.position);
                Neuraliser neuraliser = EffectsController.InstantiateEffect(neuraliserPrefab, owner.transform.position + Vector3.up * (owner.height + 3f) + Vector3.right * owner.transform.localScale.x * 4f, Quaternion.identity) as Neuraliser;
                neuraliser.direction = (int)owner.transform.localScale.x;
                neuraliser.playerNum = PlayerNum;
            }
            else
            {
                HeroController.FlashSpecialAmmo(PlayerNum);
                hero.ActivateGun();
            }
            hero.PressSpecialFacingDirection = 0;
        }
    }
}
