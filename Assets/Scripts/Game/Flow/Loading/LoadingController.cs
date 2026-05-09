using System;
using Common.AssetLoading.UnityAddressables;
using Cysharp.Threading.Tasks;
using Game.Flow.Game;
using Game.Level.Catalogs;
using Game.Level.Services;
using UnityEngine;
using VContainer.Unity;

namespace Game.Flow.Loading
{
    /// <summary>
    /// Controls the loading screen flow. Transitions to MainMenu after global services are initialized.
    /// </summary>
    public class LoadingController : IStartable
    {
        private readonly SceneLoaderService _SceneLoaderService;
        private readonly CatalogUpdateService _CatalogUpdateService;
        private readonly RemoteCompositeLoader _RemoteCompositeLoader;
        private readonly ILogger _Logger;

        /// <summary>
        /// Creates the loading controller with scene navigation and Addressables bootstrap dependencies.
        /// </summary>
        /// <param name="sceneLoaderService">Loads scenes after bootstrap.</param>
        /// <param name="catalogUpdateService">Updates Addressables catalogs from remote when configured.</param>
        /// <param name="remoteCompositeLoader">Loads the worlds manifest composite when Addressables resolves it.</param>
        /// <param name="logger">Application logger.</param>
        public LoadingController(
            SceneLoaderService sceneLoaderService,
            CatalogUpdateService catalogUpdateService,
            RemoteCompositeLoader remoteCompositeLoader,
            ILogger logger)
        {
            _SceneLoaderService = sceneLoaderService;
            _CatalogUpdateService = catalogUpdateService;
            _RemoteCompositeLoader = remoteCompositeLoader;
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public void Start()
        {
            BootstrapAsync().Forget();
        }

        private async UniTask BootstrapAsync()
        {
            await UniTask.Yield();
            _Logger.Log(LogType.Log, nameof(LoadingController), "Bootstrap start: checking catalogs then loading remote composite.");
            await _CatalogUpdateService.UpdateCatalogAsync();

            bool compositeLeaseOk = await _RemoteCompositeLoader.LoadAsync();
            if (!compositeLeaseOk)
            {
                _Logger.LogWarning(
                    nameof(LoadingController),
                    "StagesCollectionComposite Addressables load failed or produced no asset. " +
                    "IStagesCollectionCatalog will use the GameInstaller serialized fallback composite until a remote lease succeeds.");
            }
            else
            {
                _Logger.Log(
                    LogType.Log,
                    nameof(LoadingController),
                    "Bootstrap: StagesCollectionComposite loaded via Addressables; catalog provider uses remote lease.");
            }

            StagesCollectionComposite activeRemote = _RemoteCompositeLoader.Composite;
            _Logger.Log(
                LogType.Log,
                nameof(LoadingController),
                $"Bootstrap summary: remoteCompositeLoaded={compositeLeaseOk}; remoteCollectionsCount={(activeRemote != null ? activeRemote.CollectionsCount : 0)}");

            await _SceneLoaderService.LoadMainMenu();
        }
    }
}
