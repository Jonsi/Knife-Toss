using KnifeToss.Application.Services;
using KnifeToss.Installers;
using UnityEngine;

namespace KnifeToss.Presentation.Gameplay
{
    /// <summary>
    /// Bridges KnifeThrower events to the session service without direct references.
    /// </summary>
    [RequireComponent(typeof(KnifeThrower))]
    public class KnifeThrowRelay : MonoBehaviour
    {
        [SerializeField] private GameplayInstaller installer;
        [SerializeField] private KnifeThrower knifeThrower;

        private LevelSessionService sessionService;

        private void Awake()
        {
            if (knifeThrower == null)
            {
                knifeThrower = GetComponent<KnifeThrower>();
            }

            if (installer == null)
            {
                installer = FindFirstObjectByType<GameplayInstaller>();
            }
        }

        private void OnEnable()
        {
            sessionService = installer != null ? installer.SessionService : null;
            if (knifeThrower != null)
            {
                knifeThrower.KnifeThrown += HandleKnifeThrown;
            }
        }

        private void OnDisable()
        {
            if (knifeThrower != null)
            {
                knifeThrower.KnifeThrown -= HandleKnifeThrown;
            }
        }

        private void HandleKnifeThrown()
        {
            sessionService?.HandleKnifeThrown();
        }
    }
}
