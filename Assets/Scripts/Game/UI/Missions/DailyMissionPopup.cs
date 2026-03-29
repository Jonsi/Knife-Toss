using Game.UI.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Missions
{
    /// <summary>
    /// View-only popup for browsing and claiming daily missions from the main menu.
    /// </summary>
    public class DailyMissionPopup : BasePopup
    {
        [SerializeField] private TextMeshProUGUI _TitleText;
        [SerializeField] private TextMeshProUGUI _RefreshTimerText;
        [SerializeField] private Transform _MissionItemsRoot;
        [SerializeField] private MissionItemView _MissionItemPrefab;
        [SerializeField] private Button _CloseButton;

        public TextMeshProUGUI TitleText => _TitleText;
        public TextMeshProUGUI RefreshTimerText => _RefreshTimerText;
        public Transform MissionItemsRoot => _MissionItemsRoot;
        public MissionItemView MissionItemPrefab => _MissionItemPrefab;
        public Button CloseButton => _CloseButton;

        protected override bool ShouldPauseGameplayWhileVisible => false;
    }
}
