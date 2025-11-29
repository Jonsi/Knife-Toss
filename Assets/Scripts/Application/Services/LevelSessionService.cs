using KnifeToss.Core.Abstractions;
using KnifeToss.Core.Domain;
using KnifeToss.Infrastructure.Leveling;
using KnifeToss.Infrastructure.Persistence;
using KnifeToss.Presentation.Gameplay;
using UnityEngine;

namespace KnifeToss.Application.Services
{
    /// <summary>
    /// Coordinates the lifecycle of a level and exposes a pure session model.
    /// </summary>
    public sealed class LevelSessionService
    {
        private readonly LevelSession session;
        private readonly ILevelCatalog catalog;
        private readonly ILevelProgressStore progressStore;
        private readonly RotatingTarget rotatingTarget;
        private readonly KnifeThrower knifeThrower;
        private readonly KnivesHierarchyManager hierarchyManager;
        private readonly PreStuckKnifeSpawner preStuckSpawner;
        private readonly int startLevel;

        private LevelRules currentRules;

        public LevelSessionService(
            LevelSession levelSession,
            ILevelCatalog levelCatalog,
            ILevelProgressStore levelProgressStore,
            RotatingTarget target,
            KnifeThrower thrower,
            KnivesHierarchyManager knivesHierarchy,
            PreStuckKnifeSpawner preStuckKnifeSpawner,
            int startingLevel)
        {
            session = levelSession;
            catalog = levelCatalog;
            progressStore = levelProgressStore;
            rotatingTarget = target;
            knifeThrower = thrower;
            hierarchyManager = knivesHierarchy;
            preStuckSpawner = preStuckKnifeSpawner;
            startLevel = Mathf.Max(1, startingLevel);
        }

        public LevelSession Session => session;
        public LevelRules CurrentRules => currentRules;
        public int CurrentLevel => session.LevelIndex;

        public void StartInitialLevel()
        {
            int highestLevel = progressStore.LoadHighestLevel(startLevel);
            int levelToStart = Mathf.Max(startLevel, highestLevel);
            StartLevel(levelToStart);
        }

        public void StartLevel(int levelIndex)
        {
            hierarchyManager.ResetHierarchy();
            Transform knivesParent = hierarchyManager.EnsureParent();

            currentRules = catalog.GetRules(levelIndex);
            session.Begin(levelIndex, currentRules);

            ConfigureTarget(currentRules);
            ConfigureThrower(knivesParent, currentRules);

            int preStuckSpawned = preStuckSpawner.Spawn(currentRules);
            session.SeedPreStuckKnives(preStuckSpawned);
        }

        public void RetryLevel()
        {
            StartLevel(session.LevelIndex);
        }

        public void NextLevel()
        {
            StartLevel(session.LevelIndex + 1);
        }

        public void HandleKnifeStuck()
        {
            session.RecordKnifeStuck();
            if (session.IsComplete)
            {
                SaveProgress();
            }
        }

        public void HandleKnifeFailure()
        {
            session.RecordFailure();
        }

        public void HandleKnifeThrown()
        {
            session.RecordThrow();
            if (session.HasExhaustedKnives())
            {
                knifeThrower.SetCanThrow(false);
            }
        }

        private void ConfigureTarget(LevelRules rules)
        {
            if (rotatingTarget == null)
                return;

            rotatingTarget.SetRotationSpeed(rules.RotationSpeed);
            rotatingTarget.SetDirectionChangeInterval(rules.DirectionChangeInterval);
            rotatingTarget.SetRandomizeDirection(rules.RandomizeDirection);
            rotatingTarget.ResetRotation();
        }

        private void ConfigureThrower(Transform knivesParent, LevelRules rules)
        {
            if (knifeThrower == null)
                return;

            knifeThrower.ResetState();
            knifeThrower.SetMaxKnives(rules.MaxKnives);
            knifeThrower.SetKnivesParent(knivesParent);
            knifeThrower.SetCanThrow(true);
        }

        private void SaveProgress()
        {
            if (progressStore == null)
                return;

            progressStore.SaveHighestLevel(session.LevelIndex);
        }
    }
}
