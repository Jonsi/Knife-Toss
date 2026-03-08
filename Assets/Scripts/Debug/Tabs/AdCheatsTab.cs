#if TEST_ENABLED
using System.ComponentModel;
using Common.Ads.Data;
using Common.Ads.Interfaces;
using Common.Ads.Providers;

namespace Game.Debugging.Tabs
{
    /// <summary>
    /// SRDebugger tab for ad system inspection and manual control during QA.
    /// Surfaces key <see cref="IAdService"/> state and exposes actions for forcing outcomes,
    /// resetting gates, and toggling the kill switch — without needing a real ad SDK.
    /// </summary>
    public class AdCheatsTab : OptionsTab
    {
        public override string Key => "AdCheats";
        public override string Title => "5. Ads";
        public override string IconStyleKey => "Icon_Settings";
        public override int SortIndex => -106;

        [Category("State")]
        [DisplayName("Ads Enabled")]
        public bool AdsEnabled => TryResolve<AdSettings>(CurrentResolver)?.AdsEnabled ?? false;

        [Category("Actions")]
        [DisplayName("Reset Cooldowns")]
        public void ResetCooldowns()
        {
            TryResolve<IAdService>(CurrentResolver)?.ResetCooldowns();
            RequestRefresh();
        }

        [Category("Actions")]
        [DisplayName("Reset Once-Per-Run Flags")]
        public void ResetOncePerRun()
        {
            TryResolve<IAdService>(CurrentResolver)?.ResetRunState();
            RequestRefresh();
        }

        [Category("Force Outcome")]
        [DisplayName("Force: Completed")]
        public void ForceCompleted() =>
            TryResolve<MockAdProvider>(CurrentResolver)?.ForceNextOutcome(AdMockOutcome.Completed);

        [Category("Force Outcome")]
        [DisplayName("Force: Skipped")]
        public void ForceSkipped() =>
            TryResolve<MockAdProvider>(CurrentResolver)?.ForceNextOutcome(AdMockOutcome.Skipped);

        [Category("Force Outcome")]
        [DisplayName("Force: Unavailable")]
        public void ForceUnavailable() =>
            TryResolve<MockAdProvider>(CurrentResolver)?.ForceNextOutcome(AdMockOutcome.Unavailable);

        [Category("Force Outcome")]
        [DisplayName("Force: Timeout")]
        public void ForceTimeout() =>
            TryResolve<MockAdProvider>(CurrentResolver)?.ForceNextOutcome(AdMockOutcome.Timeout);

        [Category("Force Outcome")]
        [DisplayName("Force: Failed")]
        public void ForceFailed() =>
            TryResolve<MockAdProvider>(CurrentResolver)?.ForceNextOutcome(AdMockOutcome.Failed("Forced failure."));
    }
}
#endif
