using Cysharp.Threading.Tasks;
using System.Threading;
using KnifeToss.Core.Abstractions;
using KnifeToss.Core.Data;
using KnifeToss.Core.Domain;
using KnifeToss.Infrastructure.Leveling;
using KnifeToss.Presentation.Gameplay;
using UnityEngine;

namespace KnifeToss.Application.Level
{
    /// <summary>
    /// Handles loading and configuring levels.
    /// </summary>
    public sealed class LevelLoader
    {
        private readonly ILevelCatalog _catalog;
        private readonly GameplayObjectProvider _gameplayObjectProvider;
        private readonly KnivesHierarchyManager _hierarchyManager;
        private readonly HitableObjectSpawner _hitableSpawner;
        private readonly PreStuckKnifeSpawner _preStuckSpawner;

        public LevelLoader(
            ILevelCatalog catalog,
            GameplayObjectProvider gameplayObjectProvider,
            KnivesHierarchyManager hierarchyManager,
            HitableObjectSpawner hitableSpawner,
            PreStuckKnifeSpawner preStuckSpawner)
        {
            _catalog = catalog;
            _gameplayObjectProvider = gameplayObjectProvider;
            _hierarchyManager = hierarchyManager;
            _hitableSpawner = hitableSpawner;
            _preStuckSpawner = preStuckSpawner;
        }

        public async UniTask<LevelLoadResult> LoadLevelAsync(int levelIndex)
        {
            await RebuildKnifeHierarchyAsync();

            LevelData levelData = _catalog.GetLevelData(levelIndex);
            LevelRules rules = _catalog.GetRules(levelIndex);

            Transform knivesParent = _hierarchyManager.EnsureParent();
            ConfigureTarget(levelData);
            ConfigureThrower(knivesParent, rules);

            await UniTask.WaitForEndOfFrame();

            SpawnHitableObjects(levelData);
            int preStuckSpawned = _preStuckSpawner.Spawn(levelData);

            return new LevelLoadResult
            {
                LevelData = levelData,
                Rules = rules,
                PreStuckKnives = preStuckSpawned
            };
        }

        private async UniTask RebuildKnifeHierarchyAsync()
        {
            _hierarchyManager.ResetHierarchy();
            await UniTask.Yield();
        }

        private void ConfigureTarget(LevelData levelData)
        {
            TargetGroupManager targetGroupManager = _gameplayObjectProvider.GetTargetGroupManager();
            targetGroupManager.Configure(levelData, 0);
            targetGroupManager.ResumeAll();
        }

        private void ConfigureThrower(Transform knivesParent, LevelRules rules)
        {
            KnifeThrower knifeThrower = _gameplayObjectProvider.GetKnifeThrower();
            knifeThrower.ResetState();
            knifeThrower.SetScaleProvider(_gameplayObjectProvider.GetTargetGroupManager());
            knifeThrower.SetTotalKnives(rules.KnifeCount);
            knifeThrower.SetKnivesParent(knivesParent);
            knifeThrower.SetCanThrow(true);
        }

        private void SpawnHitableObjects(LevelData levelData)
        {
            _hitableSpawner?.Spawn(levelData);
        }

        public struct LevelLoadResult
        {
            public LevelData LevelData;
            public LevelRules Rules;
            public int PreStuckKnives;
        }
    }
}

