using System.Threading;
using Common.Ads.Data;
using Common.Ads.Interfaces;
using Cysharp.Threading.Tasks;

namespace Common.Ads.Providers
{
    /// <summary>
    /// Safe non-test fallback used until a production ad SDK adapter is configured.
    /// It never displays or rewards a simulated ad.
    /// </summary>
    public sealed class UnavailableAdProvider : IAdProvider
    {
        public UniTask InitializeAsync(CancellationToken ct = default) => UniTask.CompletedTask;

        public UniTask LoadAsync(AdPlacement placement, CancellationToken ct = default) =>
            UniTask.CompletedTask;

        public bool IsLoaded(AdPlacement placement) => false;

        public UniTask<AdResult> ShowAsync(AdPlacement placement, CancellationToken ct = default) =>
            UniTask.FromResult(AdResult.Unavailable);

        public void Dispose()
        {
        }
    }
}
