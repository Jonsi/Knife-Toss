using Game.Level.Catalogs;

namespace Game.Level.Interfaces
{
    /// <summary>
    /// Provides the active ordered worlds manifest (composite), including remote-loaded instances after bootstrap.
    /// </summary>
    public interface IStagesCollectionCatalog
    {
        /// <summary>
        /// Current composite asset, or null when unavailable.
        /// </summary>
        StagesCollectionComposite Composite { get; }

        /// <summary>
        /// Number of world collections in order.
        /// </summary>
        int CollectionsCount { get; }

        /// <summary>
        /// Addressables location key for the world at the index, or null when missing.
        /// </summary>
        /// <param name="collectionIndex">Zero-based world index.</param>
        string GetCollectionLocationKey(int collectionIndex);

        /// <summary>
        /// Configured stage slot count for unlock math without loading the collection asset.
        /// </summary>
        /// <param name="collectionIndex">Zero-based world index.</param>
        int GetStageSlotCountForWorld(int collectionIndex);

        /// <summary>
        /// One-based cumulative stage label for UI across prior worlds plus local index.
        /// </summary>
        /// <param name="collectionIndex">Zero-based world index.</param>
        /// <param name="localStageIndex">Zero-based stage index within the world.</param>
        int GetCumulativeStageNumberDisplay(int collectionIndex, int localStageIndex);
    }
}
