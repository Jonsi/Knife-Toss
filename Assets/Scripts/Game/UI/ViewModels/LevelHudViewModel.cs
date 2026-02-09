using Game.Economy.Data;
using Game.Economy.Interfaces;
using Game.Flow.Game;
using Game.Flow.Level;
using Game.Flow.Stage;
using Game.Level.Interfaces;
using Game.UI.Interfaces;

namespace Game.UI.ViewModels
{
    /// <summary>
    /// Provides current HUD values derived from runtime game data.
    /// </summary>
    public class LevelHudViewModel : ILevelHudData
    {
        private readonly LevelData _LevelData;
        private readonly StageData _StageData;
        private readonly GameData _GameData;
        private readonly ILevelCatalog _LevelCatalog;
        private readonly ICurrencyService _CurrencyService;
        private readonly CurrencyTypeCatalog _CurrencyTypeCatalog;

        /// <summary>
        /// Initializes a new instance of the LevelHudViewModel.
        /// </summary>
        public LevelHudViewModel(
            LevelData levelData,
            StageData stageData,
            GameData gameData,
            ILevelCatalog levelCatalog,
            ICurrencyService currencyService,
            CurrencyTypeCatalog currencyTypeCatalog)
        {
            _LevelData = levelData;
            _StageData = stageData;
            _GameData = gameData;
            _LevelCatalog = levelCatalog;
            _CurrencyService = currencyService;
            _CurrencyTypeCatalog = currencyTypeCatalog;
        }

        /// <inheritdoc />
        public int CurrentStage => _StageData.StageIndex + 1;

        /// <inheritdoc />
        public int CurrentLevel => _StageData.CurrentLevelIndex + 1;

        /// <inheritdoc />
        public int TotalLevelsInStage => _LevelCatalog.GetLevelCount(_StageData.StageIndex);

        /// <inheritdoc />
        public int KnivesRemaining
        {
            get
            {
                int remaining = _LevelData.KnifeCount - _LevelData.KnivesThrown;
                return remaining < 0 ? 0 : remaining;
            }
        }

        /// <inheritdoc />
        public int TotalKnives => _LevelData.KnifeCount;

        /// <inheritdoc />
        public int TotalCollectibles
        {
            get
            {
                if (_CurrencyService == null || _CurrencyTypeCatalog == null)
                    return 0;
                var configs = _CurrencyTypeCatalog.GetAllConfigs();
                if (configs == null || configs.Count == 0)
                    return 0;
                var first = configs[0];
                return first != null && !string.IsNullOrEmpty(first.TypeId)
                    ? _CurrencyService.GetBalance(first.TypeId)
                    : 0;
            }
        }
    }
}
