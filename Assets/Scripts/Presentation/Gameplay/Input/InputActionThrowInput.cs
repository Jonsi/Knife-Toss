using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Presentation.Gameplay.Input
{



    public sealed class InputActionThrowInput : MonoBehaviour, IThrowInput
    {
        [SerializeField] private InputActionReference actionReference;

        private Vector2 lastScreenPosition;

        public event Action<Vector2> Pressed;
        public event Action<Vector2> Released;
        public Vector2 CurrentScreenPosition => ResolveScreenPosition();

        /// <summary>
        /// Performs the OnEnable operation.
        /// </summary>
        private void OnEnable()
        {
            if (actionReference != null && actionReference.action != null)
            {
                actionReference.action.Enable();
                actionReference.action.started += HandleStarted;
                actionReference.action.canceled += HandleCanceled;
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
                actionReference.action.canceled -= HandleCanceled;
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
        private void HandleStarted(InputAction.CallbackContext context)
        {
            if (ShouldIgnorePointerInput(context))
            {
                return;
            }

            Vector2 screenPosition = ResolveScreenPosition();
            lastScreenPosition = screenPosition;
            Pressed?.Invoke(screenPosition);
        }

        private void HandleCanceled(InputAction.CallbackContext context)
        {
            if (ShouldIgnorePointerInput(context))
            {
                return;
            }

            Vector2 screenPosition = ResolveScreenPosition();
            lastScreenPosition = screenPosition;
            Released?.Invoke(screenPosition);
        }

        private Vector2 ResolveScreenPosition()
        {
            if (TryGetScreenPosition(out Vector2 position))
            {
                return position;
            }

            return lastScreenPosition;
        }

        private bool TryGetScreenPosition(out Vector2 position)
        {
            position = Vector2.zero;

            if (actionReference != null && actionReference.action != null)
            {
                InputAction action = actionReference.action;
                InputControl control = action.activeControl;

                if (control != null && control.valueType == typeof(Vector2))
                {
                    position = action.ReadValue<Vector2>();
                    return position != Vector2.zero;
                }
            }

            if (UnityEngine.InputSystem.Mouse.current != null)
            {
                position = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
                return true;
            }

            if (UnityEngine.InputSystem.Touchscreen.current != null)
            {
                position = UnityEngine.InputSystem.Touchscreen.current.primaryTouch.position.ReadValue();
                return true;
            }

            return false;
        }

        private bool ShouldIgnorePointerInput(InputAction.CallbackContext context)
        {
            if (EventSystem.current == null)
            {
                return false;
            }

            if (context.control?.device is Pointer pointerDevice)
            {
                if (pointerDevice is Mouse)
                {
                    return EventSystem.current.IsPointerOverGameObject();
                }

                if (pointerDevice is Touchscreen touchscreen)
                {
                    foreach (TouchControl touch in touchscreen.touches)
                    {
                        if (!touch.press.isPressed)
                        {
                            continue;
                        }

                        int touchId = touch.touchId.ReadValue();
                        if (EventSystem.current.IsPointerOverGameObject(touchId))
                        {
                            return true;
                        }
                    }
                }
            }

            return EventSystem.current.IsPointerOverGameObject();
        }
    }
}
