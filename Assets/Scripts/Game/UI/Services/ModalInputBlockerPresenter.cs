using UnityEngine;
using VContainer;

namespace Game.UI.Services
{
    /// <summary>
    /// Drives a full-screen UI scrim from scene-scoped modal input authority.
    /// Keep this object active and order its raycast-target Graphic above screen
    /// content but below modal panels.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class ModalInputBlockerPresenter : MonoBehaviour, System.IDisposable
    {
        [SerializeField] private CanvasGroup _CanvasGroup;
        [SerializeField] private bool _ShowVisualWhileBlocking = true;

        private IModalStackCoordinator _Coordinator;
        private bool _IsSubscribed;

        public bool IsBlocking => _CanvasGroup != null && _CanvasGroup.blocksRaycasts;

        [Inject]
        public void Construct(IModalStackCoordinator coordinator)
        {
            Unsubscribe();
            _Coordinator = coordinator;

            if (isActiveAndEnabled)
            {
                SubscribeAndRefresh();
            }
        }

        private void Awake()
        {
            EnsureCanvasGroup();
        }

        private void OnEnable()
        {
            SubscribeAndRefresh();
        }

        private void OnDisable()
        {
            Dispose();
        }

        private void OnDestroy()
        {
            // OnDisable already restored the CanvasGroup. During destruction Unity may
            // invalidate the component before this callback completes, so only detach
            // the non-Unity subscription here.
            Unsubscribe();
            _CanvasGroup = null;
        }

        public void Dispose()
        {
            Unsubscribe();
            Apply(false);
        }

        private void SubscribeAndRefresh()
        {
            EnsureCanvasGroup();

            if (_Coordinator == null)
            {
                Apply(false);
                return;
            }

            if (!_IsSubscribed)
            {
                _Coordinator.StateChanged += OnModalStateChanged;
                _IsSubscribed = true;
            }

            Apply(_Coordinator.State.BlocksUnderlyingInput);
        }

        private void Unsubscribe()
        {
            if (!_IsSubscribed || _Coordinator == null)
            {
                _IsSubscribed = false;
                return;
            }

            _Coordinator.StateChanged -= OnModalStateChanged;
            _IsSubscribed = false;
        }

        private void OnModalStateChanged(ModalStackState state)
        {
            Apply(state.BlocksUnderlyingInput);
        }

        private void Apply(bool isBlocking)
        {
            EnsureCanvasGroup();
            if (_CanvasGroup == null)
            {
                return;
            }

            _CanvasGroup.alpha = isBlocking && _ShowVisualWhileBlocking ? 1f : 0f;
            _CanvasGroup.blocksRaycasts = isBlocking;
            _CanvasGroup.interactable = false;
        }

        private void EnsureCanvasGroup()
        {
            if (this == null)
            {
                return;
            }

            if (_CanvasGroup == null)
            {
                _CanvasGroup = GetComponent<CanvasGroup>();
            }
        }
    }
}
