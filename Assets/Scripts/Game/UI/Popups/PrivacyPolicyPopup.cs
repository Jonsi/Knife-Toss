using System;
using Game.UI.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Popups
{
    public class PrivacyPolicyPopup : BasePopup
    {
        [SerializeField] private ScrollRect _ContentScrollRect;
        [SerializeField] private TextMeshProUGUI _PolicyText;
        [SerializeField] private Button _CloseButton;
        
        public event Action OnCloseRequested;

        private void Awake()
        {
            ActionButtonPresenter.GetOrAdd(_CloseButton);
            if (_CloseButton != null)
                _CloseButton.onClick.AddListener(() => OnCloseRequested?.Invoke());
        }

        public void SetContent(string policyText)
        {
            if (_PolicyText != null)
                _PolicyText.text = policyText;
            
            if (_ContentScrollRect != null)
                _ContentScrollRect.verticalNormalizedPosition = 1f;
        }
    }
}
