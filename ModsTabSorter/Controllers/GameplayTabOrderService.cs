using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.GameplaySetup;
using BeatSaberMarkupLanguage.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using static BeatSaberMarkupLanguage.Components.CustomListTableData;

namespace ModsTabSorter.Controllers
{
    internal static class GameplayTabOrderService
    {
        public static IReadOnlyList<string> GetCurrentTabNames()
        {
            return GetCurrentTabNames(null);
        }

        public static IReadOnlyList<string> GetCurrentTabNames(GameplaySetup gameplaySetup)
        {
            try
            {
                SortedList<GameplaySetupMenu> menus = GetMenus(gameplaySetup);
                if (menus == null)
                    return Array.Empty<string>();

                return menus
                    .Select(GetMenuName)
                    .Where(IsValidTabName)
                    .ToList();
            }
            catch (Exception ex)
            {
                Plugin.Log?.Warn($"Failed to read current gameplay setup tabs: {ex.Message}");
                return Array.Empty<string>();
            }
        }

        public static List<CustomCellInfo> BuildDisplayItems(IEnumerable<string> tabNames)
        {
            return tabNames
                .Where(IsValidTabName)
                .Select((name, index) => new CustomCellInfo($"{index + 1}. {name}"))
                .ToList();
        }

        public static IReadOnlyList<string> GetEffectiveTabNames(GameplaySetup gameplaySetup = null)
        {
            gameplaySetup = ResolveGameplaySetup(gameplaySetup);
            return BuildEffectiveOrder(GetCurrentTabNames(gameplaySetup), Configuration.PluginConfig.Instance?.OrderedTabs);
        }

        public static void ApplyUserOrder(IEnumerable<string> orderedTabs)
        {
            SaveOrder(orderedTabs);
            bool changed = ApplyConfiguredOrder();
            Plugin.Log?.Debug($"ApplyUserOrder: runtime reorder changed={changed}");
        }

        public static bool ApplyConfiguredOrder(GameplaySetup gameplaySetup = null)
        {
            gameplaySetup = ResolveGameplaySetup(gameplaySetup);
            SortedList<GameplaySetupMenu> menus = GetMenus(gameplaySetup);
            if (menus == null || menus.Count == 0)
                return false;

            List<string> currentNames = menus
                .Select(GetMenuName)
                .Where(IsValidTabName)
                .ToList();
            if (currentNames.Count == 0)
                return false;

            List<string> effectiveOrder = BuildEffectiveOrder(currentNames, Configuration.PluginConfig.Instance?.OrderedTabs);
            bool menusChanged = ReorderMenuEntries(gameplaySetup, effectiveOrder);

            if (menusChanged)
                RefreshGameplaySetupUi(gameplaySetup);

            Plugin.Log?.Debug(
                $"ApplyConfiguredOrder: current=[{string.Join(", ", currentNames)}], desired=[{string.Join(", ", effectiveOrder)}], menusChanged={menusChanged}");

            return menusChanged;
        }

        private static GameplaySetup ResolveGameplaySetup(GameplaySetup gameplaySetup)
        {
            if (gameplaySetup != null)
                return gameplaySetup;

            try
            {
                return GameplaySetup.Instance;
            }
            catch (Exception ex)
            {
                Plugin.Log?.Warn($"Failed to resolve GameplaySetup instance: {ex.Message}");
                return null;
            }
        }

        private static SortedList<GameplaySetupMenu> GetMenus(GameplaySetup gameplaySetup)
        {
            gameplaySetup = ResolveGameplaySetup(gameplaySetup);
            if (gameplaySetup == null)
                return null;

            return gameplaySetup.menus;
        }

        private static string GetMenuName(GameplaySetupMenu menu)
        {
            if (menu == null)
                return null;

            return menu.Name;
        }

        private static List<string> BuildEffectiveOrder(IEnumerable<string> currentTabs, IEnumerable<string> savedTabs)
        {
            List<string> current = SanitizeTabNames(currentTabs);
            List<string> saved = SanitizeTabNames(savedTabs);
            HashSet<string> currentSet = new HashSet<string>(current, StringComparer.Ordinal);
            HashSet<string> effectiveSet = new HashSet<string>(StringComparer.Ordinal);
            List<string> effective = new List<string>(current.Count);

            foreach (string tabName in saved)
            {
                if (currentSet.Contains(tabName) && effectiveSet.Add(tabName))
                    effective.Add(tabName);
            }

            foreach (string tabName in current)
            {
                if (effectiveSet.Add(tabName))
                    effective.Add(tabName);
            }

            return effective;
        }

        private static List<string> SanitizeTabNames(IEnumerable<string> tabNames)
        {
            List<string> sanitized = new List<string>();
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);

            if (tabNames == null)
                return sanitized;

