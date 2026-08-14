using System;
using System.Collections.Generic;
using Game.Level.Catalogs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Stages
{
    /// <summary>
    /// One world row in the stage select screen (themed background, title, stage entries).
    /// </summary>
    public class UiWorldStageRow : MonoBehaviour
    {
        [SerializeField] private Image _BackgroundImage;
        [SerializeField] private TMP_Text _WorldTitleText;
        [SerializeField] private UiStageItem _StageItemPrefab;
        [SerializeField] private Transform _StagesContentRoot;

        private StagesCollection _Collection;
        private int _CollectionIndex;
        private readonly List<UiStageItem> _Items = new List<UiStageItem>();

        /// <summary>
        /// Runtime world title used by progression-owned chrome outside the row.
        /// </summary>
        public string WorldTitle => _Collection != null ? _Collection.ThemeName : string.Empty;

        /// <summary>
        /// Builds child stage items and applies world chrome.
        /// </summary>
        public void Initialize(
            StagesCollection collection,
            int collectionIndex,
            StageListPresenter presenter,
            Action<int> onLocalStageSelected,
            Action<int> onLocalRewardClaimed)
        {
            _Collection = collection;
            _CollectionIndex = collectionIndex;
            _Items.Clear();

            if (_BackgroundImage != null)
            {
                _BackgroundImage.sprite = collection != null ? collection.StageSelectBackground : null;
            }

            if (_WorldTitleText != null)
            {
                _WorldTitleText.text = collection != null ? collection.ThemeName : string.Empty;
            }

            if (collection?.Stages == null || _StageItemPrefab == null || _StagesContentRoot == null || presenter == null)
            {
                return;
            }

            foreach (Transform child in _StagesContentRoot)
            {
                Destroy(child.gameObject);
            }

            for (int i = 0; i < collection.Stages.Count; i++)
            {
                StageListItemData itemData = presenter.GetItemData(collection, collectionIndex, i);
                if (itemData == null)
                {
                    _Items.Add(null);
                    continue;
                }

                UiStageItem item = Instantiate(_StageItemPrefab, _StagesContentRoot);
                int localIndex = i;
                item.Initialize(
                    itemData,
                    _ => onLocalStageSelected?.Invoke(localIndex),
                    _ => onLocalRewardClaimed?.Invoke(localIndex));
                _Items.Add(item);
            }
        }

        /// <summary>
        /// Rebinds one stage item after progression or reward state changes.
        /// </summary>
        public void RefreshLocalStage(
            int localStageIndex,
            StageListPresenter presenter,
            Action<int> onLocalStageSelected,
            Action<int> onLocalRewardClaimed)
        {
            if (_Collection == null || presenter == null || localStageIndex < 0 || localStageIndex >= _Items.Count)
            {
                return;
            }

            UiStageItem item = _Items[localStageIndex];
            if (item == null)
            {
                return;
            }

            StageListItemData itemData = presenter.GetItemData(_Collection, _CollectionIndex, localStageIndex);
            if (itemData == null)
            {
                return;
            }

            int captured = localStageIndex;
            item.Initialize(
                itemData,
                _ => onLocalStageSelected?.Invoke(captured),
                _ => onLocalRewardClaimed?.Invoke(captured));
        }

        /// <summary>
        /// Rebinds all stage items in this row (e.g. after changing focused world).
        /// </summary>
        public void RefreshAllStages(
            StageListPresenter presenter,
            Action<int> onLocalStageSelected,
            Action<int> onLocalRewardClaimed)
        {
            if (_Collection == null || presenter == null)
            {
                return;
            }

            for (int i = 0; i < _Items.Count; i++)
            {
                RefreshLocalStage(i, presenter, onLocalStageSelected, onLocalRewardClaimed);
            }
        }
    }
}
