using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KnifeToss.Presentation.Gameplay
{
    /// <summary>
    /// Adapts an InputAction to the throw input abstraction.
    /// </summary>
    public sealed class InputActionThrowInput : MonoBehaviour, IThrowInput
    {
        [SerializeField] private InputActionReference actionReference;

        public event Action Performed;

        private void OnEnable()
        {
            if (actionReference != null && actionReference.action != null)
            {
                actionReference.action.Enable();
                actionReference.action.performed += HandlePerformed;
            }
        }

        private void OnDisable()
        {
            if (actionReference != null && actionReference.action != null)
            {
                actionReference.action.performed -= HandlePerformed;
                actionReference.action.Disable();
            }
        }

        public void Enable()
        {
            actionReference?.action?.Enable();
        }

        public void Disable()
        {
            actionReference?.action?.Disable();
        }

        private void HandlePerformed(InputAction.CallbackContext _)
        {
            Performed?.Invoke();
        }
    }
}
