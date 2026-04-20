#if TEST_ENABLED
using System.ComponentModel;
using System.Linq;
using Game.Economy.Interfaces;
using Game.Flow.Game;
using Game.Persistence.Services;
using Game.Settings;
using VContainer;

namespace Game.Debugging.Tabs
{
    /// <summary>
    /// Debug tab for general progress and options.
    /// </summary>
    public class GeneralTab : OptionsTab
    {
        public override string Key => "General";
        public override string Title => "1. General";
        public override string IconStyleKey => "Icon_Settings";
        public override int SortIndex => -110;

        public GeneralTab(IObjectResolver resolver)
        {
        }

        [Category("Progress")]
        [DisplayName("Highest Stage")]
        public int HighestStage
        {
            get
            {
                var resolver = CurrentResolver;
                return (TryResolve<PlayerDataService>(resolver)?.GetHighestStageIndex() ?? 0) + 1;
            }
        }

        [Category("Progress")]
        [DisplayName("Current Stage")]
        public int CurrentStage
        {
            get
            {
                var resolver = CurrentResolver;
                return (TryResolve<GameData>(resolver)?.CurrentStageIndex ?? 0) + 1;
            }
        }

        [Category("Progress")]
        [DisplayName("Total Coins")]
        public int TotalCoins
        {
            get
            {
                var resolver = CurrentResolver;
                string currencyId = GetFirstCurrencyId();
                return string.IsNullOrEmpty(currencyId) ? 0 : (TryResolve<ICurrencyService>(resolver)?.GetBalance(currencyId) ?? 0);
            }
        }

        [Category("Progress")]
        [DisplayName("Reset All Player Data")]
        public void ClearProgress()
        {
            var resolver = CurrentResolver;
            if (resolver == null) return;

            var cheats = TryResolve<GeneralCheats>(resolver);
            cheats?.ClearProgress();
            RequestRefresh();
        }

        [Category("Progress")]
        [DisplayName("Unlock All Stages As Won")]
        public void UnlockAllStagesAsWon()
        {
            var resolver = CurrentResolver;
            if (resolver == null) return;

            var cheats = TryResolve<GeneralCheats>(resolver);
            cheats?.UnlockAllStagesAsWon();
            RequestRefresh();
        }

        [Category("Debug")]
        [DisplayName("Enable Test Logging")]
        public bool EnableTestLogging
        {
            get => GameSettingsAccess.IsTestLoggingEnabled;
            set => GameSettingsAccess.SetTestLoggingEnabled(value);
        }
    }
}
#endif
