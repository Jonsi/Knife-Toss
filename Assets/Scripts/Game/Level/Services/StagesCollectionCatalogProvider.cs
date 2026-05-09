using System;
using Game.Level.Catalogs;
using Game.Level.Interfaces;
using UnityEngine;

namespace Game.Level.Services
{
    /// <summary>
    /// Exposes the remote-loaded composite when present, otherwise the baked fallback from the installer.
    /// </summary>
    public class StagesCollectionCatalogProvider : IStagesCollectionCatalog
    {
        private readonly StagesCollectionComposite _FallbackComposite;
        private readonly RemoteCompositeLoader _RemoteLoader;
        private readonly ILogger _Logger;
        private bool? _LastWasRemote;

        /// <summary>
        /// Creates the provider with fallback serialized composite and remote loader state.
        /// </summary>
        /// <param name="fallbackComposite">Composite baked with the player build.</param>
        /// <param name="remoteLoader">Loader holding an optional remote lease.</param>
        public StagesCollectionCatalogProvider(
            StagesCollectionComposite fallbackComposite,
            RemoteCompositeLoader remoteLoader,
            ILogger logger)
        {
            _FallbackComposite = fallbackComposite ?? throw new ArgumentNullException(nameof(fallbackComposite));
            _RemoteLoader = remoteLoader ?? throw new ArgumentNullException(nameof(remoteLoader));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public StagesCollectionComposite Composite
        {
            get
            {
                bool useRemote = _RemoteLoader.Composite != null;
                if (!_LastWasRemote.HasValue || _LastWasRemote.Value != useRemote)
                {
                    _LastWasRemote = useRemote;
                    StagesCollectionComposite active = useRemote ? _RemoteLoader.Composite : _FallbackComposite;
                    int count = active != null ? active.CollectionsCount : 0;
                    _Logger.Log(
                        LogType.Log,
                        nameof(StagesCollectionCatalogProvider),
                        $"Active composite source={(useRemote ? "remote" : "fallback")}; collectionsCount={count}; keys=[{BuildWorldKeySummary(active)}]");
                }

                return useRemote ? _RemoteLoader.Composite : _FallbackComposite;
            }
        }

        /// <inheritdoc />
        public int CollectionsCount => Composite != null ? Composite.CollectionsCount : 0;

        /// <inheritdoc />
        public string GetCollectionLocationKey(int collectionIndex)
        {
            return Composite != null ? Composite.GetCollectionLocationKey(collectionIndex) : null;
        }

        /// <inheritdoc />
        public int GetStageSlotCountForWorld(int collectionIndex)
        {
            return Composite != null ? Composite.GetStageSlotCountForWorld(collectionIndex) : 0;
        }

        /// <inheritdoc />
        public int GetCumulativeStageNumberDisplay(int collectionIndex, int localStageIndex)
        {
            return Composite != null
                ? Composite.GetCumulativeStageNumberDisplay(collectionIndex, localStageIndex)
                : 0;
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
