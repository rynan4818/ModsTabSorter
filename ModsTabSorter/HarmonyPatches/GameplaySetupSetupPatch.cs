using BeatSaberMarkupLanguage.GameplaySetup;
using HarmonyLib;
using ModsTabSorter.Controllers;

namespace ModsTabSorter.HarmonyPatches
{
    [HarmonyPatch(typeof(GameplaySetup), "Setup")]
    internal static class GameplaySetupSetupPatch
    {
        private static void Prefix(GameplaySetup __instance)
        {
            GameplayTabOrderService.ApplyConfiguredOrder(__instance);
        }
    }
}
