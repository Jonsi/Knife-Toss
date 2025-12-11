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

        /// <summary>
        /// Initializes a new instance of the <see cref="LevelHudViewModel"/> class.
        /// </summary>
        /// <param name="levelData">Active level runtime data.</param>
        /// <param name="stageData">Active stage runtime data.</param>
        /// <param name="gameData">Session-wide runtime data.</param>
        /// <param name="levelCatalog">Catalog providing stage and level counts.</param>
        public LevelHudViewModel(
            LevelData levelData,
            StageData stageData,
            GameData gameData,
            ILevelCatalog levelCatalog)
        {
            _LevelData = levelData;
            _StageData = stageData;
            _GameData = gameData;
            _LevelCatalog = levelCatalog;
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
        public int TotalCollectibles => _GameData.TotalCollected;
    }
}
