using System;
using System.Reflection.Emit;
using BroMakerLib.CustomObjects;
using BroMakerLib.Vanilla.Specials;
using HarmonyLib;
using RocketLib.Extensions;
using UnityEngine;

namespace BroMakerLib.Abilities
{
    public static class AbilityPatches
    {
        // Uses IL emit to bypass the type system and set BoondockBro.leadingBro (typed BoondockBro)
        // to a TestVanDammeAnim reference. All companion AI methods only access base-class fields
        // on leadingBro, so this is safe. Methods that access BoondockBro-specific members
        // (ReduceLives, SwitchToLeadBro) are guarded with clear/restore prefix/postfix patches.
        private static readonly Action<BoondockBro, TestVanDammeAnim> _setLeadingBro;

        static AbilityPatches()
        {
            var dm = new DynamicMethod(
                "SetLeadingBro",
                null,
                new[] { typeof(BoondockBro), typeof(TestVanDammeAnim) },
                typeof(BoondockBro).Module,
                true);
            var il = dm.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stfld, typeof(BoondockBro).GetField("leadingBro"));
            il.Emit(OpCodes.Ret);
            _setLeadingBro = (Action<BoondockBro, TestVanDammeAnim>)dm.CreateDelegate(
                typeof(Action<BoondockBro, TestVanDammeAnim>));
        }

        internal static void SetBoondockLeadingBro(BoondockBro companion, TestVanDammeAnim owner)
        {
            _setLeadingBro(companion, owner);
        }

        #region Boomerang/Chakram patches

        [HarmonyPatch(typeof(Boomerang), "ReturnBoomerang")]
        static class Boomerang_ReturnBoomerang_Patch
        {
            static void Prefix(Boomerang __instance)
            {
                if (__instance.firedBy == null || __instance.firedBy is BroMax)
                {
                    return;
                }

                var customHero = __instance.firedBy as ICustomHero;
                if (customHero == null)
                {
                    return;
                }

                if (customHero.SpecialAbility is BroMaxSpecial broMaxSpecial)
                {
                    broMaxSpecial.ReturnBoomerang(__instance);
                }
            }
        }

        [HarmonyPatch(typeof(Chakram), "RunChakramCatch")]
        static class Chakram_RunChakramCatch_Patch
        {
            static bool Prefix(Chakram __instance)
            {
                if (__instance.firedBy == null || __instance.firedBy is Xebro)
                {
                    return true;
                }

                var customHero = __instance.firedBy as ICustomHero;
                if (customHero == null)
                {
                    return false;
                }

                if (!__instance.GetFieldValue<bool>("canBeCaught"))
                {
                    return false;
                }

                if (!(customHero.SpecialAbility is XebroSpecial xebroSpecial))
                {
                    return false;
                }

                var hero = __instance.firedBy as TestVanDammeAnim;
                float num = hero.X - __instance.X;
                float num2 = hero.Y + 10f - __instance.Y;
                if (Mathf.Abs(num) < 9f && Mathf.Abs(num2) < 14f)
                {
                    xebroSpecial.CatchChakram(__instance);
                    Sound.GetInstance().PlaySoundEffectAt(__instance.soundHolder.special3Sounds, 0.5f, __instance.transform.position, 1f, true, false, false, 0f);
                    __instance.SetFieldValue("hasPlayedReturnSwooshSound", false);
                    UnityEngine.Object.Destroy(__instance.gameObject);
                }

                return false;
            }
        }

        #endregion

        #region BoondockBro companion patches

        // When the ability owner isn't a BoondockBro, the RPC passes null for LeadingBro.
        // Force-set it to the player's actual character so the companion AI works.
        [HarmonyPatch(typeof(BoondockBro), "SetUpTrailingBro")]
        static class BoondockBro_SetUpTrailingBro_Patch
        {
            static void Postfix(BoondockBro __instance, BoondockBro LeadingBro, int PlayerNum)
            {
                if (LeadingBro != null) return;
                var player = HeroController.players[PlayerNum];
                if (player != null && player.character != null)
                {
                    SetBoondockLeadingBro(__instance, player.character);
                }
            }
        }

        [HarmonyPatch(typeof(BoondockBro), "SetUpConnollyBro")]
        static class BoondockBro_SetUpConnollyBro_Patch
        {
            static void Postfix(BoondockBro ConnollyBro, BoondockBro LeadingBro, int PlayerNum)
            {
                if (LeadingBro != null) return;
                var player = HeroController.players[PlayerNum];
                if (player != null && player.character != null)
                {
                    SetBoondockLeadingBro(ConnollyBro, player.character);
                }
            }
        }

        // Guard patches: skip the original method when leadingBro is force-set to a
        // non-BoondockBro, and replicate only the safe parts of the vanilla logic.
        // We can't use clear/restore because clearing leadingBro to null causes
        // ReduceLives to enter the wrong branch (base.ReduceLives → life loss).
        [HarmonyPatch(typeof(BoondockBro), "ReduceLives")]
        static class BoondockBro_ReduceLives_Patch
        {
            static bool Prefix(BoondockBro __instance)
            {
                if (__instance.isLeadBro || __instance.leadingBro == null ||
                    (object)__instance.leadingBro is BoondockBro)
                {
                    return true;
                }
                // Non-lead bro with force-set leadingBro: replicate vanilla non-lead branch
                // without accessing BoondockBro-specific fields (trailingBro, SetEnraged)
                if (__instance.connollyBro != null)
                {
                    __instance.connollyBro.SetEnraged(true);
                }
                if (__instance.IsMine)
                {
                    HeroController.SetAvatarDead(__instance.playerNum, __instance.usePrimaryAvatar);
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(BillyConnolly), "ReduceLives")]
        static class BillyConnolly_ReduceLives_Patch
        {
            static bool Prefix(BillyConnolly __instance)
            {
                if (__instance.leadingBro == null || (object)__instance.leadingBro is BoondockBro)
                {
                    return true;
                }
                // Vanilla BillyConnolly.ReduceLives only enrages leadingBro and trailingBro —
                // both BoondockBro-specific. Skip entirely for non-BoondockBro leaders.
                return false;
            }
        }

        [HarmonyPatch(typeof(BoondockBro), "SwitchToLeadBro")]
        static class BoondockBro_SwitchToLeadBro_Patch
        {
            static bool Prefix(BoondockBro __instance)
            {
                if (__instance.leadingBro != null && !((object)__instance.leadingBro is BoondockBro))
                {
                    // Can't access leadingBro.isLeadBro — just promote this bro
                    __instance.isLeadBro = true;
                    return false;
                }
                return true;
            }
        }

        #endregion
    }
}
