using Game.Entities.Knife.Views;
using UnityEngine;

namespace Game.Installers
{
    /// <summary>
    /// Holds scene references needed by the level systems.
    /// </summary>
    public class LevelSceneReferences : MonoBehaviour
    {
        [SerializeField] private Transform _LevelRoot;
        [SerializeField] private KnifeThrowerView _KnifeThrowerView;

        public Transform LevelRoot => _LevelRoot;
        public KnifeThrowerView KnifeThrowerView => _KnifeThrowerView;
    }
}
