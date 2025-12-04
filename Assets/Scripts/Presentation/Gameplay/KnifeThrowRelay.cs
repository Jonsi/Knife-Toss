using KnifeToss.Application.Services;
using UnityEngine;
using VContainer;

namespace KnifeToss.Presentation.Gameplay
{



    /// <summary>
    /// Represents the KnifeThrowRelay class.
    /// </summary>
    [RequireComponent(typeof(KnifeThrower))]
    public class KnifeThrowRelay : MonoBehaviour
    {
        [SerializeField] private KnifeThrower knifeThrower;

        private LevelSessionService sessionService;

        /// <summary>
        /// Performs the Awake operation.
        /// </summary>
        private void Awake()
        {
            if (knifeThrower == null)
            {
                knifeThrower = GetComponent<KnifeThrower>();
            }
        }

        /// <summary>
        /// Performs the Construct operation.
        /// </summary>
        [Inject]
        public void Construct(LevelSessionService levelSessionService)
        {
            sessionService = levelSessionService;
        }

        /// <summary>
        /// Performs the OnEnable operation.
        /// </summary>
        private void OnEnable()
        {
            if (knifeThrower != null)
            {
                knifeThrower.KnifeThrown += HandleKnifeThrown;
                knifeThrower.KnifeResolved += HandleKnifeResolved;
            }
        }

        /// <summary>
        /// Performs the OnDisable operation.
        /// </summary>
        private void OnDisable()
        {
            if (knifeThrower != null)
            {
                knifeThrower.KnifeThrown -= HandleKnifeThrown;
                knifeThrower.KnifeResolved -= HandleKnifeResolved;
            }
        }

        /// <summary>
        /// Performs the HandleKnifeThrown operation.
        /// </summary>
        private void HandleKnifeThrown()
        {
            if (sessionService == null)
            {
                return;
            }

            sessionService.HandleKnifeThrown();
        }

        /// <summary>
        /// Performs the HandleKnifeResolved operation.
        /// </summary>
        private void HandleKnifeResolved()
        {
            if (sessionService == null)
            {
                return;
            }

            sessionService.HandleKnifeResolved();
        }
    }
}
