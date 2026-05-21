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
                Debug.LogWarning("[RewardsFlow] Presenter.SetRewards: rewards list is null.");
                return;
            }

            for (var i = 0; i < rewards.Count; i++)
            {
                if (rewards[i] != null)
                {
                    _Rewards.Add(rewards[i]);
                }
            }

            Debug.Log($"[RewardsFlow] Presenter.SetRewards: stored {_Rewards.Count} items (from incoming {rewards.Count}).");
        }

        /// <summary>
        /// Removes spawned reward views, stops tweens, and hides all rows.
        /// </summary>
        public void Clear()
        {
            Debug.Log($"[RewardsFlow] Presenter.Clear: row slots={_RewardRowsOrdered.Length}, clearing children.");
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
                Debug.LogWarning($"[RewardsFlow] Presenter.PresentAsync: early exit. rewardCount={count}, prefab={_RewardItemPrefab != null}, rowArrayLength={_RewardRowsOrdered.Length}.");
                return;
            }

            var itemsPerRow = Mathf.Max(1, _ItemsPerRow);
            var rowsNeeded = (count + itemsPerRow - 1) / itemsPerRow;
            Debug.Log($"[RewardsFlow] Presenter.PresentAsync: itemsPerRow={itemsPerRow}, rowsNeeded={rowsNeeded}, rewardCount={count}.");

            for (var r = 0; r < _RewardRowsOrdered.Length; r++)
            {
                if (_RewardRowsOrdered[r] == null)
                {
                    Debug.LogWarning($"[RewardsFlow] Presenter: row index {r} is null in _RewardRowsOrdered.");
                    continue;
                }

                var active = r < rowsNeeded;
                _RewardRowsOrdered[r].gameObject.SetActive(active);
                Debug.Log($"[RewardsFlow] Presenter: row {r} '{_RewardRowsOrdered[r].name}' setActive={active}.");
            }

            var views = new List<RewardItemView>(count);

            for (var i = 0; i < count; i++)
            {
                var rowIndex = i / itemsPerRow;

                if (rowIndex < 0 || rowIndex >= _RewardRowsOrdered.Length || _RewardRowsOrdered[rowIndex] == null)
                {
                    Debug.LogWarning($"[RewardsFlow] Presenter: skip spawn index {i}: invalid rowIndex={rowIndex}.");
                    continue;
                }

                var parent = _RewardRowsOrdered[rowIndex];
                var reward = _Rewards[i];
                var view = Instantiate(_RewardItemPrefab, parent);
                view.Setup(reward.Icon, reward.DisplayText);
                var rt = (RectTransform)view.transform;
                rt.localScale = Vector3.zero;
                views.Add(view);
                Debug.Log($"[RewardsFlow] Presenter: spawned reward {i} under row {rowIndex} ('{parent.name}').");
            }

            Debug.Log($"[RewardsFlow] Presenter: starting staggered reveal for {views.Count} views.");

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

            Debug.Log("[RewardsFlow] Presenter.PresentAsync: finished.");
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
