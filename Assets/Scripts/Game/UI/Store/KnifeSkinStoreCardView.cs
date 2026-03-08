using Game.Store.Data;
using Game.Store.Rewards;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Store
{
    /// <summary>
    /// Card view used for knife skin store items.
    /// </summary>
    public class KnifeSkinStoreCardView : StoreCardView
    {
        [Header("Knife Skin State UI")]
        [SerializeField] private GameObject _PricePillRoot;
        [SerializeField] private Button _EquipButton;
        [SerializeField] private TMP_Text _EquipButtonText;
        [SerializeField] private string _BuyButtonLabel = "Buy";
        [SerializeField] private string _EquipButtonLabel = "Equip";
        [SerializeField] private string _EquippedButtonLabel = "Equipped";
        [SerializeField] private Color _BuyColor = Color.white;
        [SerializeField] private Color _BuyTextColor = Color.white;
        [SerializeField] private Color _EquipColor = Color.white;
        [SerializeField] private Color _EquippedColor = Color.gray;
        [SerializeField] private Color _EquipTextColor = Color.white;
        [SerializeField] private Color _EquippedTextColor = Color.white;

        private void OnEnable()
        {
            if (_EquipButton != null)
            {
                _EquipButton.onClick.RemoveListener(OnEquipPressed);
                _EquipButton.onClick.AddListener(OnEquipPressed);
            }
        }

        protected override void OnDisable()
        {
            if (_EquipButton != null)
            {
                _EquipButton.onClick.RemoveListener(OnEquipPressed);
            }

            base.OnDisable();
        }

        public void SetState(bool isOwned, bool isEquipped, bool canInteract)
        {
            SetPricePillVisible(!isOwned);
            SetButtonVisible(isOwned);

            if (!isOwned)
            {
                SetInteractable(canInteract);
                SetButtonLabel(_BuyButtonLabel);
                return;
            }

            if (isEquipped)
            {
                ApplyEquippedState();
                return;
            }

            ApplyEquipState(canInteract);
        }

        private void ApplyEquipState(bool canInteract)
        {
            SetButtonLabel(_EquipButtonLabel);
            SetButtonStyle(_EquipColor, _EquipTextColor);
            SetActionInteractable(canInteract);
        }

        private void ApplyEquippedState()
        {
            SetButtonLabel(_EquippedButtonLabel);
            SetButtonStyle(_EquippedColor, _EquippedTextColor);
            SetActionInteractable(false);
        }

        private void SetPricePillVisible(bool visible)
        {
            if (_PricePillRoot != null)
            {
                _PricePillRoot.SetActive(visible);
            }

            PricePill?.SetVisible(visible);
        }

        private void SetButtonVisible(bool visible)
        {
            if (_EquipButton != null)
            {
                _EquipButton.gameObject.SetActive(visible);
            }
        }

        private void SetButtonLabel(string label)
        {
            if (_EquipButtonText != null)
            {
                _EquipButtonText.text = label;
            }
        }

        private void SetButtonStyle(Color buttonColor, Color textColor)
        {
            if (_EquipButton != null)
            {
                SetButtonColors(_EquipButton, buttonColor);
            }

            if (_EquipButtonText != null)
            {
                _EquipButtonText.color = textColor;
            }
        }

        private void SetActionInteractable(bool canInteract)
        {
            SetInteractable(canInteract);

            if (_EquipButton != null)
            {
                _EquipButton.interactable = canInteract;
            }
        }

        private void OnEquipPressed()
        {
            if (BuyButton == null || ReferenceEquals(BuyButton, _EquipButton))
            {
                return;
            }

            BuyButton.onClick.Invoke();
        }

        private static void SetButtonColors(Button button, Color baseColor)
        {
            var colors = button.colors;
            colors.normalColor = baseColor;
            colors.highlightedColor = baseColor;
            colors.pressedColor = baseColor;
            colors.selectedColor = baseColor;
            colors.disabledColor = baseColor;
            colors.colorMultiplier = 1f;
            button.colors = colors;
        }

        protected override void BindEntry(PurchasableEntry entry)
        {
            if (entry?.Reward is KnifeSkinReward knifeSkinReward && knifeSkinReward.Config != null)
            {
                SetDisplaySprite(knifeSkinReward.Config.Icon);
            }
        }
    }
}
