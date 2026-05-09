using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Common.AssetLoading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace Common.AssetLoading.UnityAddressables
{
    /// <summary>
    /// <see cref="IAssetLoader"/> backed by Unity Addressables.
    /// </summary>
    public class AddressableAssetLoader : IAssetLoader
    {
        private static readonly int MaxInternalIdLoggedLength = 256;

        private static readonly int MaxDependencyDepth = 16;

        private const string UriHintHttps = "https-uri";

        private const string UriHintNonHttps = "non-https-uri";

        private const string UriHintEmpty = "empty-uri";

        private readonly ILogger _Logger;

        /// <summary>
        /// Creates a loader that reports failures through <paramref name="logger"/>.
        /// </summary>
        /// <param name="logger">Application logger instance.</param>
        public AddressableAssetLoader(ILogger logger)
        {
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async UniTask<ILoadedAsset<T>> LoadAsync<T>(string locationKey, CancellationToken cancellationToken = default)
        {
            Type assetType = typeof(T);
            string resolvedLocationsSummary =
                await TrySummarizeLocationsAsync(locationKey, assetType, cancellationToken);

            _Logger.Log(
                LogType.Log,
                nameof(AddressableAssetLoader),
                $"Load begin key={locationKey} type={assetType.Name} {resolvedLocationsSummary}");

            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(locationKey);
            try
            {
                await handle.ToUniTask(cancellationToken: cancellationToken);
                string assetSummary = DescribeLoadedAsset(handle.Result);
                _Logger.Log(
                    LogType.Log,
                    nameof(AddressableAssetLoader),
                    $"Load success key={locationKey} type={assetType.Name} {resolvedLocationsSummary} asset={assetSummary}");
                return new AddressableLoadedAsset<T>(handle);
            }
            catch (OperationCanceledException)
            {
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }

                _Logger.LogWarning(
                    nameof(AddressableAssetLoader),
                    $"Load cancelled key={locationKey} type={assetType.Name} {resolvedLocationsSummary}");
                return null;
            }
            catch (Exception exception)
            {
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }

                _Logger.LogError(
                    nameof(AddressableAssetLoader),
                    $"Load failed key={locationKey} type={assetType.Name} {resolvedLocationsSummary} error={exception}");
                return null;
            }
        }

        private static async UniTask<string> TrySummarizeLocationsAsync(
            object locationKey,
            Type assetType,
            CancellationToken cancellationToken)
        {
            AsyncOperationHandle<IList<IResourceLocation>> handle =
                Addressables.LoadResourceLocationsAsync(locationKey, assetType);
            try
            {
                await handle.ToUniTask(cancellationToken: cancellationToken);
                IList<IResourceLocation> list = handle.Result;
                if (list == null || list.Count == 0)
                {
                    return "locations=none";
                }

                bool graphContainsHttps = false;
                var visited = new HashSet<string>();
                var chains = new StringBuilder();

                for (int i = 0; i < list.Count; i++)
                {
                    if (i > 0)
                    {
                        chains.Append(" | ");
                    }

                    bool branchHasHttps = false;
                    AppendLocationChain(chains, list[i], depth: 0, visited, ref branchHasHttps);
                    graphContainsHttps |= branchHasHttps;
                }

                var summary = new StringBuilder();
                summary.Append("deliveryHttpsInDependencyGraph=");
                summary.Append(graphContainsHttps ? "true" : "false");
                summary.Append(' ');
                summary.Append(chains);
                return summary.ToString();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return $"locations=resolve-error:{ex.Message}";
            }
            finally
            {
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
            }
        }

        private static void AppendLocationChain(
            StringBuilder summary,
            IResourceLocation location,
            int depth,
            HashSet<string> visited,
            ref bool branchContainsHttps)
        {
            if (location == null || depth > MaxDependencyDepth)
            {
                return;
            }

            string internalId = location.InternalId ?? string.Empty;
            if (IsHttpsUri(internalId))
            {
                branchContainsHttps = true;
            }

            string dedupeKey = $"{location.ProviderId}\x1f{internalId}";
            if (!visited.Add(dedupeKey))
            {
                return;
            }

            summary.Append('[');
            summary.Append(ShortProviderId(location.ProviderId));
            summary.Append(' ');
            summary.Append(ClassifyUriHint(internalId));
            summary.Append(' ');
            summary.Append(TruncateForLog(internalId));
            summary.Append(']');

            if (!location.HasDependencies || location.Dependencies == null || location.Dependencies.Count == 0)
            {
                return;
            }

            for (int i = 0; i < location.Dependencies.Count; i++)
            {
                summary.Append(i == 0 ? " -> " : " | ");
                AppendLocationChain(summary, location.Dependencies[i], depth + 1, visited, ref branchContainsHttps);
            }
        }

        private static bool IsHttpsUri(string internalId)
        {
            if (string.IsNullOrEmpty(internalId))
            {
                return false;
            }

            return internalId.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                   internalId.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
        }

        private static string ClassifyUriHint(string internalId)
        {
            if (string.IsNullOrEmpty(internalId))
            {
                return UriHintEmpty;
            }

            return IsHttpsUri(internalId) ? UriHintHttps : UriHintNonHttps;
        }

        private static string ShortProviderId(string providerId)
        {
            if (string.IsNullOrEmpty(providerId))
            {
                return "?";
            }

            int lastDot = providerId.LastIndexOf('.');
            return lastDot >= 0 ? providerId.Substring(lastDot + 1) : providerId;
        }

        private static string TruncateForLog(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            if (value.Length <= MaxInternalIdLoggedLength)
            {
                return value;
            }

            return value.Substring(0, MaxInternalIdLoggedLength) + "...";
        }

        private static string DescribeLoadedAsset<T>(T asset)
        {
            switch (asset)
            {
                case UnityEngine.Object unityObject when unityObject != null:
                    return unityObject.name;
                case null:
                    return "null";
                default:
                    return asset.ToString();
            }
        }
    }
}
