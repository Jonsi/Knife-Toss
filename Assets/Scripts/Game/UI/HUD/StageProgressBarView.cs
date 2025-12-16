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
                _LevelStartedChannel.Subscribe(OnLevelStarted);
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
            bool levelCountChanged = data.TotalLevelsInStage != _CachedLevelCount;

            _CachedLevelCount = data.TotalLevelsInStage;
            _CurrentLevelIndex = data.LevelIndex;
            _CurrentStageIndex = data.StageIndex;
            UpdateStageLabels(data.StageIndex);

            if (levelCountChanged || _Slots.Count != _CachedLevelCount)
                Rebuild();

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

            if (_Slots.Count == _CachedLevelCount)
                return;

            Rebuild();
        }

        private void Rebuild()
        {
            for (int i = 0; i < _Slots.Count; i++)
            {
                if (_Slots[i] != null)
                    Destroy(_Slots[i].gameObject);
            }
            _Slots.Clear();

            for (int i = 0; i < _CachedLevelCount; i++)
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

            int completedIndex = _CurrentLevelIndex;
            for (int i = 0; i < _Slots.Count; i++)
            {
                var slot = _Slots[i];
                if (slot == null)
                    continue;

                slot.color = i < completedIndex ? _CompletedColor : _IncompleteColor;
            }
        }

        private void UpdateStageLabels(int stageIndex)
        {
            if (_CurrentStageLabel != null)
                _CurrentStageLabel.text = (stageIndex + 1).ToString();

            if (_NextStageLabel != null)
                _NextStageLabel.text = (stageIndex + 2).ToString();
        }
    }
}
