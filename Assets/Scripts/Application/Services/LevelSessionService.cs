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
        private readonly HitableObjectSpawner hitableSpawner;
        private readonly int startLevel;
        private readonly KnifeImpactEventChannel impactEventChannel;
        private readonly HitableObjectHitEventChannel hitableEventChannel;

        private LevelRules currentRules;
        private LevelData currentLevelData;

        /// <summary>
        /// Performs the LevelSessionService operation.
        /// </summary>
        public LevelSessionService(
            LevelSession levelSession,
            ILevelCatalog levelCatalog,
            ILevelProgressStore levelProgressStore,
            GameplayObjectProvider gameplayObjectProvider,
            KnivesHierarchyManager knivesHierarchy,
            PreStuckKnifeSpawner preStuckKnifeSpawner,
            HitableObjectSpawner hitableObjectSpawner,
            int startingLevel,
            KnifeImpactEventChannel eventChannel,
            HitableObjectHitEventChannel hitableHitEventChannel)
        {
            session = levelSession;
            catalog = levelCatalog;
            progressStore = levelProgressStore;
            this.gameplayObjectProvider = gameplayObjectProvider;
            hierarchyManager = knivesHierarchy;
            preStuckSpawner = preStuckKnifeSpawner;
            hitableSpawner = hitableObjectSpawner;
            startLevel = Mathf.Max(1, startingLevel);
            impactEventChannel = eventChannel;
            hitableEventChannel = hitableHitEventChannel;

            if (impactEventChannel != null)
            {
                impactEventChannel.KnifeStuck += HandleKnifeStuck;
            }

            if (hitableEventChannel != null)
            {
                hitableEventChannel.HitableHitAt += HandleHitableObjectHit;
            }
        }

        public LevelSession Session => session;
        public LevelRules CurrentRules => currentRules;
        public int CurrentLevel => session.LevelIndex;

        /// <summary>
        /// Performs the StartInitialLevelAsync operation.
        /// </summary>
        public UniTask StartInitialLevelAsync()
        {
            int highestLevel = progressStore.LoadHighestLevel(startLevel);
            int levelToStart = Mathf.Max(startLevel, highestLevel);
            return StartLevelAsync(levelToStart);
        }

        /// <summary>
        /// Performs the StartLevelAsync operation.
        /// </summary>
        public async UniTask StartLevelAsync(int levelIndex)
        {
            ResetThrowerTweens();
            await RebuildKnifeHierarchyAsync();
            Transform knivesParent = hierarchyManager.EnsureParent();

            ApplyRules(levelIndex);
            ConfigureTarget(currentRules);
            ConfigureThrower(knivesParent, currentRules);

            await UniTask.WaitForEndOfFrame();
            SpawnHitableObjects();
            int preStuckSpawned = preStuckSpawner.Spawn(currentLevelData);
            session.SeedPreStuckKnives(preStuckSpawned);
        }

        /// <summary>
        /// Performs the RetryLevelAsync operation.
        /// </summary>
        public UniTask RetryLevelAsync()
        {
            return StartLevelAsync(session.LevelIndex);
        }

        /// <summary>
        /// Performs the NextLevelAsync operation.
        /// </summary>
        public UniTask NextLevelAsync()
        {
            return StartLevelAsync(session.LevelIndex + 1);
        }

        /// <summary>
        /// Performs the HandleKnifeStuck operation.
        /// </summary>
        public void HandleKnifeStuck()
        {
            session.RecordKnifeStuck();
            ScheduleEvaluation();
        }

        /// <summary>
        /// Performs the HandleHitableObjectHit operation.
        /// </summary>
        private void HandleHitableObjectHit(Vector3 _)
        {
            session.RecordHitableObjectHit();
            ScheduleEvaluation();
        }

        /// <summary>
        /// Performs the HandleKnifeThrown operation.
        /// </summary>
        public void HandleKnifeThrown()
        {
            session.RecordThrow();
            if (session.KnivesThrown >= session.Rules.KnifeCount)
            {
                gameplayObjectProvider.GetKnifeThrower().SetCanThrow(false);
            }
        }

        /// <summary>
        /// Performs the HandleKnifeResolved operation.
        /// </summary>
        public void HandleKnifeResolved()
        {
            ScheduleEvaluation();
        }

        /// <summary>
        /// Evaluates completion/failure after knife-related events.
        /// </summary>
        private bool evaluationScheduled;

        private void ScheduleEvaluation()
        {
            if (evaluationScheduled || !session.HasExhaustedKnives())
            {
                return;
            }

            evaluationScheduled = true;
            AwaitEvaluation().Forget();
        }

        private async UniTaskVoid AwaitEvaluation()
        {
            await UniTask.NextFrame();
            evaluationScheduled = false;
            EvaluateEndState();
        }

        private void EvaluateEndState()
        {
            if (!session.HasExhaustedKnives() || session.IsComplete || session.IsFailed)
            {
                return;
            }

            if (session.HasMetObjectives)
            {
                session.MarkCompleteIfNeeded();
                DisableThrowerInput();
                SaveProgress();
            }
            else
            {
                session.RecordFailure();
                DisableThrowerInput();
            }
        }

        /// <summary>
        /// Performs the ConfigureTarget operation.
        /// </summary>
        private void ConfigureTarget(LevelRules rules)
        {
            TargetGroupManager targetGroupManager = gameplayObjectProvider.GetTargetGroupManager();
            targetGroupManager.Configure(currentLevelData, 0);
            targetGroupManager.ResumeAll();
        }

        /// <summary>
        /// Performs the ConfigureThrower operation.
        /// </summary>
        private void ConfigureThrower(Transform knivesParent, LevelRules rules)
        {
            KnifeThrower knifeThrower = gameplayObjectProvider.GetKnifeThrower();

            knifeThrower.ResetState();
            knifeThrower.SetScaleProvider(gameplayObjectProvider.GetTargetGroupManager());
            knifeThrower.SetTotalKnives(rules.KnifeCount);
            knifeThrower.SetKnivesParent(knivesParent);
            knifeThrower.SetCanThrow(true);
        }

        /// <summary>
        /// Performs the SaveProgress operation.
        /// </summary>
        private void SaveProgress()
        {
            if (progressStore == null)
                return;

            progressStore.SaveHighestLevel(session.LevelIndex);
        }

        /// <summary>
        /// Performs the DisableThrowerInput operation.
        /// </summary>
        private void DisableThrowerInput()
        {
            KnifeThrower knifeThrower = gameplayObjectProvider?.GetKnifeThrower();
            knifeThrower?.SetCanThrow(false);
        }

        /// <summary>
        /// Performs the ResetThrowerTweens operation.
        /// </summary>
        private void ResetThrowerTweens()
        {
            KnifeThrower knifeThrower = gameplayObjectProvider.GetKnifeThrower();
            knifeThrower?.KillAllKnifeTweens();
        }

        /// <summary>
        /// Performs the RebuildKnifeHierarchyAsync operation.
        /// </summary>
        private async UniTask RebuildKnifeHierarchyAsync()
        {
            hierarchyManager.ResetHierarchy();
            await UniTask.Yield();
        }

        /// <summary>
        /// Performs the ApplyRules operation.
        /// </summary>
        private void ApplyRules(int levelIndex)
        {
            currentLevelData = catalog.GetLevelData(levelIndex);
            currentRules = catalog.GetRules(levelIndex);
            session.Begin(levelIndex, currentRules);
        }

        /// <summary>
        /// Performs the SpawnHitableObjects operation.
        /// </summary>
        private void SpawnHitableObjects()
        {
            hitableSpawner?.Spawn(currentLevelData);
        }

        /// <summary>
        /// Performs the Dispose operation.
        /// </summary>
        public void Dispose()
        {
            if (impactEventChannel != null)
            {
                impactEventChannel.KnifeStuck -= HandleKnifeStuck;
            }

            if (hitableEventChannel != null)
            {
                hitableEventChannel.HitableHitAt -= HandleHitableObjectHit;
            }
        }
    }
}
