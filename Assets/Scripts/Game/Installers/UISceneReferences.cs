using Game.Presentation.HUD.Collectibles;
using Game.Presentation.HUD.KnifeMagazine;
using Game.Presentation.HUD.MuteButton;
using Game.Presentation.HUD.PowerUps;
using Game.Presentation.HUD.StageProgress;
using Game.UI.Controllers;
using UnityEngine;

namespace Game.Installers
{
    /// <summary>
    /// Holds UI scene references for installer registration.
    /// </summary>
    public class UISceneReferences : MonoBehaviour
    {
        [Header("Controllers")]
        [SerializeField] private UIController _Controller;

        [Header("HUD Views")]
        [SerializeField] private KnifeMagazineView _KnifeMagazineView;
        [SerializeField] private StageProgressView _StageProgressView;
        [SerializeField] private CollectibleCountView _CollectibleCountView;
        [SerializeField] private MuteButtonView _MuteButtonView;
        [SerializeField] private PowerUpHudView _PowerUpHudView;

        public UIController Controller => _Controller;
        public KnifeMagazineView KnifeMagazineView => _KnifeMagazineView;
        public StageProgressView StageProgressView => _StageProgressView;
        public CollectibleCountView CollectibleCountView => _CollectibleCountView;
        public MuteButtonView MuteButtonView => _MuteButtonView;
        public PowerUpHudView PowerUpHudView => _PowerUpHudView;
    }
}
