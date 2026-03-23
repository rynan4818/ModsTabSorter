using BeatSaberMarkupLanguage.GameplaySetup;
using HarmonyLib;
using ModsTabSorter.Controllers;
using System.Reflection;

namespace ModsTabSorter.HarmonyPatches
{
    [HarmonyPatch]
    internal static class GameplaySetupAddTabPatch
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(
                typeof(GameplaySetup),
                "AddTab",
                new[]
                {
                    typeof(Assembly),
                    typeof(string),
                    typeof(string),
                    typeof(object),
                    typeof(MenuType)
                });
        }

        private static void Postfix(GameplaySetup __instance)
        {
            GameplayTabOrderService.ApplyConfiguredOrder(__instance);
        }
    }
}
