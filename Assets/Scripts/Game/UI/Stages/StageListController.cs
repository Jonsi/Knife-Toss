using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using Common.AssetLoading;
using Game.Flow.Game;
using Game.Installers;
using Game.Level.Catalogs;
using Game.Level.Interfaces;
using Game.Persistence.Services;
using Game.Rewards.Services;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Game.UI.Stages
{
    /// <summary>
    /// Reusable stage list controller for both standalone and in-screen stage selection UIs.
    /// </summary>
    public class StageListController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private bool _LoadGameOnStageSelected;
        [SerializeField] private bool _ScrollToCurrentStageOnStart;
        [SerializeField] private bool _FlipOrientation;

        [Header("UI References")]
        [SerializeField] private UiWorldStageRow _WorldRowPrefab;
        [SerializeField] private Transform _ContentRoot;
        [SerializeField] private ScrollRect _ScrollRect;

        [Inject] private PlayerDataService _PlayerDataService;
        [Inject] private SceneLoaderService _SceneLoaderService;
        [Inject] private GameData _GameData;
        [Inject] private RewardClaimTracker _RewardClaimTracker;
        [Inject] private StageRewardClaimService _StageRewardClaimService;
        [Inject] private IStagesCollectionCatalog _StagesCollectionCatalog;
        [Inject] private IAssetLoader _AssetLoader;

        private StageListPresenter _StageListPresenter;
        private readonly List<UiWorldStageRow> _WorldRows = new List<UiWorldStageRow>();
        private readonly List<ILoadedAsset<StagesCollection>> _MenuWorldLoads = new List<ILoadedAsset<StagesCollection>>();

        private void Start()
        {
            TryInjectFromSceneScopes();

            if (_StagesCollectionCatalog == null)
            {
                enabled = false;
                return;
            }

            if (_PlayerDataService == null || _GameData == null || _AssetLoader == null)
            {
                enabled = false;
                return;
            }

            _StageListPresenter = new StageListPresenter(_PlayerDataService, _GameData, _RewardClaimTracker, _StagesCollectionCatalog);
            BuildWorldRowsWhenReadyAsync().Forget();
        }

        private void OnDestroy()
        {
            ReleaseMenuWorldLoads();
        }

        private void TryInjectFromSceneScopes()
        {
            if (_StagesCollectionCatalog != null && _PlayerDataService != null && _GameData != null && _AssetLoader != null)
            {
                return;
            }

            GameObject[] roots = gameObject.scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                StageInstaller stageInstaller = roots[i].GetComponent<StageInstaller>();
                if (stageInstaller != null && stageInstaller.Container != null)
                {
                    stageInstaller.Container.Inject(this);
                    return;
                }
            }

            for (int i = 0; i < roots.Length; i++)
            {
                StagesMenuInstaller menuInstaller = roots[i].GetComponent<StagesMenuInstaller>();
                if (menuInstaller != null && menuInstaller.Container != null)
                {
                    menuInstaller.Container.Inject(this);
                    return;
                }
            }
        }

        private async UniTaskVoid BuildWorldRowsWhenReadyAsync()
        {
            await BuildWorldRowsAsync();

            if (_ScrollToCurrentStageOnStart)
            {
                ScrollToCurrentStage();
            }
        }

        private async UniTask BuildWorldRowsAsync()
        {
            ClearWorldRows();

            if (_WorldRowPrefab == null || _ContentRoot == null || _StageListPresenter == null)
            {
                return;
            }

            ReleaseMenuWorldLoads();

            int count = _StagesCollectionCatalog.CollectionsCount;
            for (int i = 0; i < count; i++)
            {
                _MenuWorldLoads.Add(null);
            }

            for (int collectionIndex = 0; collectionIndex < count; collectionIndex++)
            {
                string locationKey = _StagesCollectionCatalog.GetCollectionLocationKey(collectionIndex);
                if (string.IsNullOrEmpty(locationKey))
                {
                    continue;
                }

                ILoadedAsset<StagesCollection> loaded = await _AssetLoader.LoadAsync<StagesCollection>(locationKey);
                _MenuWorldLoads[collectionIndex] = loaded;
                if (loaded == null)
                {
                    continue;
                }

                StagesCollection collection = loaded.Asset;
                if (collection == null)
                {
                    continue;
                }

                UiWorldStageRow row = Instantiate(_WorldRowPrefab, _ContentRoot);
                int capturedCollection = collectionIndex;
                row.Initialize(
                    collection,
                    collectionIndex,
                    _StageListPresenter,
                    local => OnStageSelected(capturedCollection, local),
                    local => OnStageRewardClaimed(capturedCollection, local));
                _WorldRows.Add(row);
            }

            if (_FlipOrientation)
            {
                ApplyFlippedOrientation();
            }
        }

        private void ReleaseMenuWorldLoads()
        {
            for (int i = 0; i < _MenuWorldLoads.Count; i++)
            {
                _MenuWorldLoads[i]?.Dispose();
            }

            _MenuWorldLoads.Clear();
        }

        private void OnStageSelected(int collectionIndex, int localStageIndex)
        {
            if (_StageListPresenter == null || _StagesCollectionCatalog == null)
            {
                return;
            }

            StagesCollection collection = GetLoadedMenuCollection(collectionIndex);
            if (collection == null)
            {
                return;
            }

            int previousCollection = _GameData.CurrentCollectionIndex;

            if (!_StageListPresenter.TrySelectStage(collection, collectionIndex, localStageIndex))
            {
                return;
            }

            if (_LoadGameOnStageSelected)
            {
                _SceneLoaderService.LoadGame().Forget();
                return;
            }

            RefreshRow(previousCollection);
            RefreshRow(collectionIndex);
        }

        private StagesCollection GetLoadedMenuCollection(int collectionIndex)
        {
            if (collectionIndex < 0 || collectionIndex >= _MenuWorldLoads.Count)
            {
                return null;
            }

            ILoadedAsset<StagesCollection> lease = _MenuWorldLoads[collectionIndex];
            return lease != null && lease.IsValid ? lease.Asset : null;
        }

        private void RefreshRow(int collectionIndex)
        {
            if (collectionIndex < 0 || collectionIndex >= _WorldRows.Count || _StageListPresenter == null)
            {
                return;
            }

            UiWorldStageRow row = _WorldRows[collectionIndex];
            row?.RefreshAllStages(
                _StageListPresenter,
                local => OnStageSelected(collectionIndex, local),
                local => OnStageRewardClaimed(collectionIndex, local));
        }

        private void OnStageRewardClaimed(int collectionIndex, int localStageIndex)
        {
            ClaimStageRewardAsync(collectionIndex, localStageIndex).Forget();
        }

        private async UniTaskVoid ClaimStageRewardAsync(int collectionIndex, int localStageIndex)
        {
            StagesCollection collection = GetLoadedMenuCollection(collectionIndex);
            if (collection?.Stages == null || localStageIndex < 0 || localStageIndex >= collection.Stages.Count)
            {
                return;
            }

            bool claimed = await _StageRewardClaimService.ClaimAsync(
                collection.Stages[localStageIndex],
                collectionIndex,
                localStageIndex);
            if (claimed && collectionIndex < _WorldRows.Count)
            {
                UiWorldStageRow row = _WorldRows[collectionIndex];
                row?.RefreshLocalStage(
                    localStageIndex,
                    _StageListPresenter,
                    local => OnStageSelected(collectionIndex, local),
                    local => OnStageRewardClaimed(collectionIndex, local));
            }
        }

        private void ApplyFlippedOrientation()
        {
            if (_ContentRoot == null)
            {
                return;
            }

            for (int childIndex = 0; childIndex < _ContentRoot.childCount; childIndex++)
            {
                _ContentRoot.GetChild(childIndex).SetSiblingIndex(0);
            }
        }

        private void ScrollToCurrentStage()
        {
            if (_ScrollRect == null || _GameData == null)
            {
                return;
            }

            int itemCount = _ContentRoot != null ? _ContentRoot.childCount : 0;
            if (itemCount <= 1)
            {
                _ScrollRect.verticalNormalizedPosition = 1f;
                return;
            }

            Canvas.ForceUpdateCanvases();

            int focusedCollectionIndex = _GameData.CurrentCollectionIndex;
            int visualIndex = _FlipOrientation
                ? itemCount - 1 - focusedCollectionIndex
                : focusedCollectionIndex;

            visualIndex = Mathf.Clamp(visualIndex, 0, itemCount - 1);
            float normalizedOffset = (float)visualIndex / (itemCount - 1);
            _ScrollRect.verticalNormalizedPosition = 1f - normalizedOffset;
        }

        private void ClearWorldRows()
        {
            _WorldRows.Clear();

            if (_ContentRoot == null)
            {
                return;
            }

            foreach (Transform child in _ContentRoot)
            {
                Destroy(child.gameObject);
            }
        }
    }
}
