using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Common.AssetLoading.UnityAddressables
{
    /// <summary>
    /// Applies Addressables catalog updates so remote content lists reflect published bundles.
    /// </summary>
    public class CatalogUpdateService
    {
        private readonly ILogger _Logger;

        /// <summary>
        /// Creates the service with application logging.
        /// </summary>
        /// <param name="logger">Logger instance; must not be null.</param>
        public CatalogUpdateService(ILogger logger)
        {
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Checks for catalog updates and applies them when available; completes quietly when offline or unchanged.
        /// </summary>
        /// <param name="cancellationToken">Cancel token for the operation.</param>
        public async UniTask UpdateCatalogAsync(CancellationToken cancellationToken = default)
        {
            _Logger.Log(LogType.Log, nameof(CatalogUpdateService), "Catalog remote check starting.");

            AsyncOperationHandle<List<string>> checkHandle = Addressables.CheckForCatalogUpdates(false);
            List<string> catalogsToUpdate = null;
            try
            {
                await checkHandle.ToUniTask(cancellationToken: cancellationToken);
                catalogsToUpdate = checkHandle.Result;
            }
            catch (OperationCanceledException)
            {
                _Logger.LogWarning(nameof(CatalogUpdateService), "Catalog check cancelled.");
                return;
            }
            catch (Exception exception)
            {
                _Logger.LogError(nameof(CatalogUpdateService), $"Catalog check failed: {exception}");
                return;
            }
            finally
            {
                if (checkHandle.IsValid())
                {
                    Addressables.Release(checkHandle);
                }
            }

            if (catalogsToUpdate == null || catalogsToUpdate.Count == 0)
            {
                _Logger.Log(
                    LogType.Log,
                    nameof(CatalogUpdateService),
                    "Catalog check finished; no remote catalog updates (device keeps existing catalogs).");
                return;
            }

            _Logger.Log(
                LogType.Log,
                nameof(CatalogUpdateService),
                $"Catalog check finished; updatesAvailable={catalogsToUpdate.Count}; ids=[{string.Join(", ", catalogsToUpdate)}]");

            _Logger.Log(
                LogType.Log,
                nameof(CatalogUpdateService),
                $"Catalog update starting; pendingCatalogIds={catalogsToUpdate.Count}");

            var updateHandle = Addressables.UpdateCatalogs(catalogsToUpdate, false);
            try
            {
                await updateHandle.ToUniTask(cancellationToken: cancellationToken);
                int appliedLocatorCount = updateHandle.Result != null ? updateHandle.Result.Count : 0;
                _Logger.Log(
                    LogType.Log,
                    nameof(CatalogUpdateService),
                    $"Catalog update succeeded; appliedCatalogIds={catalogsToUpdate.Count}; locatorCount={appliedLocatorCount}");
            }
            catch (OperationCanceledException)
            {
                _Logger.LogWarning(nameof(CatalogUpdateService), "Catalog update cancelled.");
                return;
            }
            catch (Exception exception)
            {
                _Logger.LogError(nameof(CatalogUpdateService), $"Catalog update failed: {exception}");
            }
        }
    }
}