            foreach (string tabName in tabNames)
            {
                if (!IsValidTabName(tabName))
                    continue;

                if (seen.Add(tabName))
                    sanitized.Add(tabName);
            }

            return sanitized;
        }

        private static void SaveOrder(IEnumerable<string> orderedTabs)
        {
            var config = Configuration.PluginConfig.Instance;
            if (config == null)
                return;

            List<string> sanitized = SanitizeTabNames(orderedTabs);
            if (AreSameSequence(config.OrderedTabs, sanitized))
                return;

            config.OrderedTabs = sanitized;
            config.Changed();
            Plugin.Log?.Debug($"SaveOrder: [{string.Join(", ", sanitized)}]");
        }

        private static bool ReorderMenuEntries(GameplaySetup gameplaySetup, IReadOnlyList<string> desiredOrder)
        {
            SortedList<GameplaySetupMenu> menus = GetMenus(gameplaySetup);
            if (menus == null || menus.Count == 0)
                return false;

            List<GameplaySetupMenu> currentEntries = menus.ToList();
            List<string> currentOrder = currentEntries.Select(GetMenuName).Where(IsValidTabName).ToList();
            if (AreSameSequence(currentOrder, desiredOrder))
                return false;

            SortedList<GameplaySetupMenu> reorderedMenus = new SortedList<GameplaySetupMenu>(CreateMenuComparer(currentEntries, desiredOrder));
            foreach (GameplaySetupMenu entry in currentEntries)
            {
                reorderedMenus.Add(entry);
            }

            gameplaySetup.menus = reorderedMenus;
            return true;
        }

        private static IComparer<GameplaySetupMenu> CreateMenuComparer(
            IReadOnlyList<GameplaySetupMenu> currentEntries,
            IReadOnlyList<string> desiredOrder)
        {
            Dictionary<string, int> desiredIndexLookup = new Dictionary<string, int>(StringComparer.Ordinal);
            for (int i = 0; i < desiredOrder.Count; i++)
            {
                string tabName = desiredOrder[i];
                if (IsValidTabName(tabName) && !desiredIndexLookup.ContainsKey(tabName))
                {
                    desiredIndexLookup.Add(tabName, i);
                }
            }

            Dictionary<string, int> currentIndexLookup = new Dictionary<string, int>(StringComparer.Ordinal);
            for (int i = 0; i < currentEntries.Count; i++)
            {
                string tabName = GetMenuName(currentEntries[i]);
                if (IsValidTabName(tabName) && !currentIndexLookup.ContainsKey(tabName))
                {
                    currentIndexLookup.Add(tabName, i);
                }
            }

            return Comparer<GameplaySetupMenu>.Create((left, right) =>
            {
                string leftName = GetMenuName(left);
                string rightName = GetMenuName(right);

                int leftDesiredIndex = GetOrderIndex(desiredIndexLookup, leftName);
                int rightDesiredIndex = GetOrderIndex(desiredIndexLookup, rightName);
                int comparison = leftDesiredIndex.CompareTo(rightDesiredIndex);
                if (comparison != 0)
                    return comparison;

                int leftCurrentIndex = GetOrderIndex(currentIndexLookup, leftName);
                int rightCurrentIndex = GetOrderIndex(currentIndexLookup, rightName);
                comparison = leftCurrentIndex.CompareTo(rightCurrentIndex);
                if (comparison != 0)
                    return comparison;

                return StringComparer.Ordinal.Compare(leftName, rightName);
            });
        }

        private static int GetOrderIndex(IReadOnlyDictionary<string, int> lookup, string tabName)
        {
            if (lookup == null || !IsValidTabName(tabName))
            {
                return int.MaxValue;
            }

            int orderIndex;
            if (lookup.TryGetValue(tabName, out orderIndex))
            {
                return orderIndex;
            }

            return int.MaxValue;
        }

        private static void RefreshGameplaySetupUi(GameplaySetup gameplaySetup)
        {
            try
            {
                if (gameplaySetup?.rootObject != null)
                {
                    gameplaySetup.QueueRefreshView();
                    return;
                }

                CustomListTableData modsList = gameplaySetup?.modsList;
                if (modsList?.TableView != null)
                {
                    modsList.TableView.ReloadData();
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.Warn($"Failed to refresh gameplay setup UI after reordering: {ex.Message}");
            }
        }

        private static bool AreSameSequence(IEnumerable<string> left, IEnumerable<string> right)
        {
            List<string> leftList = left?.ToList() ?? new List<string>();
            List<string> rightList = right?.ToList() ?? new List<string>();

            if (leftList.Count != rightList.Count)
                return false;

            for (int i = 0; i < leftList.Count; i++)
            {
                if (!string.Equals(leftList[i], rightList[i], StringComparison.Ordinal))
                    return false;
            }

            return true;
        }

        private static bool IsValidTabName(string tabName)
        {
            return !string.IsNullOrWhiteSpace(tabName);
        }
    }
}
