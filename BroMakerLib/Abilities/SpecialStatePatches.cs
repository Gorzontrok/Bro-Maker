using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using BroMakerLib.Loggers;
using HarmonyLib;

namespace BroMakerLib.Abilities
{
    internal static class SpecialStatePatches
    {
        internal static void Apply(Harmony harmony)
        {
            SpecialAbility.specialStateFailures = new List<string>();
            List<string> failures = SpecialAbility.specialStateFailures;
            List<KeyValuePair<MethodBase, MethodInfo>> patched = new List<KeyValuePair<MethodBase, MethodInfo>>();
            MethodInfo transpiler = AccessTools.Method(typeof(SpecialStatePatches), nameof(Transpiler));
            try
            {
                FieldInfo usingSpecialField = AccessTools.DeclaredField(typeof(TestVanDammeAnim), "usingSpecial");
                FieldInfo wasUsingSpecialField = AccessTools.DeclaredField(typeof(TestVanDammeAnim), "wasUsingSpecial");
                SpecialAbility.specialStateUsingSpecialRef = AccessTools.FieldRefAccess<TestVanDammeAnim, bool>("usingSpecial");
                if (usingSpecialField == null || wasUsingSpecialField == null)
                {
                    failures.Add("TestVanDammeAnim.usingSpecial or wasUsingSpecial not found");
                }

                // Type, method, then the expected `usingSpecial` loads and stores; copies are the loads into `wasUsingSpecial`, left alone.
                object[][] targets =
                {
                    new object[] { typeof(TestVanDammeAnim), "CheckFacingDirection", 1, 0, 0 },
                    new object[] { typeof(TestVanDammeAnim), "AnimateIdle", 2, 0, 0 },
                    new object[] { typeof(TestVanDammeAnim), "AnimateRunning", 1, 0, 0 },
                    new object[] { typeof(TestVanDammeAnim), "ChangeFrame", 3, 0, 0 },
                    new object[] { typeof(TestVanDammeAnim), "SetActionStateToJumping", 1, 0, 0 },
                    new object[] { typeof(TestVanDammeAnim), "LedgeGrapple", 1, 0, 0 },
                    new object[] { typeof(TestVanDammeAnim), "ConstrainToWalls", 4, 0, 0 },
                    new object[] { typeof(TestVanDammeAnim), "MustIgnoreHighFiveMeleePress", 1, 0, 0 },
                    new object[] { typeof(TestVanDammeAnim), "CheckInput", 1, 3, 1 },
                    new object[] { typeof(TestVanDammeAnim), "ClearFireInput", 0, 1, 0 },
                    new object[] { typeof(TestVanDammeAnim), "CoverInAcidRPC", 0, 1, 0 },
                    new object[] { typeof(TestVanDammeAnim), "AnimatePockettedSpecial", 0, 1, 0 },
                    new object[] { typeof(BroBase), "SetActionstateToIdle", 1, 0, 0 },
                    new object[] { typeof(BroBase), "AnimatePockettedSpecial", 0, 1, 0 }
                };

                List<MethodBase> methods = new List<MethodBase>();
                foreach (object[] target in targets)
                {
                    Type type = (Type)target[0];
                    string name = (string)target[1];
                    string key = type.Name + "." + name;
                    SpecialAbility.specialStateExpectedCounts[key] = new[] { (int)target[2], (int)target[3], (int)target[4] };
                    MethodBase method = AccessTools.DeclaredMethod(type, name);
                    if (method == null)
                    {
                        failures.Add(key + " not found");
                    }
                    methods.Add(method);
                }

                for (int i = 0; i < methods.Count && failures.Count == 0; i++)
                {
                    patched.Add(new KeyValuePair<MethodBase, MethodInfo>(methods[i], transpiler));
                    harmony.Patch(methods[i], transpiler: new HarmonyMethod(transpiler));
                }
            }
            catch (Exception ex)
            {
                failures.Add(ex.ToString());
            }

            if (failures.Count == 0)
            {
                SpecialAbility.SpecialStateActive = true;
                return;
            }

            foreach (KeyValuePair<MethodBase, MethodInfo> patch in patched)
            {
                try
                {
                    harmony.Unpatch(patch.Key, patch.Value);
                }
                catch (Exception ex)
                {
                    failures.Add("Unpatching " + patch.Key.Name + ": " + ex);
                }
            }
            BMLogger.Log("Special state patches not applied: " + string.Join("; ", failures.ToArray()));
            BMLogger.Error(
                "BroMaker couldn't adjust how the game handles specials, because this version of the game differs from the one BroMaker expects or another mod changes the same part of the game. A custom bro's special can still switch on effects of the bro it is built on, and that bro's own moves can cut it short.");
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            string key = original.DeclaringType.Name + "." + original.Name;
            int[] expected;
            if (!SpecialAbility.specialStateExpectedCounts.TryGetValue(key, out expected))
            {
                return codes;
            }

            FieldInfo usingSpecialField = AccessTools.DeclaredField(typeof(TestVanDammeAnim), "usingSpecial");
            FieldInfo wasUsingSpecialField = AccessTools.DeclaredField(typeof(TestVanDammeAnim), "wasUsingSpecial");
            Func<int, bool> isCopy = index => codes[index].LoadsField(usingSpecialField) && index + 1 < codes.Count && codes[index + 1].StoresField(wasUsingSpecialField);
            int reads = 0;
            int stores = 0;
            int copies = 0;
            for (int i = 0; i < codes.Count; i++)
            {
                if (isCopy(i))
                {
                    copies++;
                }
                else if (codes[i].LoadsField(usingSpecialField))
                {
                    reads++;
                }
                else if (codes[i].StoresField(usingSpecialField))
                {
                    stores++;
                }
            }
            if (reads != expected[0] || stores != expected[1] || copies != expected[2])
            {
                string mismatch = $"{key}: expected {expected[0]} reads, {expected[1]} stores and {expected[2]} copies of usingSpecial, found {reads}, {stores} and {copies}";
                SpecialAbility.specialStateFailures.Add(mismatch);
                if (SpecialAbility.SpecialStateActive)
                {
                    SpecialAbility.SpecialStateActive = false;
                    BMLogger.Log("Special state patch left out on a later patch pass: " + mismatch);
                    BMLogger.Error(
                        "Another mod changed how the game handles specials in a way BroMaker couldn't follow, so a custom bro's special can now switch on effects of the bro it is built on, and that bro's own moves can cut it short.");
                }
                return codes;
            }

            MethodInfo readMethod = AccessTools.Method(typeof(SpecialAbility), nameof(SpecialAbility.ReadUsingSpecial));
            MethodInfo storeMethod = AccessTools.Method(typeof(SpecialAbility), nameof(SpecialAbility.StoreUsingSpecial));
            List<CodeInstruction> result = new List<CodeInstruction>(codes.Count + (reads * 2));
            for (int i = 0; i < codes.Count; i++)
            {
                CodeInstruction code = codes[i];
                if (!isCopy(i) && code.LoadsField(usingSpecialField))
                {
                    // The unit stays on the stack for the read helper.
                    CodeInstruction dup = new CodeInstruction(OpCodes.Dup);
                    dup.MoveLabelsFrom(code);
                    dup.MoveBlocksFrom(code);
                    result.Add(dup);
                    result.Add(code);
                    result.Add(new CodeInstruction(OpCodes.Call, readMethod));
                }
                else if (code.StoresField(usingSpecialField))
                {
                    code.opcode = OpCodes.Call;
                    code.operand = storeMethod;
                    result.Add(code);
                }
                else
                {
                    result.Add(code);
                }
            }
            return result;
        }
    }
}
