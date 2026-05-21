using System.Collections.Generic;
using Common.Rewards.Interfaces;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Popups
{
    /// <summary>
    /// Test helper: builds sample rewards and opens the popup when the open button is clicked.
    /// </summary>
    public class RewardsPresentationPopupTestFlow : MonoBehaviour
    {
        private sealed class DisplayOnlyReward : IRewardItem
        {
            private readonly Sprite _Icon;
            private readonly string _DisplayText;

            public DisplayOnlyReward(Sprite icon, string displayText)
            {
                _Icon = icon;
                _DisplayText = displayText ?? string.Empty;
            }

            /// <inheritdoc/>
            public string DisplayText => _DisplayText;

            /// <inheritdoc/>
            public Sprite Icon => _Icon;

            /// <inheritdoc/>
            public UniTask GrantAsync(IRewardGrantContext context)
            {
                return UniTask.CompletedTask;
            }
        }

        [SerializeField]
        private RewardsPresentationPopup _Popup;

        [SerializeField]
        private Button _OpenPopupButton;

        [SerializeField]
        private Sprite _DemoRewardIcon;

        [SerializeField]
        private Sprite _ClosedChestSprite;

        [SerializeField]
        private Sprite _OpenedChestSprite;

        [SerializeField]
        [Min(0)]
        private int _DemoRewardCount = 6;

        private void Start()
        {
            PrepareChestSpritesOnPopup();
        }

        private void OnEnable()
        {
            if (_OpenPopupButton != null)
            {
                _OpenPopupButton.onClick.AddListener(OpenPopupClicked);
            }
        }

        private void OnDisable()
        {
            if (_OpenPopupButton != null)
            {
                _OpenPopupButton.onClick.RemoveListener(OpenPopupClicked);
            }
        }

        private void OpenPopupClicked()
        {
            if (_Popup == null)
            {
                return;
            }

            PrepareChestSpritesOnPopup();

            var list = new List<IRewardItem>();

            for (var i = 0; i < _DemoRewardCount; i++)
            {
                list.Add(new DisplayOnlyReward(_DemoRewardIcon, $"+{10 + i} DEMO"));
            }

            _Popup.SetRewards(list);
            _Popup.Show();
        }

        private void PrepareChestSpritesOnPopup()
        {
            if (_Popup == null)
            {
                return;
            }

            _Popup.SetChestSprites(_ClosedChestSprite, _OpenedChestSprite);
        }
    }
}
