using Common.AssetLoading;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Common.AssetLoading.UnityAddressables
{
    /// <summary>
    /// Wraps an Addressables operation handle as a disposable <see cref="ILoadedAsset{T}"/>.
    /// </summary>
    public class AddressableLoadedAsset<T> : ILoadedAsset<T>
    {
        private AsyncOperationHandle<T> _OperationHandle;

        /// <summary>
        /// Creates a lease over the specified completed Addressables handle.
        /// </summary>
        public AddressableLoadedAsset(AsyncOperationHandle<T> operationHandle)
        {
            _OperationHandle = operationHandle;
        }

        /// <inheritdoc />
        public T Asset => _OperationHandle.Result;

        /// <inheritdoc />
        public bool IsValid => _OperationHandle.IsValid();

        /// <inheritdoc />
        public void Dispose()
        {
            if (_OperationHandle.IsValid())
            {
                Addressables.Release(_OperationHandle);
            }

            _OperationHandle = default;
        }
    }
}
