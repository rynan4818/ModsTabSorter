using BeatSaberMarkupLanguage;
using HMUI;
using Zenject;

namespace ModsTabSorter.UI
{
    internal class ModsTabSorterFlowCoordinator : FlowCoordinator
    {
        private ModsTabSorterListViewController _listViewController;

        [Inject]
        public void Construct(ModsTabSorterListViewController listViewController)
        {
            _listViewController = listViewController;
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            if (firstActivation)
            {
                SetTitle("Mods Tab Sorter");
                showBackButton = true;
                ProvideInitialViewControllers(_listViewController);
            }
        }

        protected override void BackButtonWasPressed(ViewController topViewController)
        {
            BeatSaberUI.MainFlowCoordinator.DismissFlowCoordinator(this);
            base.BackButtonWasPressed(topViewController);
        }
    }
}
