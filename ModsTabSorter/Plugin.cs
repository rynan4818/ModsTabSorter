using HarmonyLib;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using ModsTabSorter.Installers;
using SiraUtil.Zenject;
using System.Reflection;
using IPALogger = IPA.Logging.Logger;

namespace ModsTabSorter
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        internal static Plugin Instance { get; private set; }
        internal static IPALogger Log { get; private set; }

        private static Harmony HarmonyInstance { get; set; }
        private const string HarmonyId = "com.github.rynan4818.ModsTabSorter";

        [Init]
        public void Init(IPALogger logger, Config conf, Zenjector zenjector)
        {
            Instance = this;
            Log = logger;
            Configuration.PluginConfig.Instance = conf.Generated<Configuration.PluginConfig>();

            Log.Info("ModsTabSorter initialized.");
            zenjector.Install<ModsTabSorterMenuInstaller>(Location.Menu);
        }

        [OnStart]
        public void OnApplicationStart()
        {
            Log.Debug("OnApplicationStart");
            HarmonyInstance = new Harmony(HarmonyId);
            HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
        }

        [OnExit]
        public void OnApplicationQuit()
        {
            Log.Debug("OnApplicationQuit");
            HarmonyInstance?.UnpatchSelf();
            HarmonyInstance = null;
        }
    }
}
