using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.GameplaySetup;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static BeatSaberMarkupLanguage.Components.CustomListTableData;

namespace ModsTabSorter.Controllers
{
    internal static class GameplayTabOrderService
    {
        private static readonly BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private static readonly FieldInfo MenusField = typeof(GameplaySetup).GetField("menus", InstanceFlags);
        private static readonly FieldInfo ModsListField = typeof(GameplaySetup).GetField("modsList", InstanceFlags);
        private static readonly FieldInfo ModsTabField = typeof(GameplaySetup).GetField("modsTab", InstanceFlags);
        private static readonly FieldInfo TabsField = typeof(TabSelector).GetField("tabs", InstanceFlags);
        private static readonly Type GameplaySetupMenuType = typeof(GameplaySetup).Assembly.GetType("BeatSaberMarkupLanguage.GameplaySetup.GameplaySetupMenu");
        private static readonly FieldInfo GameplaySetupMenuNameField = GameplaySetupMenuType?.GetField("name", InstanceFlags);
        private static readonly PropertyInfo TabNameProperty = typeof(Tab).GetProperty(nameof(Tab.TabName), InstanceFlags);

        public static IReadOnlyList<string> GetCurrentTabNames()
        {
            return GetCurrentTabNames(GameplaySetup.instance);
        }

        public static IReadOnlyList<string> GetCurrentTabNames(GameplaySetup gameplaySetup)
        {
            try
            {
                IList menus = GetMenus(gameplaySetup);
                if (menus == null)
                    return Array.Empty<string>();

                return menus.Cast<object>()
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
            gameplaySetup = gameplaySetup ?? GameplaySetup.instance;
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
            gameplaySetup = gameplaySetup ?? GameplaySetup.instance;
            IList menus = GetMenus(gameplaySetup);
            if (menus == null || menus.Count == 0)
                return false;

            List<string> currentNames = menus.Cast<object>()
                .Select(GetMenuName)
                .Where(IsValidTabName)
                .ToList();
            if (currentNames.Count == 0)
                return false;

            List<string> effectiveOrder = BuildEffectiveOrder(currentNames, Configuration.PluginConfig.Instance?.OrderedTabs);
            bool menusChanged = ReorderMenuEntries(menus, effectiveOrder);
            bool tabsChanged = ReorderTabSelectorEntries(gameplaySetup, effectiveOrder);

            if (menusChanged || tabsChanged)
                RefreshGameplaySetupUi(gameplaySetup);

            Plugin.Log?.Debug(
                $"ApplyConfiguredOrder: current=[{string.Join(", ", currentNames)}], desired=[{string.Join(", ", effectiveOrder)}], menusChanged={menusChanged}, tabsChanged={tabsChanged}");

            return menusChanged || tabsChanged;
        }

        private static IList GetMenus(GameplaySetup gameplaySetup)
        {
            if (gameplaySetup == null || MenusField == null)
                return null;

            return MenusField.GetValue(gameplaySetup) as IList;
        }

        private static string GetMenuName(object menu)
        {
            if (menu == null || GameplaySetupMenuNameField == null)
                return null;

            return GameplaySetupMenuNameField.GetValue(menu) as string;
        }

        private static string GetTabName(object tab)
        {
            if (tab == null || TabNameProperty == null)
                return null;

            return TabNameProperty.GetValue(tab, null) as string;
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

        private static bool ReorderMenuEntries(IList menus, IReadOnlyList<string> desiredOrder)
        {
            List<object> currentEntries = menus.Cast<object>().ToList();
            List<string> currentOrder = currentEntries.Select(GetMenuName).Where(IsValidTabName).ToList();
            if (AreSameSequence(currentOrder, desiredOrder))
                return false;

            Dictionary<string, object> byName = currentEntries
                .Where(entry => IsValidTabName(GetMenuName(entry)))
                .GroupBy(GetMenuName, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);

            List<object> reorderedEntries = new List<object>(currentEntries.Count);
            foreach (string tabName in desiredOrder)
            {
                if (byName.TryGetValue(tabName, out object entry))
                    reorderedEntries.Add(entry);
            }

            foreach (object entry in currentEntries)
            {
                string tabName = GetMenuName(entry);
                if (!IsValidTabName(tabName) || !desiredOrder.Contains(tabName))
                    reorderedEntries.Add(entry);
            }

            menus.Clear();
            foreach (object entry in reorderedEntries)
                menus.Add(entry);

            return true;
        }

        private static bool ReorderTabSelectorEntries(GameplaySetup gameplaySetup, IReadOnlyList<string> desiredOrder)
        {
            if (gameplaySetup == null || ModsTabField == null || TabsField == null)
                return false;

            TabSelector selector = GetModsTabSelector(gameplaySetup);
            if (selector == null)
                return false;

            IList tabs = TabsField.GetValue(selector) as IList;
            if (tabs == null || tabs.Count == 0)
                return false;

            List<object> currentTabs = tabs.Cast<object>().ToList();
            List<string> currentOrder = currentTabs.Select(GetTabName).Where(IsValidTabName).ToList();
            List<string> selectorOrder = BuildEffectiveOrder(currentOrder, desiredOrder);
            if (AreSameSequence(currentOrder, selectorOrder))
                return false;

            Dictionary<string, object> byName = currentTabs
                .Where(entry => IsValidTabName(GetTabName(entry)))
                .GroupBy(GetTabName, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);

            List<object> reorderedTabs = new List<object>(currentTabs.Count);
            foreach (string tabName in selectorOrder)
            {
                if (byName.TryGetValue(tabName, out object entry))
                    reorderedTabs.Add(entry);
            }

            foreach (object entry in currentTabs)
            {
                string tabName = GetTabName(entry);
                if (!IsValidTabName(tabName) || !selectorOrder.Contains(tabName))
                    reorderedTabs.Add(entry);
            }

            tabs.Clear();
            foreach (object entry in reorderedTabs)
                tabs.Add(entry);

            return true;
        }

        private static void RefreshGameplaySetupUi(GameplaySetup gameplaySetup)
        {
            try
            {
                TabSelector selector = GetModsTabSelector(gameplaySetup);
                selector?.Refresh();

                CustomListTableData modsList = ModsListField?.GetValue(gameplaySetup) as CustomListTableData;
                if (modsList?.tableView != null)
                {
                    object tableView = modsList.tableView;
                    tableView.GetType().GetMethod("ReloadData", Type.EmptyTypes)?.Invoke(tableView, null);
                    tableView.GetType().GetMethod("RefreshContentSize", Type.EmptyTypes)?.Invoke(tableView, null);
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.Warn($"Failed to refresh gameplay setup UI after reordering: {ex.Message}");
            }
        }

        private static TabSelector GetModsTabSelector(GameplaySetup gameplaySetup)
        {
            Transform modsTab = ModsTabField?.GetValue(gameplaySetup) as Transform;
            if (modsTab == null)
                return null;

            return modsTab
                .GetComponentsInChildren<TabSelector>(true)
                .FirstOrDefault(selector => string.Equals(selector.tabTag, "mod-tab", StringComparison.Ordinal));
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
