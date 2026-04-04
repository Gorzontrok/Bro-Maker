using System.Collections.Generic;
using BroMakerLib.Abilities;
using BroMakerLib.Infos;
using UnityEngine;

namespace BroMakerLib.CustomObjects
{
    public interface ICustomHero
    {
        [Syncronize]
        CustomBroInfo Info { get; set; }
        [Syncronize]
        BroBase Character { get; set; }
        MuscleTempleFlexEffect FlexEffect { get; set; }
        int CurrentVariant { get; set; }
        Vector2 CurrentGunSpriteOffset { get; set; }
        List<Material> CurrentSpecialMaterials { get; set; }
        Vector2 CurrentSpecialMaterialOffset { get; set; }
        float CurrentSpecialMaterialSpacing { get; set; }
        Material CurrentFirstAvatar { get; set; }

        // Ability instances
        SpecialAbility SpecialAbility { get; }
        MeleeAbility MeleeAbility { get; }

        // Protected field access for the ability system
        SpriteSM Sprite { get; }
        int SpritePixelWidth { get; }
        int SpritePixelHeight { get; }
        bool DoingMelee { get; }
        bool Ducking { get; }
        float DeltaTime { get; }
        Sound Sound { get; }
        float FrameRate { get; set; }
        bool UsingSpecial { get; set; }
        bool UsingPockettedSpecial { get; set; }
        int PressSpecialFacingDirection { get; set; }
        int GunFrame { get; set; }
        float InvulnerableTime { get; set; }

        // Protected method access for the ability system
        void SetSpriteOffset(float x, float y);
        void DeactivateGun();
        void ActivateGun();
        void ChangeFrame();
        void TriggerBroSpecialEvent();
        void PlayAttackSound();
        void PlayAttackSound(float v);
        void SetGunSprite(int spriteFrame, int spriteRow);

        /// <summary>
        /// Called once at prefab creation time to set up fields that would otherwise be lost
        /// during the component swap. Runs before any Awake calls.
        /// </summary>
        void PrefabSetup();
    }
}
