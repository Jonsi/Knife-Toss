using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using Game.Flow.Game;
using Game.Level.Catalogs;
using Game.Level.Configurations.Stage;
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
        [SerializeField] private UiWorldStageRow _WorldRowPrefab;
        [SerializeField] private Transform _ContentRoot;
        [SerializeField] private Button _BackButton;

        [Inject] private PlayerDataService _PlayerDataService;
        [Inject] private SceneLoaderService _SceneLoaderService;
        [Inject] private GameData _GameData;
        [Inject] private RewardClaimTracker _RewardClaimTracker;
        [Inject] private StageRewardClaimService _StageRewardClaimService;
        [Inject] private StagesCollectionComposite _StagesComposite;

        private StageListPresenter _StageListPresenter;
        private readonly List<UiWorldStageRow> _WorldRows = new List<UiWorldStageRow>();

        private void Start()
        {
            if (_StagesComposite == null || _WorldRowPrefab == null || _ContentRoot == null)
            {
                enabled = false;
                return;
            }

            if (_PlayerDataService == null || _GameData == null)
            {
                enabled = false;
                return;
            }

            _StageListPresenter = new StageListPresenter(_PlayerDataService, _GameData, _RewardClaimTracker, _StagesComposite);
            BuildWorldRows();
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

        private void BuildWorldRows()
        {
            ClearWorldRows();

            int count = _StagesComposite.CollectionsCount;
            for (int collectionIndex = 0; collectionIndex < count; collectionIndex++)
            {
                StagesCollection collection = _StagesComposite.GetCollection(collectionIndex);
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
        }

        private void OnBackClicked()
        {
            _SceneLoaderService.LoadMainMenu().Forget();
        }

        private void OnStageSelected(int collectionIndex, int localStageIndex)
        {
            if (_StageListPresenter == null || _StagesComposite == null)
            {
                return;
            }

            StagesCollection collection = _StagesComposite.GetCollection(collectionIndex);
            if (collection == null)
            {
                return;
            }

            if (!_StageListPresenter.TrySelectStage(collection, collectionIndex, localStageIndex))
            {
                return;
            }

            _SceneLoaderService.LoadGame().Forget();
        }

        private void OnStageRewardClaimed(int collectionIndex, int localStageIndex)
        {
            ClaimStageRewardAsync(collectionIndex, localStageIndex).Forget();
        }

        private async UniTaskVoid ClaimStageRewardAsync(int collectionIndex, int localStageIndex)
        {
            StagesCollection collection = _StagesComposite?.GetCollection(collectionIndex);
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
