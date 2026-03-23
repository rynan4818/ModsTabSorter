using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.MenuButtons;
using System;
using Zenject;

namespace ModsTabSorter.UI
{
    internal class ModsTabSorterMenuButtonController : IInitializable, IDisposable
    {
        private readonly ModsTabSorterFlowCoordinator _flowCoordinator;
        private MenuButton _menuButton;

        [Inject]
        public ModsTabSorterMenuButtonController(ModsTabSorterFlowCoordinator flowCoordinator)
        {
            _flowCoordinator = flowCoordinator;
        }

        public void Initialize()
        {
            _menuButton = new MenuButton("Mods Tab Sorter", "Open the Mods Tab Sorter.", ShowFlowCoordinator);
            MenuButtons.instance?.RegisterButton(_menuButton);
        }

        public void Dispose()
        {
            if (_menuButton != null)
            {
                MenuButtons.instance?.UnregisterButton(_menuButton);
                _menuButton = null;
            }
        }

        private void ShowFlowCoordinator()
        {
            BeatSaberUI.MainFlowCoordinator.PresentFlowCoordinator(_flowCoordinator);
        }
    }
}
