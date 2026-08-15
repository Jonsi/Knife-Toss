using System;
using Game.Economy.Data;
using Game.Feedback.Data;
using Game.Feedback.Events;
using Game.Store.Data;
using Game.Store.Rewards;
using Game.UI.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Store
{
    /// <summary>
    /// Base store card view. Concrete card types provide the prefab-specific implementation.
    /// </summary>
    public abstract class StoreCardView : MonoBehaviour
    {
        [field: SerializeField] public StorePricePillView PricePill { get; private set; }
        [field: SerializeField] public Image IconImage { get; private set; }
        [field: SerializeField] public Image ShadowImage { get; private set; }
        [field: SerializeField] public Button BuyButton { get; private set; }

        protected PurchasableEntry Entry { get; private set; }
        private Action<PurchasableEntry> _OnBuyClicked;
        private ActionButtonPresenter _ButtonPresenter;

        public PurchasableEntry BoundEntry => Entry;

        public void Setup(
            PurchasableEntry entry,
            Action<PurchasableEntry> onBuyClicked,
            FeedbackEventChannel feedbackChannel = null)
        {
            ResetForReuse();
            Entry = entry;
            _OnBuyClicked = onBuyClicked;

            if (PricePill != null)
            {
                if (entry?.Price != null && entry.Price.IsIapPrice)
                {
                    PricePill.SetCustomPrice(string.Empty);
                }
                else
                {
                    PricePill.SetPrice(entry?.Price?.Amount ?? 0, entry?.Price?.Currency ?? CurrencyType.None);
                }
            }

            BindEntry(entry);

            if (BuyButton != null)
            {
                _ButtonPresenter = ActionButtonPresenter.GetOrAdd(BuyButton, feedbackChannel);
                BuyButton.onClick.RemoveListener(OnBuyPressed);
                BuyButton.onClick.AddListener(OnBuyPressed);
            }
        }

        public virtual void ResetForReuse()
        {
            if (BuyButton != null)
            {
                BuyButton.onClick.RemoveListener(OnBuyPressed);
                BuyButton.interactable = false;
            }

            _ButtonPresenter?.CancelSubmission();
            _OnBuyClicked = null;
            Entry = null;
        }

        public void SetInteractable(bool interactable)
        {
            if (BuyButton != null)
            {
                if (_ButtonPresenter != null)
                {
                    _ButtonPresenter.SetAvailable(interactable);
                }
                else
                {
                    BuyButton.interactable = interactable;
                }
            }
        }

        public void ResolveSubmission(PurchaseResult result)
        {
            if (_ButtonPresenter == null)
                return;

            if (result.Success)
            {
                _ButtonPresenter.CompleteSubmission();
                return;
            }

            bool insufficientFunds = string.Equals(
                result.FailureReason,
                PurchaseResult.InsufficientFunds.FailureReason,
                StringComparison.Ordinal);
            _ButtonPresenter.PresentBlocked(new FeedbackEventData(
                insufficientFunds ? FeedbackKind.InsufficientFunds : FeedbackKind.Blocked,
                insufficientFunds ? "store.insufficient_funds" : "store.purchase_blocked",
                GetFeedbackContextId()));
        }

        protected virtual void BindEntry(PurchasableEntry entry)
        {
        }

        protected void SetDisplaySprite(Sprite sprite)
        {
            if (IconImage != null)
            {
                IconImage.sprite = sprite;
            }

            if (ShadowImage != null)
            {
                ShadowImage.sprite = sprite;
            }
        }

        protected virtual void OnDisable()
        {
            if (BuyButton != null)
            {
                BuyButton.onClick.RemoveListener(OnBuyPressed);
            }
        }

        private void OnBuyPressed()
        {
            if (Entry == null || _OnBuyClicked == null)
                return;

            if (_ButtonPresenter != null && !_ButtonPresenter.TryBeginSubmission())
                return;

            _OnBuyClicked.Invoke(Entry);
        }

        private string GetFeedbackContextId()
        {
            return Entry?.Reward switch
            {
                PowerUpReward powerUp when powerUp.Config != null => powerUp.Config.InventoryKey,
                KnifeSkinReward skin when skin.Config != null => skin.Config.Id,
                _ => gameObject.name
            };
        }

    }
}
