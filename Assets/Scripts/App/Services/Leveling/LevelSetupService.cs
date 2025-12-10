using System;
using System.Linq;
using App.Services.Session;
using App.Services.Target;
using Infrastructure.Leveling;
using KnifeToss.Core.Abstractions.Gameplay;
using KnifeToss.Core.Abstractions.Gameplay.Session;
using KnifeToss.Core.Data.Level;
using UnityEngine;

namespace App.Services.Leveling
{
    /// <summary>
    /// Orchestrates the loading and setup of a level, including spawning targets and setting rules.
    /// </summary>
    public class LevelSetupService
    {
        private readonly StagesConfigCatalog _levelCatalog;
        private readonly ILevelSpawner _levelSpawner;
        private readonly RuntimeLevelRulesProvider _rulesProvider;
        private readonly ILevelSessionService _sessionService;
        private readonly TargetTrackingService _targetTrackingService;

        public LevelSetupService(
            StagesConfigCatalog levelCatalog,
            ILevelSpawner levelSpawner,
            RuntimeLevelRulesProvider rulesProvider,
            ILevelSessionService sessionService,
            TargetTrackingService targetTrackingService)
        {
            _levelCatalog = levelCatalog;
            _levelSpawner = levelSpawner;
            _rulesProvider = rulesProvider;
            _sessionService = sessionService;
            _targetTrackingService = targetTrackingService;
        }

        /// <summary>
        /// Loads a specific level, spawns its content, and starts the session.
        /// </summary>
        /// <param name="stageIndex">The index of the stage.</param>
        /// <param name="levelIndex">The index of the level.</param>
        public void LoadLevel(int stageIndex, int levelIndex)
        {
            LevelConfig config = _levelCatalog.GetLevelConfig(stageIndex, levelIndex);
            if (config == null)
            {
                Debug.LogError($"Failed to load level {stageIndex}-{levelIndex}");
                return;
            }

            int totalKnivesToBreak = config.Targets.Sum(t => t.KnivesToBreak);
            
            int knifeCount = config.KnifeCount > 0 ? config.KnifeCount : totalKnivesToBreak + config.ExtraKnives;

            LevelRules rules = new LevelRules(knifeCount, totalKnivesToBreak);
            _rulesProvider.SetRules(rules);

            var targets = _levelSpawner.SpawnLevel(config);
            _targetTrackingService.RegisterTargets(targets);

            _sessionService.StartSession();
        }
    }
}
