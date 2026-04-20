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
    /// Controls the standalone stages menu scene.
    /// </summary>
    public class StagesMenuController : MonoBehaviour
    {
        [SerializeField] private StagesCollection _StagesCollection;
        [SerializeField] private UiStageItem _StageItemPrefab;
        [SerializeField] private Transform _ContentRoot;
        [SerializeField] private Button _BackButton;

        [Inject] private PlayerDataService _PlayerDataService;
        [Inject] private SceneLoaderService _SceneLoaderService;
        [Inject] private GameData _GameData;
        [Inject] private RewardClaimTracker _RewardClaimTracker;
        [Inject] private StageRewardClaimService _StageRewardClaimService;

        private StageListPresenter _StageListPresenter;
        private readonly List<UiStageItem> _StageItems = new();

        private void Start()
        {
            if (_StagesCollection == null || _StageItemPrefab == null || _ContentRoot == null)
            {
                Debug.LogError("StagesMenuController is missing required stage list references.");
                enabled = false;
                return;
            }

            if (_PlayerDataService == null || _GameData == null)
            {
                Debug.LogError("StagesMenuController is missing injected progression services.");
                enabled = false;
                return;
            }

            _StageListPresenter = new StageListPresenter(_PlayerDataService, _GameData, _RewardClaimTracker);
            BuildStageItems();
        }

        private void OnEnable()
        {
            if (_BackButton != null)
            {
                _BackButton.onClick.AddListener(OnBackClicked);
            }
        }

        private void OnDisable()
        {
            if (_BackButton != null)
            {
                _BackButton.onClick.RemoveListener(OnBackClicked);
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
        }

        private void OnBackClicked()
        {
            _SceneLoaderService.LoadMainMenu().Forget();
        }

        private void OnStageSelected(int stageIndex)
        {
            if (_StageListPresenter == null || !_StageListPresenter.TrySelectStage(_StagesCollection, stageIndex))
            {
                return;
            }

            _SceneLoaderService.LoadGame().Forget();
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
