using System.Collections.Generic;
using Game.Flow.Events;
using Game.Flow.Stage;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.HUD
{
    /// <summary>
    /// Displays level progress within the current stage.
    /// </summary>
    public class StageProgressBarView : MonoBehaviour
    {
        [SerializeField] private Transform _Container;
        [SerializeField] private GameObject _SlotPrefab;
        [SerializeField] private Color _CompletedColor = Color.white;
        [SerializeField] private Color _CurrentLevelColor = Color.yellow;
        [SerializeField] private Color _IncompleteColor = Color.gray;
        [SerializeField] private LevelStartedEventChannel _LevelStartedChannel;
        [SerializeField] private StageCompletedEventChannel _StageCompletedChannel;
        [SerializeField] private TMP_Text _CurrentStageLabel;
        [SerializeField] private TMP_Text _NextStageLabel;

        private readonly List<Image> _Slots = new List<Image>();
        private int _CachedLevelCount = -1;
        private int _CurrentLevelIndex = 0;
        private int _CurrentStageIndex = -1;

        private void OnEnable()
        {
            if (_LevelStartedChannel != null)
            {
                _LevelStartedChannel.Subscribe(OnLevelStarted);
                if (_LevelStartedChannel.HasLastEvent)
                    OnLevelStarted(_LevelStartedChannel.LastEvent);
            }
            if (_StageCompletedChannel != null)
                _StageCompletedChannel.Subscribe(OnStageCompleted);

            RebuildIfNeeded();
            Refresh();
        }

        private void OnDisable()
        {
            if (_LevelStartedChannel != null)
                _LevelStartedChannel.Unsubscribe(OnLevelStarted);
            if (_StageCompletedChannel != null)
                _StageCompletedChannel.Unsubscribe(OnStageCompleted);
        }

        private void OnLevelStarted(LevelStartedEventData data)
        {
            if (data == null)
                return;

            bool levelCountChanged = data.TotalLevelsInStage != _CachedLevelCount;

            _CachedLevelCount = data.TotalLevelsInStage;
            _CurrentLevelIndex = data.LevelIndex;
            _CurrentStageIndex = data.StageIndex;
            UpdateStageLabels(data.StageIndex);

            if (levelCountChanged || _Slots.Count < _CachedLevelCount)
                EnsureSlotCount();

            Refresh();
        }

        private void OnStageCompleted(StageData data)
        {
            if (data == null)
                return;

            if (_CurrentStageIndex >= 0 && data.StageIndex != _CurrentStageIndex)
                return;

            _CurrentLevelIndex = _CachedLevelCount;
            Refresh();
        }

        private void RebuildIfNeeded()
        {
            if (_Container == null || _SlotPrefab == null)
                return;

            if (_CachedLevelCount < 0)
                return;

            if (_Slots.Count >= _CachedLevelCount)
                return;

            EnsureSlotCount();
        }

        private void EnsureSlotCount()
        {
            while (_Slots.Count < _CachedLevelCount)
            {
                var slotObject = Instantiate(_SlotPrefab, _Container);
                slotObject.transform.localScale = Vector3.one;
                var image = slotObject.GetComponent<Image>();
                if (image != null)
                {
                    _Slots.Add(image);
                }
            }
        }

        private void Refresh()
        {
            if (_CachedLevelCount < 0)
                return;

            UpdateMarkerLayout();

            for (int i = 0; i < _Slots.Count; i++)
            {
                var slot = _Slots[i];
                if (slot == null)
                    continue;

                bool isUsedByCurrentStage = i < _CachedLevelCount;
                slot.gameObject.SetActive(isUsedByCurrentStage);
                if (!isUsedByCurrentStage)
                    continue;

                if (i < _CurrentLevelIndex)
                {
                    slot.color = _CompletedColor;
                }
                else if (i == _CurrentLevelIndex && _CurrentLevelIndex < _CachedLevelCount)
                {
                    slot.color = _CurrentLevelColor;
                }
                else
                {
                    slot.color = _IncompleteColor;
                }
            }

        }

        private void UpdateMarkerLayout()
        {
            if (!(_Container is RectTransform containerRect) || _CachedLevelCount <= 0)
                return;

            HorizontalLayoutGroup layout = _Container.GetComponent<HorizontalLayoutGroup>();
            if (layout == null)
                return;

            float availableWidth = Mathf.Max(0f, containerRect.rect.width - layout.padding.horizontal);
            const float minimumSpacing = 4f;
            float markerSize = Mathf.Clamp(
                (availableWidth - minimumSpacing * (_CachedLevelCount - 1)) / _CachedLevelCount,
                14f,
                28f);
            layout.spacing = _CachedLevelCount > 1
                ? Mathf.Max(minimumSpacing,
                    (availableWidth - markerSize * _CachedLevelCount) / (_CachedLevelCount - 1))
                : 0f;

            for (int i = 0; i < _Slots.Count; i++)
            {
                Image slot = _Slots[i];
                if (slot == null)
                    continue;

                var slotRect = slot.rectTransform;
                slotRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, markerSize);
                slotRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, markerSize);
                LayoutElement element = slot.GetComponent<LayoutElement>();
                if (element != null)
                {
                    element.preferredWidth = markerSize;
                    element.preferredHeight = markerSize;
                }
            }
        }

        private void UpdateStageLabels(int stageIndex)
        {
            if (_CurrentStageLabel != null)
                _CurrentStageLabel.text = $"STAGE {stageIndex + 1}";

            if (_NextStageLabel != null)
                _NextStageLabel.text = (stageIndex + 2).ToString();
        }
    }
}
