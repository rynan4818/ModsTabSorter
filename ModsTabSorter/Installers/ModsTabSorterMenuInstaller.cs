using ModsTabSorter.UI;
using Zenject;

namespace ModsTabSorter.Installers
{
    public class ModsTabSorterMenuInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<ModsTabSorterListViewController>().FromNewComponentAsViewController().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<ModsTabSorterFlowCoordinator>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<ModsTabSorterMenuButtonController>().AsSingle().NonLazy();
        }
    }
}
