using System.Collections.Generic;
using Game.Presentation.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation.HUD.StageProgress
{
    /// <summary>
    /// Displays stage progress slots and labels.
    /// </summary>
    public class StageProgressView : BaseView<StageProgressModel>
    {
        [SerializeField] private Transform _Container;
        [SerializeField] private GameObject _SlotPrefab;
        [SerializeField] private Color _CompletedColor = Color.white;
        [SerializeField] private Color _IncompleteColor = Color.gray;
        [SerializeField] private TMP_Text _CurrentStageLabel;
        [SerializeField] private TMP_Text _NextStageLabel;

        private readonly List<Image> _Slots = new List<Image>();
        private int _CachedLevelCount = -1;

        public void SetStageProgress(int totalLevels, int currentLevelIndex, int stageIndex)
        {
            if (_Container == null || _SlotPrefab == null)
            {
                return;
            }

            totalLevels = Mathf.Max(0, totalLevels);
            currentLevelIndex = Mathf.Clamp(currentLevelIndex, 0, totalLevels);
            stageIndex = Mathf.Max(0, stageIndex);

            if (_CachedLevelCount != totalLevels || _Slots.Count != totalLevels)
            {
                Rebuild(totalLevels);
            }

            UpdateStageLabels(stageIndex);

            for (int i = 0; i < _Slots.Count; i++)
            {
                var slot = _Slots[i];
                if (slot == null)
                {
                    continue;
                }

                slot.color = i < currentLevelIndex ? _CompletedColor : _IncompleteColor;
            }
        }

        private void Rebuild(int levelCount)
        {
            for (int i = 0; i < _Slots.Count; i++)
            {
                if (_Slots[i] != null)
                {
                    Destroy(_Slots[i].gameObject);
                }
            }
            _Slots.Clear();

            _CachedLevelCount = levelCount;

            for (int i = 0; i < levelCount; i++)
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

        private void UpdateStageLabels(int stageIndex)
        {
            if (_CurrentStageLabel != null)
            {
                _CurrentStageLabel.text = (stageIndex + 1).ToString();
            }

            if (_NextStageLabel != null)
            {
                _NextStageLabel.text = (stageIndex + 2).ToString();
            }
        }
    }
}
