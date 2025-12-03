using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KnifeToss.Presentation.Gameplay
{



    public sealed class InputActionThrowInput : MonoBehaviour, IThrowInput
    {
        [SerializeField] private InputActionReference actionReference;

        public event Action Performed;

        /// <summary>
        /// Performs the OnEnable operation.
        /// </summary>
        private void OnEnable()
        {
            if (actionReference != null && actionReference.action != null)
            {
                actionReference.action.Enable();
                actionReference.action.started += HandleStarted;
            }
        }

        /// <summary>
        /// Performs the OnDisable operation.
        /// </summary>
        private void OnDisable()
        {
            if (actionReference != null && actionReference.action != null)
            {
                actionReference.action.started -= HandleStarted;
                actionReference.action.Disable();
            }
        }

        /// <summary>
        /// Performs the Enable operation.
        /// </summary>
        public void Enable()
        {
            actionReference?.action?.Enable();
        }

        /// <summary>
        /// Performs the Disable operation.
        /// </summary>
        public void Disable()
        {
            actionReference?.action?.Disable();
        }

        /// <summary>
        /// Performs the HandleStarted operation.
        /// </summary>
        private void HandleStarted(InputAction.CallbackContext _)
        {
            Performed?.Invoke();
        }
    }
}
