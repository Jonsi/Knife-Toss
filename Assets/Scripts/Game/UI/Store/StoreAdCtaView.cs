using System;
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

        private Action _OnClicked;

        public void Setup(string displayText, Action onClicked)
        {
            _OnClicked = onClicked;

            if (RewardText != null)
            {
                RewardText.text = displayText;
            }

            if (ActionButton != null)
            {
                ActionButton.onClick.RemoveListener(OnClicked);
                ActionButton.onClick.AddListener(OnClicked);
            }
        }

        public void SetInteractable(bool interactable)
        {
            if (ActionButton != null)
            {
                ActionButton.interactable = interactable;
            }
        }

        private void OnDisable()
        {
            if (ActionButton != null)
            {
                ActionButton.onClick.RemoveListener(OnClicked);
            }
        }

        private void OnClicked()
        {
            _OnClicked?.Invoke();
        }
    }
}
