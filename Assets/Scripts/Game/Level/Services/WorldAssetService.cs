using System;
using System.Threading;
using Common.AssetLoading;
using Cysharp.Threading.Tasks;
using Game.Level.Catalogs;
using Game.Level.Interfaces;
using UnityEngine;

namespace Game.Level.Services
{
    /// <summary>
    /// Loads and releases the active <see cref="StagesCollection"/> for the current session world via Addressables.
    /// </summary>
    public class WorldAssetService : IDisposable
    {
        private const int NoCollectionLoadedIndex = -1;

        private readonly IStagesCollectionCatalog _StagesCatalog;
        private readonly IAssetLoader _AssetLoader;
        private readonly ILogger _Logger;
        private ILoadedAsset<StagesCollection> _ActiveLease;
        private int _ActiveCollectionIndex = NoCollectionLoadedIndex;

        /// <summary>
        /// Creates the service with composite metadata, the shared asset loader, and logging.
        /// </summary>
        /// <param name="stagesCatalog">World manifest metadata.</param>
        /// <param name="assetLoader">Shared addressable loader.</param>
        /// <param name="logger">Application logger instance.</param>
        public WorldAssetService(IStagesCollectionCatalog stagesCatalog, IAssetLoader assetLoader, ILogger logger)
        {
            _StagesCatalog = stagesCatalog;
            _AssetLoader = assetLoader;
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Currently addressable-loaded world asset, or null between loads or after dispose.
        /// </summary>
        public StagesCollection ActiveCollection =>
            _ActiveLease != null && _ActiveLease.IsValid ? _ActiveLease.Asset : null;

        /// <summary>
        /// Index of the loaded world, or <see cref="NoCollectionLoadedIndex"/> when none.
        /// </summary>
        public int ActiveCollectionIndex => _ActiveCollectionIndex;

        /// <summary>
        /// Loads the world at the given index, releasing any prior lease.
        /// </summary>
        /// <returns>True when a collection is loaded and active; false if configuration is missing or loading failed.</returns>
        public async UniTask<bool> LoadCollectionAsync(int collectionIndex, CancellationToken cancellationToken = default)
        {
            if (collectionIndex == _ActiveCollectionIndex && _ActiveLease != null && _ActiveLease.IsValid)
            {
                return true;
            }

            string locationKey =
                _StagesCatalog != null ? _StagesCatalog.GetCollectionLocationKey(collectionIndex) : null;
            if (string.IsNullOrEmpty(locationKey))
            {
                _Logger.LogError(
                    nameof(WorldAssetService),
                    $"No Addressables location for world index {collectionIndex}.");
                return false;
            }

            _ActiveLease?.Dispose();
            _ActiveLease = null;
            _ActiveCollectionIndex = NoCollectionLoadedIndex;

            ILoadedAsset<StagesCollection> lease = await _AssetLoader.LoadAsync<StagesCollection>(locationKey, cancellationToken);
            if (lease == null)
            {
                return false;
            }

            _ActiveLease = lease;
            _ActiveCollectionIndex = collectionIndex;
            return true;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _ActiveLease?.Dispose();
            _ActiveLease = null;
            _ActiveCollectionIndex = NoCollectionLoadedIndex;
        }
    }
}
