using Cysharp.Threading.Tasks;
using Game.Flow.Game;
using Game.IAP.Services;
using Game.Feedback.Data;
using Game.Feedback.Events;
using Game.Level.Interfaces;
using Game.Level.Configurations.Stage;
using Game.Level.Catalogs;
using Game.Level.Services;
using Game.Persistence.Services;
using Game.Store.Data;
using Game.Store.Services;
using Game.UI.Common;
using Game.UI.Missions;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Game.UI.MainMenu
{
    /// <summary>
    /// Controller for the main menu scene. Handles navigation to gameplay and stages menu.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button _PlayButton;
        [SerializeField] private Button _StagesButton;
        [SerializeField] private Button _ShopButton;
        [SerializeField] private Button _RemoveAdsButton;
        [SerializeField] private Button _PvpButton;
        [SerializeField] private Button _PowerUpsButton;
        [SerializeField] private Button _MissionsButton;
        [SerializeField] private TMP_Text _CurrentStageText;
        [SerializeField] private Image _BackgroundImage;
        [SerializeField] private UiElementAnimator[] _MenuAnimators;
        [SerializeField] private KnifeSkinSelectorView _KnifeSkinSelectorView;

        [Inject] private PlayerDataService _PlayerDataService;
        [Inject] private SceneLoaderService _SceneLoaderService;
        [Inject] private GameData _GameData;
        [Inject] private DailyMissionPopupController _MissionPopupController;
        [Inject] private KnifeSkinService _KnifeSkinService;
        [Inject] private MainMenuRemoveAdsFlow _RemoveAdsFlow;
        [Inject] private IStagesCollectionCatalog _StagesCollectionCatalog;
        [Inject] private StagesCollectionResolver _StagesCollectionResolver;
        [Inject] private WorldAssetService _WorldAssetService;
        [Inject] private FeedbackEventChannel _FeedbackEventChannel;

        private bool _IsNavigating;
        private readonly List<KnifeSkinSelectionOption> _KnifeSkinOptions = new List<KnifeSkinSelectionOption>();
        private int _SelectedKnifeSkinIndex;
        private ActionButtonPresenter _ActiveNavigationPresenter;
        private ActionButtonPresenter _RemoveAdsPresenter;

        private void Awake()
        {
            if (_RemoveAdsButton == null)
            {
                _RemoveAdsButton = FindButton("RemoveAdsButton");
            }
        }

        private void Start()
        {
            ConfigureActionButtons();
            _PlayButton.onClick.AddListener(OnPlayClicked);
            _StagesButton.onClick.AddListener(OnStagesClicked);

            if (_ShopButton != null)
                _ShopButton.onClick.AddListener(OnShopClicked);

            if (_RemoveAdsButton != null)
                _RemoveAdsButton.onClick.AddListener(OnRemoveAdsClicked);
            
            if (_PvpButton != null)
                _PvpButton.onClick.AddListener(OnPvpClicked);

            if (_PowerUpsButton != null)
                _PowerUpsButton.onClick.AddListener(OnPowerUpsClicked);

            if (_MissionsButton != null)
                _MissionsButton.onClick.AddListener(OnMissionsClicked);

            if (_SceneLoaderService != null)
                _SceneLoaderService.StoreUnloaded += OnStoreUnloaded;

            StartMenuAsync().Forget();
        }

        private async UniTaskVoid StartMenuAsync()
        {
            SyncCurrentStageSelection();
            if (_WorldAssetService != null && _GameData != null)
            {
                await _WorldAssetService.LoadCollectionAsync(_GameData.CurrentCollectionIndex);
            }

            UpdateCurrentStageText();
            UpdateBackground();
            RefreshRemoveAdsButtonState();
            InitializeKnifeSkinSelector();
            PlayMenuIn();
            _SceneLoaderService.PreloadGameFromMainMenu().Forget();
        }

        private void UpdateCurrentStageText()
        {
            if (_CurrentStageText != null && _GameData != null && _StagesCollectionCatalog != null)
            {
                int stageNumber = _StagesCollectionCatalog.GetCumulativeStageNumberDisplay(
                    _GameData.CurrentCollectionIndex,
                    _GameData.CurrentStageIndex);
                _CurrentStageText.text = $"Stage {stageNumber}";
            }
        }

        private void OnDestroy()
        {
            _PlayButton.onClick.RemoveListener(OnPlayClicked);
            _StagesButton.onClick.RemoveListener(OnStagesClicked);

            if (_ShopButton != null)
                _ShopButton.onClick.RemoveListener(OnShopClicked);

            if (_RemoveAdsButton != null)
                _RemoveAdsButton.onClick.RemoveListener(OnRemoveAdsClicked);
            
            if (_PvpButton != null)
                _PvpButton.onClick.RemoveListener(OnPvpClicked);

            if (_PowerUpsButton != null)
                _PowerUpsButton.onClick.RemoveListener(OnPowerUpsClicked);

            if (_MissionsButton != null)
                _MissionsButton.onClick.RemoveListener(OnMissionsClicked);

            if (_SceneLoaderService != null)
                _SceneLoaderService.StoreUnloaded -= OnStoreUnloaded;
        }

        private void OnPlayClicked()
        {
            if (_GameData != null && _PlayerDataService != null && _StagesCollectionCatalog != null)
            {
                _PlayerDataService.ClampSelectionToUnlocked(_StagesCollectionCatalog);
            }

            LeaveMainMenuAsync(_SceneLoaderService.RevealPreloadedGameFromMainMenu, _PlayButton).Forget();
        }

        private void OnStagesClicked()
        {
            LeaveMainMenuAsync(_SceneLoaderService.LoadStagesMenu, _StagesButton).Forget();
        }

        private void OnShopClicked()
        {
            LeaveMainMenuAsync(_SceneLoaderService.LoadStore, _ShopButton).Forget();
        }

        private void OnRemoveAdsClicked()
        {
            if (_IsNavigating)
            {
                return;
            }

            TryPurchaseRemoveAdsAsync().Forget();
        }

        private void OnPvpClicked()
        {
            LeaveMainMenuAsync(_SceneLoaderService.LoadPVP, _PvpButton).Forget();
        }

        private void OnPowerUpsClicked()
        {
            LeaveMainMenuAsync(_SceneLoaderService.LoadPowerUps, _PowerUpsButton).Forget();
        }

        private void OnMissionsClicked()
        {
            _MissionPopupController?.ShowMissions();
        }

        private void OnPreviousKnifeSkinClicked()
        {
            CycleKnifeSkin(-1);
        }

        private void OnNextKnifeSkinClicked()
        {
            CycleKnifeSkin(1);
        }

        private void SyncCurrentStageSelection()
        {
            if (_GameData == null || _PlayerDataService == null || _StagesCollectionCatalog == null)
            {
                return;
            }

            _PlayerDataService.ClampSelectionToUnlocked(_StagesCollectionCatalog);
        }

        private void UpdateBackground()
        {
            StagesCollection active = _StagesCollectionResolver?.GetActiveCollection();
            if (_BackgroundImage == null || active?.Stages == null || active.Stages.Count == 0 || _GameData == null)
            {
                return;
            }

            int stageIndex = Mathf.Clamp(_GameData.CurrentStageIndex, 0, active.Stages.Count - 1);
            var stageConfig = active.Stages[stageIndex];
            var backgroundSprite = stageConfig?.LayoutSettings?.BackgroundSprite;
            if (backgroundSprite == null)
            {
                return;
            }

            _BackgroundImage.sprite = backgroundSprite;
            _BackgroundImage.preserveAspect = false;
        }

        private void PlayMenuIn()
        {
            foreach (UiElementAnimator animator in GetMenuAnimators())
            {
                animator?.PlayIn();
            }

            _KnifeSkinSelectorView?.PlayIn();
        }

        private async UniTaskVoid LeaveMainMenuAsync(Func<UniTask> loadScene, Button sourceButton)
        {
            if (_IsNavigating || loadScene == null)
            {
                return;
            }

            ActionButtonPresenter presenter = ActionButtonPresenter.GetOrAdd(sourceButton, _FeedbackEventChannel);
            if (presenter != null && !presenter.TryBeginSubmission())
            {
                return;
            }

            _IsNavigating = true;
            _ActiveNavigationPresenter = presenter;
            SetButtonsInteractable(false);

            try
            {
                await PlayMenuOutAsync();
                await loadScene();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                _IsNavigating = false;
                presenter?.PresentBlocked(new FeedbackEventData(
                    FeedbackKind.Blocked,
                    "navigation.failed",
                    sourceButton != null ? sourceButton.gameObject.name : "main_menu"));
                _ActiveNavigationPresenter = null;
                SetButtonsInteractable(true);
                PlayMenuIn();
            }
        }

        private void OnStoreUnloaded()
        {
            _IsNavigating = false;
            _ActiveNavigationPresenter?.CompleteSubmission();
            _ActiveNavigationPresenter = null;
            SetButtonsInteractable(true);
            PlayMenuIn();
        }

        private void SetButtonsInteractable(bool isInteractable)
        {
            if (_PlayButton != null)
            {
                _PlayButton.interactable = isInteractable;
            }

            if (_StagesButton != null)
            {
                _StagesButton.interactable = isInteractable;
            }

            if (_ShopButton != null)
            {
                _ShopButton.interactable = isInteractable;
            }

            if (_RemoveAdsButton != null)
            {
                _RemoveAdsButton.interactable = isInteractable &&
                                               _RemoveAdsFlow != null &&
                                               _RemoveAdsFlow.CanPurchase;
            }

            if (_PvpButton != null)
            {
                _PvpButton.interactable = isInteractable;
            }

            if (_PowerUpsButton != null)
            {
                _PowerUpsButton.interactable = isInteractable;
            }

            if (_MissionsButton != null)
            {
                _MissionsButton.interactable = isInteractable;
            }

            RefreshKnifeSkinSelector(isInteractable);
        }

        private async UniTaskVoid TryPurchaseRemoveAdsAsync()
        {
            if (_IsNavigating)
            {
                return;
            }

            if (_RemoveAdsPresenter != null && !_RemoveAdsPresenter.TryBeginSubmission())
            {
                return;
            }

            SetButtonsInteractable(false);
            try
            {
                await _RemoveAdsFlow.TryPurchaseAsync(this.GetCancellationTokenOnDestroy());
            }
            finally
            {
                _RemoveAdsPresenter?.CompleteSubmission();
                SetButtonsInteractable(true);
                RefreshRemoveAdsButtonState();
            }
        }

        private async UniTask PlayMenuOutAsync()
        {
            var tasks = new List<UniTask>();

            foreach (UiElementAnimator animator in GetMenuAnimators())
            {
                if (animator != null)
                {
                    tasks.Add(animator.PlayOutAsync());
                }
            }

            if (_KnifeSkinSelectorView != null)
            {
                tasks.Add(_KnifeSkinSelectorView.PlayOutAsync());
            }

            if (tasks.Count > 0)
            {
                await UniTask.WhenAll(tasks);
            }
        }

        private IReadOnlyList<UiElementAnimator> GetMenuAnimators()
        {
            var animators = new List<UiElementAnimator>();
            var seen = new HashSet<UiElementAnimator>();

            AddMenuAnimators(_MenuAnimators, animators, seen);
            AddMenuAnimators(GetComponentsInChildren<UiElementAnimator>(true), animators, seen);

            return animators;
        }

        private static void AddMenuAnimators(
            IReadOnlyList<UiElementAnimator> source,
            List<UiElementAnimator> destination,
            HashSet<UiElementAnimator> seen)
        {
            if (source == null)
            {
                return;
            }

            for (var i = 0; i < source.Count; i++)
            {
                UiElementAnimator animator = source[i];
                if (animator != null && seen.Add(animator))
                {
                    destination.Add(animator);
                }
            }
        }

        private void InitializeKnifeSkinSelector()
        {
            if (_KnifeSkinService == null || _KnifeSkinSelectorView == null)
            {
                return;
            }

            _KnifeSkinSelectorView.Bind(OnPreviousKnifeSkinClicked, OnNextKnifeSkinClicked);
            RefreshKnifeSkinSelector(!_IsNavigating);
        }

        private void RefreshKnifeSkinSelector(bool canInteract)
        {
            if (_KnifeSkinSelectorView == null || _KnifeSkinService == null)
            {
                return;
            }

            _KnifeSkinOptions.Clear();
            var options = _KnifeSkinService.GetSelectableOptions();
            for (var i = 0; i < options.Count; i++)
            {
                _KnifeSkinOptions.Add(options[i]);
            }

            if (_KnifeSkinOptions.Count == 0)
            {
                _KnifeSkinSelectorView.gameObject.SetActive(false);
                return;
            }

            _SelectedKnifeSkinIndex = Mathf.Clamp(_KnifeSkinService.GetSelectedOptionIndex(_KnifeSkinOptions), 0, _KnifeSkinOptions.Count - 1);
            var selectedOption = _KnifeSkinOptions[_SelectedKnifeSkinIndex];
            _KnifeSkinSelectorView.gameObject.SetActive(true);
            _KnifeSkinSelectorView.SetDisplay(
                selectedOption.PreviewSprite,
                _KnifeSkinOptions.Count > 1,
                canInteract);
        }

        private void CycleKnifeSkin(int direction)
        {
            if (_KnifeSkinService == null || _KnifeSkinOptions.Count <= 1)
            {
                return;
            }

            _SelectedKnifeSkinIndex = (_SelectedKnifeSkinIndex + direction + _KnifeSkinOptions.Count) % _KnifeSkinOptions.Count;
            var selectedOption = _KnifeSkinOptions[_SelectedKnifeSkinIndex];
            if (_KnifeSkinService.TryEquip(selectedOption.SkinId))
            {
                RefreshKnifeSkinSelector(!_IsNavigating);
            }
        }

        private Button FindButton(string objectName)
        {
            var buttons = GetComponentsInChildren<Button>(true);
            for (var i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] != null && buttons[i].gameObject.name == objectName)
                {
                    return buttons[i];
                }
            }

            return null;
        }

        private void RefreshRemoveAdsButtonState()
        {
            if (_RemoveAdsButton == null)
            {
                return;
            }

            if (_RemoveAdsFlow == null)
            {
                _RemoveAdsButton.gameObject.SetActive(false);
                return;
            }

            _RemoveAdsButton.gameObject.SetActive(_RemoveAdsFlow.IsAvailable);
            _RemoveAdsButton.interactable = !_IsNavigating && _RemoveAdsFlow.CanPurchase;
        }

        private void ConfigureActionButtons()
        {
            ActionButtonPresenter.GetOrAdd(_PlayButton, _FeedbackEventChannel);
            ActionButtonPresenter.GetOrAdd(_StagesButton, _FeedbackEventChannel);
            ActionButtonPresenter.GetOrAdd(_ShopButton, _FeedbackEventChannel);
            _RemoveAdsPresenter = ActionButtonPresenter.GetOrAdd(_RemoveAdsButton, _FeedbackEventChannel);
            ActionButtonPresenter.GetOrAdd(_PvpButton, _FeedbackEventChannel);
            ActionButtonPresenter.GetOrAdd(_PowerUpsButton, _FeedbackEventChannel);
            ActionButtonPresenter.GetOrAdd(_MissionsButton, _FeedbackEventChannel);
        }
    }
}
