using System;

namespace Common.AssetLoading
{
    /// <summary>
    /// Disposable lease of an asset instance loaded from a storage backend (e.g. Addressables).
    /// </summary>
    public interface ILoadedAsset<out T> : IDisposable
    {
        /// <summary>
        /// The loaded asset instance.
        /// </summary>
        T Asset { get; }

        /// <summary>
        /// True while the lease is valid and the asset can be used without releasing a prior load.
        /// </summary>
        bool IsValid { get; }
    }
}
