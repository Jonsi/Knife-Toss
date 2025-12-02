using KnifeToss.Application.Services;
using UnityEngine;
using VContainer;

namespace KnifeToss.Presentation.Gameplay
{



    [RequireComponent(typeof(KnifeThrower))]
    public class KnifeThrowRelay : MonoBehaviour
    {
        [SerializeField] private KnifeThrower knifeThrower;

        private LevelSessionService sessionService;

        private void Awake()
        {
            if (knifeThrower == null)
            {
                knifeThrower = GetComponent<KnifeThrower>();
            }
        }

        [Inject]
        public void Construct(LevelSessionService levelSessionService)
        {
            sessionService = levelSessionService;
        }

        private void OnEnable()
        {
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
