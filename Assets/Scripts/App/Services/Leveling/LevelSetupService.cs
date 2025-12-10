using System;
using System.Linq;
using App.Services.Session;
using App.Services.Target;
using Infrastructure.Leveling;
using KnifeToss.Core.Abstractions.Gameplay;
using KnifeToss.Core.Abstractions.Gameplay.Session;
using KnifeToss.Core.Abstractions.Leveling;
using KnifeToss.Core.Data.Level;
using UnityEngine;

namespace App.Services.Leveling
{
    /// <summary>
    /// Orchestrates the loading and setup of a level, including spawning targets and setting rules.
    /// </summary>
    public class LevelSetupService : ILevelSetupService
    {
        private readonly ILevelCatalog _LevelCatalog;
        private readonly ILevelSpawner _LevelSpawner;
        private readonly RuntimeLevelRulesProvider _RulesProvider;
        private readonly ILevelSessionService _SessionService;
        private readonly TargetTrackingService _TargetTrackingService;
        private readonly LevelSessionLogger _SessionLogger;

        public LevelSetupService(
            ILevelCatalog levelCatalog,
            ILevelSpawner levelSpawner,
            RuntimeLevelRulesProvider rulesProvider,
            ILevelSessionService sessionService,
            TargetTrackingService targetTrackingService,
            LevelSessionLogger sessionLogger)
        {
            _LevelCatalog = levelCatalog;
            _LevelSpawner = levelSpawner;
            _RulesProvider = rulesProvider;
            _SessionService = sessionService;
            _TargetTrackingService = targetTrackingService;
            _SessionLogger = sessionLogger;
        }

        /// <summary>
        /// Loads a specific level, spawns its content, and starts the session.
        /// </summary>
        /// <param name="stageIndex">The index of the stage.</param>
        /// <param name="levelIndex">The index of the level.</param>
        public void LoadLevel(int stageIndex, int levelIndex)
        {
            LevelConfig config = _LevelCatalog.GetLevelConfig(stageIndex, levelIndex);
            int totalKnivesToBreak = config.Targets.Sum(t => t.KnivesToBreak);
            // Determine knife count: prefer pre-calculated value from config, fallback to runtime calculation
            int knifeCount = config.KnifeCount > 0 ? config.KnifeCount : totalKnivesToBreak + config.ExtraKnives;

            LevelRules rules = new LevelRules(knifeCount, totalKnivesToBreak);
            _RulesProvider.SetRules(rules);

            var targets = _LevelSpawner.SpawnLevel(config);
            _TargetTrackingService.RegisterTargets(targets);

            _SessionService.StartSession(stageIndex, levelIndex);
        }
    }
}
