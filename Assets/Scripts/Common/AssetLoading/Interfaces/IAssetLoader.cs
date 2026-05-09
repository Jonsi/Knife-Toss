using System.Threading;
using Cysharp.Threading.Tasks;

namespace Common.AssetLoading
{
    /// <summary>
    /// Loads assets by a backend-specific lookup string without exposing storage details.
    /// </summary>
    public interface IAssetLoader
    {
        /// <summary>
        /// Loads an asset asynchronously and returns a disposable lease.
        /// </summary>
        /// <param name="locationKey">Lookup string for the concrete loader (e.g. Addressables location / runtime key).</param>
        /// <param name="cancellationToken">Cancellation for the load operation.</param>
        /// <returns>A valid lease, or null when the load failed or the operation was canceled.</returns>
        UniTask<ILoadedAsset<T>> LoadAsync<T>(string locationKey, CancellationToken cancellationToken = default);
    }
}
