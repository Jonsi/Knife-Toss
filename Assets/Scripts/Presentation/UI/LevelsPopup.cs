using System.Collections.Generic;
using Core.Abstractions;
using Core.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Presentation.UI
{
    /// <summary>
    /// Populates the levels popup with level icons and progress data.
    /// </summary>
    public sealed class LevelsPopup : MonoBehaviour
    {
        [Header("Level Icons")]
        [SerializeField] private RectTransform levelsParent;
        [SerializeField] private LevelIconView levelIconPrefab;
        [SerializeField] private Button closeButton;
        [SerializeField] private int minimumLevelsToShow = 1;

        private readonly List<LevelIconView> iconViews = new List<LevelIconView>();
        private bool cachedExistingIcons;
        private bool dependenciesReady;
        private IPlayerDataService playerDataService;
        private LevelConfig levelConfig;
        private ILevelCommandInvoker commandInvoker;

        /// <summary>
        /// Injects dependencies used to populate the popup.
        /// </summary>
        [VContainer.Inject]
        public void Construct(IPlayerDataService dataService, LevelConfig config, ILevelCommandInvoker levelCommandInvoker)
        {
            playerDataService = dataService;
            levelConfig = config;
            commandInvoker = levelCommandInvoker;
            dependenciesReady = true;
            Refresh();
        }

        private void Awake()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(HidePopup);
            }
        }

        private void OnDestroy()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(HidePopup);
            }
        }

        private void OnEnable()
        {
            Refresh();
        }

        /// <summary>
        /// Refreshes the level icons based on the latest player data.
        /// </summary>
        public void Refresh()
        {
            if (!dependenciesReady || levelsParent == null || levelIconPrefab == null)
            {
                return;
            }

            CacheExistingIcons();

            int levelCount = DetermineLevelCount();
            EnsureIconPool(levelCount);
            UpdateIcons(levelCount);
        }

        /// <summary>
        /// Shows the popup and refreshes its contents.
        /// </summary>
        public void ShowPopup()
        {
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
                return;
            }

            Refresh();
        }

        /// <summary>
        /// Hides the popup.
        /// </summary>
        public void HidePopup()
        {
            if (gameObject.activeSelf)
            {
                gameObject.SetActive(false);
            }
        }

        private void CacheExistingIcons()
        {
            if (cachedExistingIcons || levelsParent == null)
            {
                return;
            }

            cachedExistingIcons = true;
            LevelIconView[] existingIcons = levelsParent.GetComponentsInChildren<LevelIconView>(true);
            if (existingIcons == null)
            {
                return;
            }

            foreach (LevelIconView icon in existingIcons)
            {
                if (icon == null || iconViews.Contains(icon))
                {
                    continue;
                }

                iconViews.Add(icon);
                BindIconButton(icon);
            }

            iconViews.Sort((a, b) =>
            {
                if (a == null || b == null)
                {
                    return 0;
                }

                return a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex());
            });
        }

        private void EnsureIconPool(int targetCount)
        {
            if (targetCount < 0)
            {
                targetCount = 0;
            }

            while (iconViews.Count < targetCount)
            {
                LevelIconView instance = Instantiate(levelIconPrefab, levelsParent);
                iconViews.Add(instance);
                BindIconButton(instance);
            }

            for (int i = 0; i < iconViews.Count; i++)
            {
                LevelIconView icon = iconViews[i];
                if (icon == null)
                {
                    continue;
                }

                bool shouldEnable = i < targetCount;
                if (icon.gameObject.activeSelf != shouldEnable)
                {
                    icon.gameObject.SetActive(shouldEnable);
                }
            }
        }

        private void BindIconButton(LevelIconView icon)
        {
            if (icon == null)
            {
                return;
            }

            Button button = icon.GetComponent<Button>() ?? icon.GetComponentInChildren<Button>();
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveListener(HidePopup);
            button.onClick.AddListener(() => HandleIconButtonClicked(icon));
        }

        private void HandleIconButtonClicked(LevelIconView icon)
        {
            if (icon == null || !icon.IsActive)
            {
                return;
            }

            if (commandInvoker != null && icon.LevelNumber > 0)
            {
                commandInvoker.LoadLevel(icon.LevelNumber);
            }

            HidePopup();
        }

        private void UpdateIcons(int levelCount)
        {
            if (playerDataService == null)
            {
                return;
            }

            int highestLevelPlayed = Mathf.Max(1, playerDataService.GetHighestLevel(1));

            for (int i = 0; i < levelCount && i < iconViews.Count; i++)
            {
                LevelIconView icon = iconViews[i];
                if (icon == null)
                {
                    continue;
                }

                int levelIndex = i + 1;
                icon.SetLevelNumber(levelIndex);
                icon.SetLevelActive(levelIndex <= highestLevelPlayed);
            }
        }

        private int DetermineLevelCount()
        {
            int configuredLevels = 0;
            if (levelConfig != null && levelConfig.levels != null)
            {
                configuredLevels = Mathf.Max(0, levelConfig.levels.Count);
            }

            int highestLevel = playerDataService != null ? playerDataService.GetHighestLevel(1) : 1;
            int targetCount = Mathf.Max(minimumLevelsToShow, configuredLevels, highestLevel);
            return Mathf.Max(1, targetCount);
        }
    }
}
