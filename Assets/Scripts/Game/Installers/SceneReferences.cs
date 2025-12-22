using Game.Entities.Knife.Views;
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
    /// Unified scene references for dependency injection.
    /// </summary>
    public class SceneReferences : MonoBehaviour
    {
        [Header("Level")]
        [SerializeField] private Transform _LevelRoot;
        [SerializeField] private KnifeThrowerView _KnifeThrowerView;

        [Header("UI - Controllers")]
        [SerializeField] private UIController _UIController;

        [Header("UI - HUD Views")]
        [SerializeField] private KnifeMagazineView _KnifeMagazineView;
        [SerializeField] private StageProgressView _StageProgressView;
        [SerializeField] private CollectibleCountView _CollectibleCountView;
        [SerializeField] private MuteButtonView _MuteButtonView;
        [SerializeField] private PowerUpHudView _PowerUpHudView;

        public Transform LevelRoot => _LevelRoot;
        public KnifeThrowerView KnifeThrowerView => _KnifeThrowerView;
        public UIController UIController => _UIController;
        public KnifeMagazineView KnifeMagazineView => _KnifeMagazineView;
        public StageProgressView StageProgressView => _StageProgressView;
        public CollectibleCountView CollectibleCountView => _CollectibleCountView;
        public MuteButtonView MuteButtonView => _MuteButtonView;
        public PowerUpHudView PowerUpHudView => _PowerUpHudView;
    }
}
