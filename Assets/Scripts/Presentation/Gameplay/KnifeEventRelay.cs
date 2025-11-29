using KnifeToss.Application.Services;
using KnifeToss.Core.Events;
using KnifeToss.Installers;
using UnityEngine;

namespace KnifeToss.Presentation.Gameplay
{
    /// <summary>
    /// Converts ScriptableObject knife impact events into session service calls.
    /// </summary>
    public class KnifeEventRelay : MonoBehaviour
    {
        [SerializeField] private GameplayInstaller installer;
        [SerializeField] private KnifeImpactEventChannel eventChannel;

        private LevelSessionService sessionService;

        private void Awake()
        {
            if (installer == null)
            {
                installer = FindFirstObjectByType<GameplayInstaller>();
            }
        }

        private void OnEnable()
        {
            sessionService = installer != null ? installer.SessionService : null;
            if (eventChannel == null && installer != null)
            {
                eventChannel = installer.ImpactChannel;
            }

            if (eventChannel != null)
            {
                eventChannel.KnifeStuck += HandleKnifeStuck;
                eventChannel.KnifeFailed += HandleKnifeFailed;
            }
        }

        private void OnDisable()
        {
            if (eventChannel != null)
            {
                eventChannel.KnifeStuck -= HandleKnifeStuck;
                eventChannel.KnifeFailed -= HandleKnifeFailed;
            }
        }

        private void HandleKnifeStuck()
        {
            sessionService?.HandleKnifeStuck();
        }

        private void HandleKnifeFailed()
        {
            sessionService?.HandleKnifeFailure();
        }
    }
}
