using System;
using Cysharp.Threading.Tasks;
using KnifeToss.Core.Abstractions;
using KnifeToss.Core.Data;
using KnifeToss.Core.Domain;
using KnifeToss.Core.Events;
using KnifeToss.Infrastructure.Leveling;
using KnifeToss.Presentation.Gameplay;
using UnityEngine;

namespace KnifeToss.Application.Services
{



    public sealed class LevelSessionService : IDisposable
    {
        private readonly LevelSession session;
        private readonly ILevelCatalog catalog;
        private readonly ILevelProgressStore progressStore;
        private readonly GameplayObjectProvider gameplayObjectProvider;
        private readonly KnivesHierarchyManager hierarchyManager;
        private readonly PreStuckKnifeSpawner preStuckSpawner;
        private readonly int startLevel;
        private readonly KnifeImpactEventChannel impactEventChannel;

        private LevelRules currentRules;
        private LevelData currentLevelData;

        public LevelSessionService(
            LevelSession levelSession,
            ILevelCatalog levelCatalog,
            ILevelProgressStore levelProgressStore,
            GameplayObjectProvider gameplayObjectProvider,
            KnivesHierarchyManager knivesHierarchy,
            PreStuckKnifeSpawner preStuckKnifeSpawner,
            int startingLevel,
            KnifeImpactEventChannel eventChannel)
        {
            session = levelSession;
            catalog = levelCatalog;
            progressStore = levelProgressStore;
            this.gameplayObjectProvider = gameplayObjectProvider;
            hierarchyManager = knivesHierarchy;
            preStuckSpawner = preStuckKnifeSpawner;
            startLevel = Mathf.Max(1, startingLevel);
            impactEventChannel = eventChannel;

            if (impactEventChannel != null)
            {
                impactEventChannel.KnifeStuck += HandleKnifeStuck;
                impactEventChannel.KnifeFailed += HandleKnifeFailure;
            }
        }

        public LevelSession Session => session;
        public LevelRules CurrentRules => currentRules;
        public int CurrentLevel => session.LevelIndex;

        public UniTask StartInitialLevelAsync()
        {
            int highestLevel = progressStore.LoadHighestLevel(startLevel);
            int levelToStart = Mathf.Max(startLevel, highestLevel);
            return StartLevelAsync(levelToStart);
        }

        public async UniTask StartLevelAsync(int levelIndex)
        {
            ResetThrowerTweens();
            await RebuildKnifeHierarchyAsync();
            Transform knivesParent = hierarchyManager.EnsureParent();

            ApplyRules(levelIndex);
            ConfigureTarget(currentRules);
            ConfigureThrower(knivesParent, currentRules);

            await UniTask.WaitForEndOfFrame();
            int preStuckSpawned = preStuckSpawner.Spawn(currentLevelData);
            session.SeedPreStuckKnives(preStuckSpawned);
        }

        public UniTask RetryLevelAsync()
        {
            return StartLevelAsync(session.LevelIndex);
        }

        public UniTask NextLevelAsync()
        {
            return StartLevelAsync(session.LevelIndex + 1);
        }

        public void HandleKnifeStuck()
        {
            session.RecordKnifeStuck();
            if (session.IsComplete)
            {
                DisableThrowerInput();
                SaveProgress();
            }
        }

        public void HandleKnifeFailure()
        {
            session.RecordFailure();
            DisableThrowerInput();
        }

        public void HandleKnifeThrown()
        {
            session.RecordThrow();
            if (session.HasExhaustedKnives())
            {
                gameplayObjectProvider.GetKnifeThrower().SetCanThrow(false);
            }
        }

        private void ConfigureTarget(LevelRules rules)
        {
            TargetGroupManager targetGroupManager = gameplayObjectProvider.GetTargetGroupManager();
            targetGroupManager.Configure(currentLevelData, 0);
            targetGroupManager.ResumeAll();
        }

        private void ConfigureThrower(Transform knivesParent, LevelRules rules)
        {
            KnifeThrower knifeThrower = gameplayObjectProvider.GetKnifeThrower();

            knifeThrower.ResetState();
            knifeThrower.SetScaleProvider(gameplayObjectProvider.GetTargetGroupManager());
            knifeThrower.SetMaxKnives(rules.MaxKnives);
            knifeThrower.SetTotalKnives(rules.RequiredKnives);
            knifeThrower.SetKnivesParent(knivesParent);
            knifeThrower.SetCanThrow(true);
        }

        private void SaveProgress()
        {
            if (progressStore == null)
                return;

            progressStore.SaveHighestLevel(session.LevelIndex);
        }

        private void DisableThrowerInput()
        {
            KnifeThrower knifeThrower = gameplayObjectProvider?.GetKnifeThrower();
            knifeThrower?.SetCanThrow(false);
        }

        private void ResetThrowerTweens()
        {
            KnifeThrower knifeThrower = gameplayObjectProvider.GetKnifeThrower();
            knifeThrower?.KillAllKnifeTweens();
        }

        private async UniTask RebuildKnifeHierarchyAsync()
        {
            hierarchyManager.ResetHierarchy();
            await UniTask.Yield();
        }

        private void ApplyRules(int levelIndex)
        {
            currentLevelData = catalog.GetLevelData(levelIndex);
            currentRules = catalog.GetRules(levelIndex);
            session.Begin(levelIndex, currentRules);
        }

        public void Dispose()
        {
            if (impactEventChannel != null)
            {
                impactEventChannel.KnifeStuck -= HandleKnifeStuck;
                impactEventChannel.KnifeFailed -= HandleKnifeFailure;
            }
        }
    }
}
