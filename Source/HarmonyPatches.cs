using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace EdB.PrepareCarefully.HarmonyPatches;

[StaticConstructorOnStartup]
internal static class Main {
    static Main() {
        var harmony = new Harmony("EdB.PrepareCarefully");
        harmony.PatchAll(Assembly.GetExecutingAssembly());

        var patchedMethods = new HashSet<ValueTuple<Type, string>>();
        foreach (var m in harmony.GetPatchedMethods()) {
            patchedMethods.Add((m.DeclaringType, m.Name));
        }

        if (patchedMethods.Count != 3
            || !patchedMethods.Contains((typeof(Page_ConfigureStartingPawns), "PreOpen"))
            || !patchedMethods.Contains((typeof(Page_ConfigureStartingPawns), "DoWindowContents"))
            || !patchedMethods.Contains((typeof(Game), "InitNewGame"))
           ) {
            var methodsMessage = String.Join(", ",
                harmony.GetPatchedMethods().Select(i => i.DeclaringType + "." + i.Name));
            Logger.Warning(
                "Did not patch all of the expected methods.  The following patched methods were found: "
                + (!methodsMessage.NullOrEmpty() ? methodsMessage : "none"));
        }
    }
}

[HarmonyPatch(typeof(Page_ConfigureStartingPawns))]
[HarmonyPatch("PreOpen")]
[HarmonyPatch(new Type[0])]
internal class ClearOriginalScenarioPatch {
    [HarmonyPostfix]
    private static void Postfix() {
        PrepareCarefully.ClearVanillaFriendlyScenario();
    }
}

[HarmonyPatch(typeof(Game))]
[HarmonyPatch("InitNewGame")]
[HarmonyPatch(new Type[0])]
internal class ReplaceScenarioPatch {
    [HarmonyPostfix]
    private static void Postfix() {
        // After we've initialized the new game, swap in the vanilla-friendly version of the scenario so that the game save
        // doesn't include any Prepare Carefully-specific scene parts.
        if (PrepareCarefully.VanillaFriendlyScenario != null) {
            Current.Game.Scenario = PrepareCarefully.VanillaFriendlyScenario;
            PrepareCarefully.ClearVanillaFriendlyScenario();
        }
    }
}

[HarmonyPatch(typeof(Page_ConfigureStartingPawns))]
[HarmonyPatch("DoWindowContents")]
[HarmonyPatch(new[] { typeof(Rect) })]
internal class PrepareCarefullyButtonPatch {
    private static void Postfix(Page_ConfigureStartingPawns __instance, ref Rect rect) {
        var BottomButSize = new Vector2(150f, 38f);
        var num = rect.height + 45f;
        var rect4 = new Rect(rect.x + (rect.width / 2f) - (BottomButSize.x / 2f), num, BottomButSize.x,
            BottomButSize.y);
        if (Widgets.ButtonText(rect4, "EdB.PC.Page.Button.PrepareCarefully".Translate(), true, false)) {
            // Version check
            if (VersionControl.CurrentVersion < PrepareCarefully.MinimumGameVersion) {
                Find.WindowStack.Add(new DialogInitializationError(null));
                SoundDefOf.ClickReject.PlayOneShot(null);
                Logger.Warning("Prepare Carefully failed to initialize because it requires at least version " +
                               PrepareCarefully.MinimumGameVersion
                               + " of RimWorld.  You are running " + VersionControl.CurrentVersionString);
                return;
            }

            try {
                ReflectionCache.Instance.Initialize();

                var prepareCarefully = PrepareCarefully.Instance;
                if (prepareCarefully == null) {
                    Logger.Error(
                        "Could not open Prepare Carefully screen, because we failed to get the Prepare Carefully singleton.");
                    return;
                }

                prepareCarefully.Initialize();
                prepareCarefully.OriginalPage = __instance;
                var page = new Page_PrepareCarefully();

                var state = prepareCarefully.State;
                if (state == null) {
                    Logger.Error(
                        "Could not open Prepare Carefully screen, because the Prepare Carefully state was null.");
                    return;
                }

                state.Page = page;
                Find.WindowStack.Add(page);
            }
            catch (Exception e) {
                Find.WindowStack.Add(new DialogInitializationError(e));
                SoundDefOf.ClickReject.PlayOneShot(null);
                throw new InitializationException("Prepare Carefully failed to initialize", e);
            }
        }
    }
}
