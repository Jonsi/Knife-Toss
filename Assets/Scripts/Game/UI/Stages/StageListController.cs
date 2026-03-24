using Cysharp.Threading.Tasks;
using Game.Flow.Game;
using Game.Level.Catalogs;
using Game.Persistence.Services;
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

        private StageListPresenter _StageListPresenter;

        private void Start()
        {
            if (_StagesCollection == null)
            {
                Debug.LogError("StagesCollection is not assigned in StageListController.");
                return;
            }

            _StageListPresenter = new StageListPresenter(_PlayerDataService, _GameData);
            Refresh();

            if (_ScrollToCurrentStageOnStart)
            {
                ScrollToCurrentStage();
            }
        }

        private void Refresh()
        {
            _StageListPresenter?.Rebuild(_StagesCollection, _StageItemPrefab, _ContentRoot, OnStageSelected);

            if (_FlipOrientation)
            {
                ApplyFlippedOrientation();
            }
        }

        private void OnStageSelected(int stageIndex)
        {
            if (_StageListPresenter == null || !_StageListPresenter.TrySelectStage(_StagesCollection, stageIndex))
            {
                return;
            }

            if (_LoadGameOnStageSelected)
            {
                _SceneLoaderService.LoadGame().Forget();
                return;
            }

            Refresh();
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
    }
}
