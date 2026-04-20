using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using Game.Flow.Game;
using Game.Level.Catalogs;
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
        [SerializeField] private StagesCollection _StagesCollection;
        [SerializeField] private bool _LoadGameOnStageSelected;
        [SerializeField] private bool _ScrollToCurrentStageOnStart;
        [SerializeField] private bool _FlipOrientation;

        [Header("UI References")]
        [SerializeField] private UiStageItem _StageItemPrefab;
        [SerializeField] private Transform _ContentRoot;
        [SerializeField] private ScrollRect _ScrollRect;

        [Inject] private PlayerDataService _PlayerDataService;
        [Inject] private SceneLoaderService _SceneLoaderService;
        [Inject] private GameData _GameData;
        [Inject] private RewardClaimTracker _RewardClaimTracker;
        [Inject] private StageRewardClaimService _StageRewardClaimService;

        private StageListPresenter _StageListPresenter;
        private readonly List<UiStageItem> _StageItems = new();

        private void Start()
        {
            if (_StagesCollection == null)
            {
                Debug.LogError("StagesCollection is not assigned in StageListController.");
                return;
            }

            if (_PlayerDataService == null || _GameData == null)
            {
                Debug.LogWarning("StageListController is missing injected progression services and will stay inactive.");
                enabled = false;
                return;
            }

            _StageListPresenter = new StageListPresenter(_PlayerDataService, _GameData, _RewardClaimTracker);
            BuildStageItems();

            if (_ScrollToCurrentStageOnStart)
            {
                ScrollToCurrentStage();
            }
        }

        private void BuildStageItems()
        {
            if (_StageListPresenter == null || _StagesCollection == null || _StageItemPrefab == null || _ContentRoot == null)
            {
                return;
            }

            ClearStageItems();

            var stages = _StagesCollection.Stages;
            if (stages == null)
            {
                return;
            }

            for (int stageIndex = 0; stageIndex < stages.Count; stageIndex++)
            {
                var itemData = _StageListPresenter.GetItemData(_StagesCollection, stageIndex);
                if (itemData == null)
                {
                    _StageItems.Add(null);
                    continue;
                }

                var item = Instantiate(_StageItemPrefab, _ContentRoot);
                item.Initialize(itemData, OnStageSelected, OnStageRewardClaimed);
                _StageItems.Add(item);
            }

            if (_FlipOrientation)
            {
                ApplyFlippedOrientation();
            }
        }

        private void OnStageSelected(int stageIndex)
        {
            if (_StageListPresenter == null)
            {
                return;
            }

            int previousFocusedStageIndex = _StageListPresenter.GetFocusedStageIndex(_StagesCollection);
            if (!_StageListPresenter.TrySelectStage(_StagesCollection, stageIndex))
            {
                return;
            }

            if (_LoadGameOnStageSelected)
            {
                _SceneLoaderService.LoadGame().Forget();
                return;
            }

            int currentFocusedStageIndex = _StageListPresenter.GetFocusedStageIndex(_StagesCollection);
            UpdateStageItem(previousFocusedStageIndex);
            if (currentFocusedStageIndex != previousFocusedStageIndex)
            {
                UpdateStageItem(currentFocusedStageIndex);
            }
        }

        private void OnStageRewardClaimed(int stageIndex)
        {
            ClaimStageRewardAsync(stageIndex).Forget();
        }

        private async UniTaskVoid ClaimStageRewardAsync(int stageIndex)
        {
            if (_StagesCollection?.Stages == null || stageIndex < 0 || stageIndex >= _StagesCollection.Stages.Count)
            {
                return;
            }

            bool claimed = await _StageRewardClaimService.ClaimAsync(_StagesCollection.Stages[stageIndex], stageIndex);
            if (claimed)
            {
                UpdateStageItem(stageIndex);
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
            if (_ScrollRect == null || _StageListPresenter == null)
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

            int focusedStageIndex = _StageListPresenter.GetFocusedStageIndex(_StagesCollection);
            int visualIndex = _FlipOrientation
                ? itemCount - 1 - focusedStageIndex
                : focusedStageIndex;

            float normalizedOffset = (float)visualIndex / (itemCount - 1);
            _ScrollRect.verticalNormalizedPosition = 1f - normalizedOffset;
        }

        private void UpdateStageItem(int stageIndex)
        {
            if (_StageListPresenter == null || _StagesCollection == null || stageIndex < 0 || stageIndex >= _StageItems.Count)
            {
                return;
            }

            UiStageItem item = _StageItems[stageIndex];
            if (item == null)
            {
                return;
            }

            var itemData = _StageListPresenter.GetItemData(_StagesCollection, stageIndex);
            if (itemData == null)
            {
                return;
            }

            item.Initialize(itemData, OnStageSelected, OnStageRewardClaimed);
        }

        private void ClearStageItems()
        {
            _StageItems.Clear();

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
