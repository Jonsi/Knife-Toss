using KnifeToss.Presentation.Gameplay.Projectiles;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KnifeToss.Presentation.Gameplay
{
    public class KnifeThrower : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject knifePrefab;
        [SerializeField] private Transform launcherTransform;
        [SerializeField] private InputActionAsset inputActionAsset;

        [Header("Throwing Settings")]
        [SerializeField] private float throwSpeed = 10f;

        private InputAction attackAction;
        private Knife currentKnife;
        private bool isKnifeInFlight;
        private int maxKnives;
        private int knivesThrown;
        private bool canThrow = true;
        private Transform knivesParent;

        public event System.Action KnifeThrown;

        private void Awake()
        {
            if (inputActionAsset != null)
            {
                attackAction = inputActionAsset.FindAction("Player/Attack");
            }

            if (launcherTransform == null)
            {
                launcherTransform = transform;
            }
        }

        private void OnEnable()
        {
            if (attackAction != null)
            {
                attackAction.Enable();
                attackAction.performed += OnThrowInput;
            }
        }

        private void OnDisable()
        {
            if (attackAction != null)
            {
                attackAction.performed -= OnThrowInput;
                attackAction.Disable();
            }
        }

        private void OnThrowInput(InputAction.CallbackContext context)
        {
            if (!canThrow || isKnifeInFlight || knifePrefab == null || launcherTransform == null)
            {
                return;
            }

            if (maxKnives > 0 && knivesThrown >= maxKnives)
            {
                return;
            }

            ThrowKnife();
        }

        private void ThrowKnife()
        {
            GameObject knifeObject = Instantiate(knifePrefab, launcherTransform.position, launcherTransform.rotation);

            if (knivesParent != null)
            {
                knifeObject.transform.SetParent(knivesParent);
            }

            currentKnife = knifeObject.GetComponent<Knife>();

            if (currentKnife != null)
            {
                currentKnife.Initialize(this, throwSpeed);
                isKnifeInFlight = true;
                knivesThrown++;
                KnifeThrown?.Invoke();
            }
        }

        public void OnKnifeResolved()
        {
            isKnifeInFlight = false;
            currentKnife = null;
        }

        public void ResetState()
        {
            if (currentKnife != null)
            {
                Destroy(currentKnife.gameObject);
                currentKnife = null;
            }

            isKnifeInFlight = false;
            knivesThrown = 0;
            canThrow = true;
        }

        public void SetMaxKnives(int max)
        {
            maxKnives = max;
            knivesThrown = 0;
        }

        public void SetCanThrow(bool allow)
        {
            canThrow = allow;
        }

        public void SetKnivesParent(Transform parent)
        {
            knivesParent = parent;
        }
    }
}

