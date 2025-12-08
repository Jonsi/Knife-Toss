using System;
using Core.Abstractions;
using Core.Data;
using Core.Domain;
using Core.Events;
using Cysharp.Threading.Tasks;
using Infrastructure.Leveling;
using Presentation.Gameplay;
using UnityEngine;

namespace App.Services
{



    public sealed class LevelSessionService : IDisposable
    {
        private readonly LevelSession session;
        private readonly ILevelCatalog catalog;
        private readonly IPlayerDataService playerDataService;
        private readonly GameplayObjectProvider gameplayObjectProvider;
        private readonly KnivesHierarchyManager hierarchyManager;
        private readonly PreStuckKnifeSpawner preStuckSpawner;
        private readonly HitableObjectSpawner hitableSpawner;
        private readonly StageLayoutApplier layoutApplier;
        private readonly int startStageIndex;
        private readonly int startLevelIndex;
        private readonly KnifeImpactEventChannel impactEventChannel;
        private readonly HitableObjectHitEventChannel hitableEventChannel;
        private readonly TargetHitTracker targetHitTracker;

        private LevelRules currentRules;
        private LevelData currentLevelData;

        /// <summary>
        /// Performs the LevelSessionService operation.
        /// </summary>
        public LevelSessionService(
            LevelSession levelSession,
            ILevelCatalog levelCatalog,
            IPlayerDataService playerDataService,
            GameplayObjectProvider gameplayObjectProvider,
            KnivesHierarchyManager knivesHierarchy,
            PreStuckKnifeSpawner preStuckKnifeSpawner,
            HitableObjectSpawner hitableObjectSpawner,
            StageLayoutApplier layoutApplier,
            int startingStageIndex,
            int startingLevelIndex,
            KnifeImpactEventChannel eventChannel,
            HitableObjectHitEventChannel hitableHitEventChannel,
            TargetHitTracker hitTracker)
        {
            session = levelSession;
            catalog = levelCatalog;
            this.playerDataService = playerDataService;
            this.gameplayObjectProvider = gameplayObjectProvider;
            hierarchyManager = knivesHierarchy;
            preStuckSpawner = preStuckKnifeSpawner;
            hitableSpawner = hitableObjectSpawner;
            this.layoutApplier = layoutApplier;
            startStageIndex = Mathf.Max(1, startingStageIndex);
            startLevelIndex = Mathf.Max(1, startingLevelIndex);
            impactEventChannel = eventChannel;
            hitableEventChannel = hitableHitEventChannel;
            targetHitTracker = hitTracker;

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
        public int CurrentStageIndex => session.StageIndex;
        public int CurrentLevelIndex => session.LevelIndex;

        /// <summary>
        /// Performs the StartInitialLevelAsync operation.
        /// </summary>
        public UniTask StartInitialLevelAsync()
        {
            int stage = playerDataService.GetCurrentStage(startStageIndex);
            return StartLevelAsync(stage, 1);
        }

        /// <summary>
        /// Performs the StartLevelAsync operation.
        /// </summary>
        public async UniTask StartLevelAsync(int stageIndex, int levelIndex)
        {
            ResetThrowerTweens();
            await RebuildKnifeHierarchyAsync();
            Transform knivesParent = hierarchyManager.EnsureParent();

            ApplyRules(stageIndex, levelIndex);
            ApplyStageLayout(stageIndex);
            ConfigureTarget(currentRules);
            ConfigureThrower(knivesParent, currentRules);

            await UniTask.WaitForEndOfFrame();
            SpawnHitableObjects();
            int preStuckSpawned = preStuckSpawner.Spawn(currentLevelData);
            session.SeedPreStuckKnives(preStuckSpawned);
            targetHitTracker?.InitializeTargets(currentLevelData);
            
            if (layoutApplier != null)
            {
                layoutApplier.SetTargetGroupManager(gameplayObjectProvider.GetTargetGroupManager());
                ApplyTargetTextures(stageIndex);
            }
        }

        /// <summary>
        /// Performs the RetryLevelAsync operation.
        /// </summary>
        public UniTask RetryLevelAsync()
        {
            return StartLevelAsync(session.StageIndex, session.LevelIndex);
        }

        /// <summary>
        /// Performs the NextLevelAsync operation.
        /// </summary>
        public UniTask NextLevelAsync()
        {
            int currentStage = session.StageIndex;
            int currentLevel = session.LevelIndex;
            int levelCountInStage = catalog.GetLevelCountInStage(currentStage);

            if (currentLevel < levelCountInStage)
            {
                return StartLevelAsync(currentStage, currentLevel + 1);
            }
            else
            {
                int nextStage = currentStage + 1;
                if (catalog.IsValidStageAndLevel(nextStage, 1))
                {
                    if (playerDataService != null)
                    {
                        playerDataService.SetCurrentStage(nextStage);
                    }
                    return StartLevelAsync(nextStage, 1);
                }
                else
                {
                    if (playerDataService != null)
                    {
                        playerDataService.SetCurrentStage(currentStage);
                    }
                    return StartLevelAsync(currentStage, currentLevel);
                }
            }
        }

        /// <summary>
        /// Resets to level 1 of the current stage.
        /// </summary>
        public UniTask ResetToStageStartAsync()
        {
            return StartLevelAsync(session.StageIndex, 1);
        }

        /// <summary>
        /// Performs the HandleKnifeStuck operation.
        /// </summary>
        public void HandleKnifeStuck()
        {
            session.RecordKnifeStuck();
            CheckCompletion();
        }

        /// <summary>
        /// Performs the HandleHitableObjectHit operation.
        /// </summary>
        private void HandleHitableObjectHit(Vector3 _)
        {
            session.RecordHitableObjectHit();
            playerDataService?.AddFruitsHit(1);
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
            CheckCompletion();
        }

        /// <summary>
        /// Records failure when a knife hits another knife.
        /// </summary>
        public void RecordKnifeToKnifeFailure()
        {
            RecordFailure();
        }

        /// <summary>
        /// Records failure when a knife goes out of bounds without hitting a target.
        /// </summary>
        public void RecordOutOfBoundsFailure()
        {
            RecordFailure();
        }

        /// <summary>
        /// Records a failure and disables thrower input.
        /// </summary>
        private void RecordFailure()
        {
            if (session == null || session.IsFailed || session.IsComplete)
            {
                return;
            }

            session.RecordFailure();
            DisableThrowerInput();
        }

        /// <summary>
        /// Checks if level should complete or fail based on current state.
        /// </summary>
        private void CheckCompletion()
        {
            if (session.IsComplete || session.IsFailed)
            {
                return;
            }

            session.MarkCompleteIfNeeded();

            if (session.IsComplete)
            {
                DisableThrowerInput();
                SaveProgress();
            }
            else if (session.HasExhaustedKnives() && session.TargetsRemaining > 0)
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
            int currentStage = session.StageIndex;
            int currentLevel = session.LevelIndex;
            int levelCountInStage = catalog.GetLevelCountInStage(currentStage);

            if (playerDataService == null)
            {
                Debug.LogError("PlayerDataService is null! Cannot save progress.");
                return;
            }

            if (currentLevel >= levelCountInStage)
            {
                int nextStage = currentStage + 1;
                if (catalog.IsValidStageAndLevel(nextStage, 1))
                {
                    playerDataService.SetCurrentStage(nextStage);
                }
                else
                {
                    playerDataService.SetCurrentStage(currentStage);
                }
            }
            else
            {
                playerDataService.SetCurrentStage(currentStage);
            }
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
        private void ApplyRules(int stageIndex, int levelIndex)
        {
            currentLevelData = catalog.GetLevelData(stageIndex, levelIndex);
            currentRules = catalog.GetRules(stageIndex, levelIndex);
            session.Begin(stageIndex, levelIndex, currentRules);
        }

        /// <summary>
        /// Applies stage layout settings (background texture).
        /// </summary>
        private void ApplyStageLayout(int stageIndex)
        {
            if (layoutApplier == null)
            {
                return;
            }

            StageLayoutSettings settings = catalog.GetStageLayoutSettings(stageIndex);
            layoutApplier.ApplyLayout(settings);
        }

        /// <summary>
        /// Applies target textures to all spawned targets.
        /// </summary>
        private void ApplyTargetTextures(int stageIndex)
        {
            if (layoutApplier == null)
            {
                return;
            }

            StageLayoutSettings settings = catalog.GetStageLayoutSettings(stageIndex);
            if (settings != null && settings.targetSprite != null)
            {
                layoutApplier.ApplyTargetTexture(settings.targetSprite);
            }
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
