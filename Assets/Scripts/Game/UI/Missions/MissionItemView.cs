using System;
using Game.Feedback.Data;
using Game.Feedback.Events;
using Game.Missions.Data;
using Game.UI.Common;
using Game.UI.Popups;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Missions
{
    /// <summary>
    /// Single mission row inside the daily missions popup.
    /// </summary>
    public class MissionItemView : MonoBehaviour
    {
        [SerializeField] private Image _Icon;
        [SerializeField] private TextMeshProUGUI _DescriptionText;
        [SerializeField] private TextMeshProUGUI _ProgressText;
        [SerializeField] private Slider _ProgressBar;
        [SerializeField] private Button _ClaimButton;
        [SerializeField] private GameObject _ClaimedOverlay;
        [SerializeField] private Transform _RewardPreviewRoot;
        [SerializeField] private RewardItemView _RewardPreviewPrefab;

        private int _MissionIndex;
        private Action<int> _ClaimCallback;
        private ActionButtonPresenter _ClaimPresenter;

        private void Awake()
        {
            if (_ClaimButton != null)
            {
                _ClaimPresenter = ActionButtonPresenter.GetOrAdd(_ClaimButton);
                _ClaimButton.onClick.AddListener(OnClaimClicked);
            }
        }

        private void OnDestroy()
        {
            if (_ClaimButton != null)
            {
                _ClaimButton.onClick.RemoveListener(OnClaimClicked);
            }
        }

        public void Setup(
            int missionIndex,
            MissionSnapshot snapshot,
            Action<int> onClaimClicked,
            FeedbackEventChannel feedbackChannel = null)
        {
            _MissionIndex = missionIndex;
            _ClaimCallback = onClaimClicked;
            _ClaimPresenter = ActionButtonPresenter.GetOrAdd(_ClaimButton, feedbackChannel);

            BuildRewardPreview(snapshot);
            Refresh(snapshot);
        }

        public void Refresh(MissionSnapshot snapshot)
        {
            var definition = snapshot.Definition;
            var currentCount = Mathf.Clamp(snapshot.CurrentCount, 0, Mathf.Max(1, snapshot.RequiredCount));
            var requiredCount = Mathf.Max(1, snapshot.RequiredCount);

            if (_Icon != null)
            {
                _Icon.sprite = definition != null ? definition.Icon : null;
                _Icon.enabled = _Icon.sprite != null;
            }

            if (_DescriptionText != null)
            {
                _DescriptionText.text = definition != null ? definition.GetDescription() : string.Empty;
            }

            if (_ProgressText != null)
            {
                _ProgressText.text = $"{currentCount} / {requiredCount}";
            }

            if (_ProgressBar != null)
            {
                _ProgressBar.minValue = 0f;
                _ProgressBar.maxValue = requiredCount;
                _ProgressBar.value = currentCount;
            }

            var isClaimable = snapshot.IsCompleted && !snapshot.Claimed;
            if (_ClaimButton != null)
            {
                _ClaimButton.interactable = isClaimable;
            }

            if (_ClaimedOverlay != null)
            {
                _ClaimedOverlay.SetActive(snapshot.Claimed);
            }
        }

        public void SetClaimInteractable(bool interactable)
        {
            if (_ClaimButton != null)
            {
                _ClaimButton.interactable = interactable;
            }
        }

        public void ResolveClaim(bool success)
        {
            if (_ClaimPresenter == null)
                return;

            if (success)
            {
                _ClaimPresenter.CompleteSubmission();
                return;
            }

            _ClaimPresenter.PresentBlocked(new FeedbackEventData(
                FeedbackKind.Blocked,
                "mission.claim_blocked",
                _MissionIndex.ToString()));
        }

        private void OnClaimClicked()
        {
            if (_ClaimCallback == null)
                return;

            if (_ClaimPresenter != null && !_ClaimPresenter.TryBeginSubmission())
                return;

            _ClaimCallback?.Invoke(_MissionIndex);
        }

        private void BuildRewardPreview(MissionSnapshot snapshot)
        {
            if (_RewardPreviewRoot == null)
            {
                return;
            }

            for (var i = _RewardPreviewRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(_RewardPreviewRoot.GetChild(i).gameObject);
            }

            if (_RewardPreviewPrefab == null || snapshot.Definition?.Rewards == null)
            {
                return;
            }

            var rewards = snapshot.Definition.Rewards;
            for (var i = 0; i < rewards.Count; i++)
            {
                var reward = rewards[i];
                if (reward == null || !reward.TryCreateGrantItem(out var item) || item == null)
                {
                    continue;
                }

                var rewardView = Instantiate(_RewardPreviewPrefab, _RewardPreviewRoot);
                rewardView.Setup(item.Icon, item.DisplayText);
            }
        }
    }
}
