using System;
using System.Threading;
using Common.AssetLoading;
using Cysharp.Threading.Tasks;
using Game.Level.Catalogs;
using UnityEngine;

namespace Game.Level.Services
{
    /// <summary>
    /// Loads <see cref="StagesCollectionComposite"/> from Addressables using an assigned reference and holds the lease.
    /// </summary>
    public class RemoteCompositeLoader : IDisposable
    {
        private readonly IAssetLoader _AssetLoader;
        private readonly ILogger _Logger;
        private readonly AssetReferenceStagesCollectionComposite _CompositeReference;
        private ILoadedAsset<StagesCollectionComposite> _Lease;

        /// <summary>
        /// Creates the loader with the shared asset loader, logging, and composite Addressables reference.
        /// </summary>
        /// <param name="assetLoader">Addressables-backed loader.</param>
        /// <param name="logger">Application logger.</param>
        /// <param name="compositeReference">Reference whose runtime key resolves the remote composite entry.</param>
        public RemoteCompositeLoader(
            IAssetLoader assetLoader,
            ILogger logger,
            AssetReferenceStagesCollectionComposite compositeReference)
        {
            _AssetLoader = assetLoader ?? throw new ArgumentNullException(nameof(assetLoader));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _CompositeReference = compositeReference ?? throw new ArgumentNullException(nameof(compositeReference));
        }

        /// <summary>
        /// Loaded composite when successful; otherwise null before load or after failure.
        /// </summary>
        public StagesCollectionComposite Composite =>
            _Lease != null && _Lease.IsValid ? _Lease.Asset : null;

        /// <summary>
        /// Loads the composite from Addressables; replaces any prior lease on success.
        /// </summary>
        /// <param name="cancellationToken">Cancellation for the load.</param>
        /// <returns>True when a composite is active after the call.</returns>
        public async UniTask<bool> LoadAsync(CancellationToken cancellationToken = default)
        {
            _Lease?.Dispose();
            _Lease = null;

            if (!_CompositeReference.RuntimeKeyIsValid())
            {
                _Logger.LogError(nameof(RemoteCompositeLoader), "Remote composite AssetReference has no valid runtime key.");
                return false;
            }

            object runtimeKeyObject = _CompositeReference.RuntimeKey;
            string locationKey = runtimeKeyObject as string ?? runtimeKeyObject?.ToString();
            if (string.IsNullOrEmpty(locationKey))
            {
                _Logger.LogError(nameof(RemoteCompositeLoader), "Remote composite runtime key could not be resolved to a string.");
                return false;
            }

            _Logger.Log(
                LogType.Log,
                nameof(RemoteCompositeLoader),
                $"Remote composite load starting; locationKey={locationKey}");

            ILoadedAsset<StagesCollectionComposite> lease =
                await _AssetLoader.LoadAsync<StagesCollectionComposite>(locationKey, cancellationToken);
            if (lease == null || !lease.IsValid || lease.Asset == null)
            {
                _Logger.LogError(nameof(RemoteCompositeLoader), $"Remote composite load failed for '{locationKey}'.");
                return false;
            }

            _Lease = lease;
            _Logger.Log(
                LogType.Log,
                nameof(RemoteCompositeLoader),
                $"Remote composite load succeeded; collectionsCount={_Lease.Asset.CollectionsCount}; keys=[{BuildWorldKeySummary(_Lease.Asset)}]");
            return true;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _Lease?.Dispose();
            _Lease = null;
        }

        private static string BuildWorldKeySummary(StagesCollectionComposite composite)
        {
            if (composite == null || composite.CollectionsCount <= 0)
            {
                return "none";
            }

            string[] keys = new string[composite.CollectionsCount];
            for (int i = 0; i < composite.CollectionsCount; i++)
            {
                string key = composite.GetCollectionLocationKey(i);
                keys[i] = $"{i}:{(string.IsNullOrEmpty(key) ? "null" : key)}";
            }

            return string.Join(" | ", keys);
        }
    }
}
