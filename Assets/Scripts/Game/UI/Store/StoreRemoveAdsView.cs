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
    /// Theme-neutral Store presentation for the existing Remove Ads IAP entry.
    /// </summary>
    public sealed class StoreRemoveAdsView : MonoBehaviour
    {
        [SerializeField] private Button _ActionButton;
        [SerializeField] private TMP_Text _ActionLabelText;
        [SerializeField] private GameObject _OwnedStateRoot;
        [SerializeField] private TMP_Text _OwnedStateText;

        private PurchasableEntry _Entry;
        private Action<PurchasableEntry> _OnClicked;
        private ActionButtonPresenter _ButtonPresenter;

        public Button ActionButton => _ActionButton;

        public void Setup(
            PurchasableEntry entry,
            Action<PurchasableEntry> onClicked,
            FeedbackEventChannel feedbackChannel = null)
        {
            _Entry = entry;
            _OnClicked = onClicked;

            if (_ActionLabelText != null)
            {
                _ActionLabelText.text = "REMOVE ADS";
            }

            if (_OwnedStateText != null)
            {
                _OwnedStateText.text = "OWNED";
            }

            if (_ActionButton != null)
            {
                _ButtonPresenter = ActionButtonPresenter.GetOrAdd(_ActionButton, feedbackChannel);
                _ActionButton.onClick.RemoveListener(OnPressed);
                _ActionButton.onClick.AddListener(OnPressed);
            }
        }

        public void SetState(bool available, bool owned, bool canInteract)
        {
            gameObject.SetActive(available);
            if (!available)
            {
                return;
            }

            if (_OwnedStateRoot != null)
            {
                _OwnedStateRoot.SetActive(owned);
            }

            if (_ActionLabelText != null)
            {
                _ActionLabelText.gameObject.SetActive(!owned);
            }

            if (_ButtonPresenter != null)
            {
                _ButtonPresenter.SetAvailable(canInteract && !owned);
            }
            else if (_ActionButton != null)
            {
                _ActionButton.interactable = canInteract && !owned;
            }
        }

        public void ResolveSubmission(PurchaseResult result)
        {
            if (_ButtonPresenter == null)
            {
                return;
            }

            if (result.Success)
            {
                _ButtonPresenter.CompleteSubmission();
                return;
            }

            _ButtonPresenter.PresentBlocked(new FeedbackEventData(
                FeedbackKind.Blocked,
                "store.remove_ads_blocked",
                gameObject.name));
        }

        private void OnDisable()
        {
            if (_ActionButton != null)
            {
                _ActionButton.onClick.RemoveListener(OnPressed);
            }
        }

        private void OnPressed()
        {
            if (_Entry == null || _OnClicked == null)
            {
                return;
            }

            if (_ButtonPresenter != null && !_ButtonPresenter.TryBeginSubmission())
            {
                return;
            }

            _OnClicked.Invoke(_Entry);
        }
    }
}
