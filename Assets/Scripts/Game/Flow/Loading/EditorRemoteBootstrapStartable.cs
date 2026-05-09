using System;
using Common.AssetLoading.UnityAddressables;
using Cysharp.Threading.Tasks;
using Game.Level.Services;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer.Unity;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.Flow.Loading
{
    /// <summary>
    /// Editor-only bootstrap to force catalog/composite refresh when starting Play Mode from non-loading scenes.
    /// </summary>
    public class EditorRemoteBootstrapStartable : IStartable
    {
        private readonly CatalogUpdateService _CatalogUpdateService;
        private readonly RemoteCompositeLoader _RemoteCompositeLoader;
        private readonly ILogger _Logger;

        public EditorRemoteBootstrapStartable(
            CatalogUpdateService catalogUpdateService,
            RemoteCompositeLoader remoteCompositeLoader,
            ILogger logger)
        {
            _CatalogUpdateService = catalogUpdateService ?? throw new ArgumentNullException(nameof(catalogUpdateService));
            _RemoteCompositeLoader = remoteCompositeLoader ?? throw new ArgumentNullException(nameof(remoteCompositeLoader));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Start()
        {
#if UNITY_EDITOR
            if (!EditorRemoteBootstrapSettings.Enabled)
            {
                return;
            }

            string sceneName = SceneManager.GetActiveScene().name;
            if (string.Equals(sceneName, "Loading", StringComparison.OrdinalIgnoreCase))
            {
                _Logger.Log(LogType.Log, nameof(EditorRemoteBootstrapStartable), "Skip forced bootstrap in Loading scene.");
                return;
            }

            ForceBootstrapAsync(sceneName).Forget();
#endif
        }

#if UNITY_EDITOR
        private async UniTaskVoid ForceBootstrapAsync(string sceneName)
        {
            _Logger.Log(
                LogType.Log,
                nameof(EditorRemoteBootstrapStartable),
                $"Editor forced remote bootstrap enabled; scene={sceneName}. Running catalog update + remote composite load.");

            await _CatalogUpdateService.UpdateCatalogAsync();
            bool loaded = await _RemoteCompositeLoader.LoadAsync();
            _Logger.Log(
                LogType.Log,
                nameof(EditorRemoteBootstrapStartable),
                $"Editor forced remote bootstrap finished; remoteCompositeLoaded={loaded}.");
        }
#endif
    }

#if UNITY_EDITOR
    public static class EditorRemoteBootstrapSettings
    {
        private const string PrefKey = "KnifeToss.Editor.ForceRemoteBootstrap";

        public static bool Enabled
        {
            get => EditorPrefs.GetBool(PrefKey, false);
            set => EditorPrefs.SetBool(PrefKey, value);
        }
    }
#endif
}
