using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using ModsTabSorter.Controllers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static BeatSaberMarkupLanguage.Components.CustomListTableData;

namespace ModsTabSorter.UI
{
    [HotReload]
    internal class ModsTabSorterListViewController : BSMLAutomaticViewController
    {
        private List<string> _tabNames = new List<string>();
        private List<CustomCellInfo> _tabItems = new List<CustomCellInfo>();
        private int _selectedIndex = -1;
        private string _statusText = "Loading...";

        [UIComponent("tab-list")]
        private CustomListTableData _tabList = null;

        [UIValue("tab-items")]
        public List<CustomCellInfo> TabItems => _tabItems;

        [UIValue("status-text")]
        public string StatusText
        {
            get => _statusText;
            private set
            {
                if (_statusText == value)
                    return;

                _statusText = value;
                NotifyPropertyChanged(nameof(StatusText));
            }
        }

        [UIValue("selected-tab-text")]
        public string SelectedTabText =>
            _selectedIndex >= 0 && _selectedIndex < _tabNames.Count
                ? $"Selected: {_tabNames[_selectedIndex]}"
                : "Selected: None";

        [UIValue("can-move-up")]
        public bool CanMoveUp => _selectedIndex > 0 && _selectedIndex < _tabNames.Count;

        [UIValue("can-move-down")]
        public bool CanMoveDown => _selectedIndex >= 0 && _selectedIndex < _tabNames.Count - 1;

        public override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);

            if (firstActivation)
            {
                rectTransform.sizeDelta = new Vector2(120f, 0f);
                rectTransform.anchorMin = new Vector2(0.5f, 0f);
                rectTransform.anchorMax = new Vector2(0.5f, 1f);
            }

            ReloadTabs(null, null);
        }

        [UIAction("tab-selected")]
        private void TabSelected(TableView tableView, int index)
        {
            _selectedIndex = index;
            RefreshSelectionState();
        }

        [UIAction("move-up")]
        private void MoveUp()
        {
            if (!CanMoveUp)
                return;

            string selectedTabName = _tabNames[_selectedIndex];
            Swap(_selectedIndex, _selectedIndex - 1);
            _selectedIndex--;
            GameplayTabOrderService.ApplyUserOrder(_tabNames);
            ReloadTabs(selectedTabName, $"Moved \"{selectedTabName}\" up.");
        }

        [UIAction("move-down")]
        private void MoveDown()
        {
            if (!CanMoveDown)
                return;

            string selectedTabName = _tabNames[_selectedIndex];
            Swap(_selectedIndex, _selectedIndex + 1);
            _selectedIndex++;
            GameplayTabOrderService.ApplyUserOrder(_tabNames);
            ReloadTabs(selectedTabName, $"Moved \"{selectedTabName}\" down.");
        }

        [UIAction("reload-tabs")]
        private void ReloadTabsAction()
        {
            ReloadTabs(GetSelectedTabName(), "Reloaded the current Gameplay Setup tab list.");
        }

        private void ReloadTabs(string preferredTabName, string statusOverride)
        {
            GameplayTabOrderService.ApplyConfiguredOrder();

            _tabNames = GameplayTabOrderService.GetEffectiveTabNames().ToList();
            _tabItems = GameplayTabOrderService.BuildDisplayItems(_tabNames);

            if (_tabNames.Count == 0)
            {
                _selectedIndex = -1;
            }
            else if (!string.IsNullOrEmpty(preferredTabName) && _tabNames.Contains(preferredTabName))
            {
                _selectedIndex = _tabNames.IndexOf(preferredTabName);
            }
            else if (_selectedIndex < 0 || _selectedIndex >= _tabNames.Count)
            {
                _selectedIndex = 0;
            }

            NotifyPropertyChanged(nameof(TabItems));

            if (_tabList != null)
            {
                _tabList.data = _tabItems;
                _tabList.tableView?.ReloadData();
                if (_selectedIndex >= 0)
                    _tabList.tableView?.SelectCellWithIdx(_selectedIndex, false);
            }

            StatusText = statusOverride ?? (
                _tabNames.Count == 0
                    ? "No Gameplay Setup mod tabs are currently registered."
                    : "Select a tab and use Move Up / Move Down to change the order.");

            RefreshSelectionState();
        }

        private string GetSelectedTabName()
        {
            return _selectedIndex >= 0 && _selectedIndex < _tabNames.Count
                ? _tabNames[_selectedIndex]
                : null;
        }

        private void RefreshSelectionState()
        {
            NotifyPropertyChanged(nameof(SelectedTabText));
            NotifyPropertyChanged(nameof(CanMoveUp));
            NotifyPropertyChanged(nameof(CanMoveDown));
        }

        private void Swap(int leftIndex, int rightIndex)
        {
            string temp = _tabNames[leftIndex];
            _tabNames[leftIndex] = _tabNames[rightIndex];
            _tabNames[rightIndex] = temp;
        }

    }
}
