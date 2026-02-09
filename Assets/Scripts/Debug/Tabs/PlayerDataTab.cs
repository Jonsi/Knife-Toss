#if TEST_ENABLED
using System.Collections.Generic;
using Game.Economy.Data;
using Game.Economy.Interfaces;
using VContainer;
using Game.Flow.Game;
using Game.Persistence.Services;

namespace Game.Debugging.Tabs
{
    /// <summary>
    /// Debug tab displaying player data.
    /// </summary>
    public class PlayerDataTab : DebugTab
    {
        public override string Key => "PlayerData";
        public override string Title => "Player";
        public override int SortIndex => -100;

        public PlayerDataTab()
        {
        }

        public PlayerDataTab(IObjectResolver resolver) : this()
        {
        }

        /// <inheritdoc />
        public override IEnumerable<DebugTabSection> GetSections()
        {
            var resolver = CurrentResolver;
            if (resolver == null) yield break;

            var playerDataService = resolver.Resolve<PlayerDataService>();
            var gameData = resolver.Resolve<GameData>();
            var currencyService = resolver.Resolve<ICurrencyService>();
            var catalog = TryResolve<CurrencyTypeCatalog>(resolver);
            var configs = catalog?.GetAllConfigs();
            string currencyId = (configs != null && configs.Count > 0 && configs[0] != null) ? (configs[0].TypeId ?? string.Empty) : string.Empty;
            int totalCoins = string.IsNullOrEmpty(currencyId) ? 0 : currencyService.GetBalance(currencyId);

            yield return new DebugTabSection("Progress")
                .Add("Highest Stage", playerDataService.GetHighestStageIndex())
                .Add("Current Stage", gameData.CurrentStageIndex)
                .Add("Total Coins", totalCoins);

            yield return new DebugTabSection("Settings")
                .Add("Is Muted", playerDataService.Settings.IsMuted ? "Yes" : "No");
        }
    }
}
#endif
