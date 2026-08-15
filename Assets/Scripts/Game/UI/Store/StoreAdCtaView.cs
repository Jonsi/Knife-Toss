using System;
using Game.Feedback.Data;
using Game.Feedback.Events;
using Game.Store.Data;
using Game.UI.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Store
{
    /// <summary>
    /// Bottom watch-ad CTA.
    /// </summary>
    public class StoreAdCtaView : MonoBehaviour
    {
        [field: SerializeField] public TMP_Text RewardText { get; private set; }
        [field: SerializeField] public Button ActionButton { get; private set; }
        [SerializeField] private TMP_Text _ActionLabelText;
        [SerializeField] private StoreAdPriceConfig _PriceConfig;

        private Action _OnClicked;
        private ActionButtonPresenter _ButtonPresenter;

        public event Action<bool> SubmissionStateChanged;
        public bool IsSubmissionInProgress { get; private set; }

        public void Setup(
            string displayText,
            Action onClicked,
            FeedbackEventChannel feedbackChannel = null)
        {
            _OnClicked = onClicked;

            if (RewardText != null)
            {
                RewardText.text = _PriceConfig != null
                    ? $"+{_PriceConfig.CoinAmount} COINS"
                    : displayText;
            }

            if (_ActionLabelText != null)
            {
                _ActionLabelText.text = "WATCH AD";
            }

            if (ActionButton != null)
            {
                _ButtonPresenter = ActionButtonPresenter.GetOrAdd(ActionButton, feedbackChannel);
                ActionButton.onClick.RemoveListener(OnClicked);
                ActionButton.onClick.AddListener(OnClicked);
            }
        }

        public void SetInteractable(bool interactable)
        {
            if (ActionButton != null)
            {
                if (_ButtonPresenter != null)
                {
                    _ButtonPresenter.SetAvailable(interactable);
                }
                else
                {
                    ActionButton.interactable = interactable;
                }
            }
        }

        public void ResolveSubmission(bool success)
        {
            if (_ButtonPresenter == null)
                return;

            if (success)
            {
                _ButtonPresenter.CompleteSubmission();
                SetSubmissionInProgress(false);
                return;
            }

            _ButtonPresenter.PresentBlocked(new FeedbackEventData(
                FeedbackKind.Blocked,
                "ad.reward_blocked",
                gameObject.name));
            SetSubmissionInProgress(false);
        }

        private void OnDisable()
        {
            if (ActionButton != null)
            {
                ActionButton.onClick.RemoveListener(OnClicked);
            }

            SetSubmissionInProgress(false);
        }

        private void OnClicked()
        {
            if (_OnClicked == null)
                return;

            if (_ButtonPresenter != null && !_ButtonPresenter.TryBeginSubmission())
                return;

            SetSubmissionInProgress(true);
            _OnClicked?.Invoke();
        }

        private void SetSubmissionInProgress(bool inProgress)
        {
            if (IsSubmissionInProgress == inProgress)
            {
                return;
            }

            IsSubmissionInProgress = inProgress;
            SubmissionStateChanged?.Invoke(inProgress);
        }
    }
}
