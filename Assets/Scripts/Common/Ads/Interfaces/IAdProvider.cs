using System.Threading;
using Common.Ads.Data;
using Cysharp.Threading.Tasks;

namespace Common.Ads.Interfaces
{
    /// <summary>
    /// Low-level SDK adapter contract. One implementation per ad SDK (or mock).
    /// Intentionally free of frequency, cooldown, and consent logic — those belong to IAdService.
    /// </summary>
    public interface IAdProvider
    {
        /// <summary>Initializes the underlying ad SDK.</summary>
        UniTask InitializeAsync(CancellationToken ct = default);

        /// <summary>Requests and caches an ad for the given placement.</summary>
        UniTask LoadAsync(AdPlacement placement, CancellationToken ct = default);

        /// <summary>Returns true if a loaded ad is ready to display for the given placement.</summary>
        bool IsLoaded(AdPlacement placement);

        /// <summary>
        /// Displays the loaded ad and awaits user interaction.
        /// Returns an <see cref="AdResult"/> describing the outcome.
        /// </summary>
        UniTask<AdResult> ShowAsync(AdPlacement placement, CancellationToken ct = default);

        /// <summary>Releases all internal provider state and resources.</summary>
        void Dispose();
    }
}
