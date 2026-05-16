using System.Collections.Generic;
using Common.Rewards.Interfaces;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.UI.Popups
{
    /// <summary>
    /// Starts a sample rewards presentation on play for scene testing (optional popup reference).
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
            RunIfConfigured();
        }

        private void RunIfConfigured()
        {
            if (_Popup == null)
            {
                return;
            }

            var list = new List<IRewardItem>();

            for (var i = 0; i < _DemoRewardCount; i++)
            {
                list.Add(new DisplayOnlyReward(_DemoRewardIcon, $"+{10 + i} DEMO"));
            }

            _Popup.SetRewards(list);
            _Popup.SetChestSprites(_ClosedChestSprite, _OpenedChestSprite);
            _Popup.Show();
        }
    }
}
