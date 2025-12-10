using System.Collections.Generic;
using System.Linq;
using Infrastructure.Leveling;
using KnifeToss.Core.Abstractions.Gameplay;
using KnifeToss.Core.Data.Level;
using UnityEngine;

namespace Infrastructure.Leveling
{
    /// <summary>
    /// Implementation of ILevelCatalog using the scriptable object StagesConfigAsset.
    /// </summary>
    public class StagesConfigCatalog : ILevelCatalog
    {
        private readonly StagesConfigAsset _configAsset;

        /// <summary>
        /// Initializes a new instance of the StagesConfigCatalog class.
        /// </summary>
        /// <param name="configAsset">The StagesConfigAsset containing the game stages data.</param>
        public StagesConfigCatalog(StagesConfigAsset configAsset)
        {
            _configAsset = configAsset;
        }

        /// <summary>
        /// Gets the total number of stages available in the configuration.
        /// </summary>
        public int StageCount => _configAsset.Stages.Count;

        /// <summary>
        /// Gets the number of levels for a specific stage.
        /// </summary>
        /// <param name="stageIndex">The index of the stage to query.</param>
        /// <returns>The number of levels in the specified stage, or 0 if the stage index is invalid.</returns>
        public int GetLevelCount(int stageIndex)
        {
            if (stageIndex < 0 || stageIndex >= _configAsset.Stages.Count)
                return 0;
            return _configAsset.Stages[stageIndex].Levels.Count;
        }

        /// <summary>
        /// Retrieves the configuration for a specific level.
        /// </summary>
        /// <param name="stageIndex">The index of the stage.</param>
        /// <param name="levelIndex">The index of the level within the stage.</param>
        /// <returns>The level configuration, or null if not found.</returns>
        public LevelConfig GetLevelConfig(int stageIndex, int levelIndex)
        {
            if (stageIndex < 0 || stageIndex >= _configAsset.Stages.Count)
                return null;
            
            var levels = _configAsset.Stages[stageIndex].Levels;
            if (levelIndex < 0 || levelIndex >= levels.Count)
                return null;

            return levels[levelIndex];
        }
    }
}
