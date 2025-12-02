using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KnifeToss.Presentation.Gameplay
{



    public sealed class InputActionThrowInput : MonoBehaviour, IThrowInput
    {
        [SerializeField] private InputActionReference actionReference;

        public event Action Performed;

        private void OnEnable()
        {
            if (actionReference != null && actionReference.action != null)
            {
                actionReference.action.Enable();
                actionReference.action.started += HandleStarted;
            }
        }

        private void OnDisable()
        {
            if (actionReference != null && actionReference.action != null)
            {
                actionReference.action.started -= HandleStarted;
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

        private void HandleStarted(InputAction.CallbackContext _)
        {
            Performed?.Invoke();
        }
    }
}
