using System;
using System.Collections.Generic;
using System.Threading;
using Common.Rewards.Interfaces;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Game.UI.Popups
{
    /// <summary>
    /// Populates horizontal reward rows and plays staggered DOTween scale-in reveals.
    /// </summary>
    public class StaggeredRewardRowsPresenter : MonoBehaviour
    {
        [SerializeField]
        private RewardItemView _RewardItemPrefab;

        [SerializeField]
        private RectTransform[] _RewardRowsOrdered = Array.Empty<RectTransform>();

        [SerializeField]
        [Min(1)]
        private int _ItemsPerRow = 4;

        [SerializeField]
        [Min(0f)]
        private float _DelayBetweenRevealStartsSeconds = 0.15f;

        [SerializeField]
        [Min(0f)]
        private float _RevealDurationSeconds = 0.35f;

        [SerializeField]
        private Ease _RevealEase = Ease.OutBack;

        [SerializeField]
        private bool _UseUnscaledTime = true;

        private readonly List<IRewardItem> _Rewards = new List<IRewardItem>();

        /// <summary>
        /// Replaces the reward list used by the next <see cref="PresentAsync"/> call.
        /// </summary>
        public void SetRewards(IReadOnlyList<IRewardItem> rewards)
        {
            _Rewards.Clear();

            if (rewards == null)
            {
                return;
            }

            for (var i = 0; i < rewards.Count; i++)
            {
                if (rewards[i] != null)
                {
                    _Rewards.Add(rewards[i]);
                }
            }
        }

        /// <summary>
        /// Removes spawned reward views, stops tweens, and hides all rows.
        /// </summary>
        public void Clear()
        {
            for (var r = 0; r < _RewardRowsOrdered.Length; r++)
            {
                var row = _RewardRowsOrdered[r];
                if (row == null)
                {
                    continue;
                }

                for (var c = row.childCount - 1; c >= 0; c--)
                {
                    var child = row.GetChild(c);
                    DOTween.Kill(child);
                    Destroy(child.gameObject);
                }

                row.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Spawns views per row and reveals them sequentially using DOTween.
        /// </summary>
        public async UniTask PresentAsync(CancellationToken cancellationToken)
        {
            var count = _Rewards.Count;

            if (count == 0 || _RewardItemPrefab == null || _RewardRowsOrdered.Length == 0)
            {
                return;
            }

            var itemsPerRow = Mathf.Max(1, _ItemsPerRow);
            var rowsNeeded = (count + itemsPerRow - 1) / itemsPerRow;

            for (var r = 0; r < _RewardRowsOrdered.Length; r++)
            {
                if (_RewardRowsOrdered[r] == null)
                {
                    continue;
                }

                _RewardRowsOrdered[r].gameObject.SetActive(r < rowsNeeded);
            }

            var views = new List<RewardItemView>(count);

            for (var i = 0; i < count; i++)
            {
                var rowIndex = i / itemsPerRow;

                if (rowIndex < 0 || rowIndex >= _RewardRowsOrdered.Length || _RewardRowsOrdered[rowIndex] == null)
                {
                    continue;
                }

                var parent = _RewardRowsOrdered[rowIndex];
                var reward = _Rewards[i];
                var view = Instantiate(_RewardItemPrefab, parent);
                view.Setup(reward.Icon, reward.DisplayText);
                var rt = (RectTransform)view.transform;
                rt.localScale = Vector3.zero;
                views.Add(view);
            }

            for (var i = 0; i < views.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (i > 0 && _DelayBetweenRevealStartsSeconds > 0f)
                {
                    await UniTask.Delay(
                        TimeSpan.FromSeconds(_DelayBetweenRevealStartsSeconds),
                        ignoreTimeScale: _UseUnscaledTime,
                        cancellationToken: cancellationToken);
                }

                var rt = (RectTransform)views[i].transform;
                AnimateReveal(rt);
            }

            if (_RevealDurationSeconds > 0f && views.Count > 0)
            {
                await UniTask.Delay(
                    TimeSpan.FromSeconds(_RevealDurationSeconds),
                    ignoreTimeScale: _UseUnscaledTime,
                    cancellationToken: cancellationToken);
            }
        }

        private void AnimateReveal(RectTransform rectTransform)
        {
            DOTween.Kill(rectTransform);
            rectTransform
                .DOScale(Vector3.one, _RevealDurationSeconds)
                .SetEase(_RevealEase)
                .SetUpdate(_UseUnscaledTime)
                .SetTarget(rectTransform);
        }
    }
}
