using Cysharp.Threading.Tasks;
using Game.Flow.Game;
using Game.IAP.Services;
using Game.Level.Catalogs;
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
        [SerializeField] private StagesCollection _StagesCollection;
        [SerializeField] private UiElementAnimator[] _MenuAnimators;
        [SerializeField] private KnifeSkinSelectorView _KnifeSkinSelectorView;

        [Inject] private PlayerDataService _PlayerDataService;
        [Inject] private SceneLoaderService _SceneLoaderService;
        [Inject] private GameData _GameData;
        [Inject] private DailyMissionPopupController _MissionPopupController;
        [Inject] private KnifeSkinService _KnifeSkinService;
        [Inject] private MainMenuRemoveAdsFlow _RemoveAdsFlow;

        private bool _IsNavigating;
        private readonly List<KnifeSkinSelectionOption> _KnifeSkinOptions = new List<KnifeSkinSelectionOption>();
        private int _SelectedKnifeSkinIndex;

        private void Awake()
        {
            if (_RemoveAdsButton == null)
            {
                _RemoveAdsButton = FindButton("RemoveAdsButton");
            }
        }

        private void Start()
        {
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

            SyncCurrentStageSelection();
            UpdateCurrentStageText();
            UpdateBackground();
            RefreshRemoveAdsButtonState();
            InitializeKnifeSkinSelector();
            PlayMenuIn();
            _SceneLoaderService.PreloadGameFromMainMenu().Forget();
        }

        private void UpdateCurrentStageText()
        {
            if (_CurrentStageText != null && _GameData != null)
            {
                int stageNumber = _GameData.CurrentStageIndex + 1;
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
        }

        private void OnPlayClicked()
        {
            if (_GameData != null)
            {
                _GameData.CurrentStageIndex = _PlayerDataService.ClampStageIndexToUnlockedRange(
                    _GameData.CurrentStageIndex,
                    _PlayerDataService.GetHighestStageIndex() + 1);
            }

            LoadGameFromMainMenuAsync().Forget();
        }

        private void OnStagesClicked()
        {
            NavigateAsync(_SceneLoaderService.LoadStagesMenu).Forget();
        }

        private void OnShopClicked()
        {
            NavigateAsync(_SceneLoaderService.LoadStore).Forget();
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
            NavigateAsync(_SceneLoaderService.LoadPVP).Forget();
        }

        private void OnPowerUpsClicked()
        {
            NavigateAsync(_SceneLoaderService.LoadPowerUps).Forget();
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
            if (_GameData == null || _PlayerDataService == null)
            {
                return;
            }

            _GameData.CurrentStageIndex = _PlayerDataService.ClampStageIndexToUnlockedRange(
                _GameData.CurrentStageIndex,
                _PlayerDataService.GetHighestStageIndex() + 1);
        }

        private void UpdateBackground()
        {
            if (_BackgroundImage == null || _StagesCollection?.Stages == null || _StagesCollection.Stages.Count == 0 || _GameData == null)
            {
                return;
            }

            int stageIndex = Mathf.Clamp(_GameData.CurrentStageIndex, 0, _StagesCollection.Stages.Count - 1);
            var stageConfig = _StagesCollection.Stages[stageIndex];
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
            if (_MenuAnimators == null)
            {
                _KnifeSkinSelectorView?.PlayIn();
                return;
            }

            foreach (UiElementAnimator animator in _MenuAnimators)
            {
                animator?.PlayIn();
            }

            _KnifeSkinSelectorView?.PlayIn();
        }

        private async UniTaskVoid NavigateAsync(Func<UniTask> loadScene)
        {
            if (_IsNavigating || loadScene == null)
            {
                return;
            }

            _IsNavigating = true;
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
                SetButtonsInteractable(true);
            }
        }

        private async UniTaskVoid LoadGameFromMainMenuAsync()
        {
            if (_IsNavigating)
            {
                return;
            }

            _IsNavigating = true;
            SetButtonsInteractable(false);

            try
            {
                await PlayMenuOutAsync();
                await _SceneLoaderService.RevealPreloadedGameFromMainMenu();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                _IsNavigating = false;
                SetButtonsInteractable(true);
            }
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
                _RemoveAdsButton.interactable = isInteractable && _RemoveAdsFlow.CanPurchase;
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

            SetButtonsInteractable(false);
            try
            {
                await _RemoveAdsFlow.TryPurchaseAsync(this.GetCancellationTokenOnDestroy());
            }
            finally
            {
                SetButtonsInteractable(true);
                RefreshRemoveAdsButtonState();
            }
        }

        private async UniTask PlayMenuOutAsync()
        {
            var tasks = new List<UniTask>();

            if (_MenuAnimators != null)
            {
                for (var i = 0; i < _MenuAnimators.Length; i++)
                {
                    var animator = _MenuAnimators[i];
                    if (animator != null)
                    {
                        tasks.Add(animator.PlayOutAsync());
                    }
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
    }
}
