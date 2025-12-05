using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Presentation.Gameplay.Input
{
    /// <summary>
    /// Bridges a Unity InputAction with the gameplay throw input contract while
    /// suppressing pointer presses that interact with UI.
    /// </summary>
    public sealed class InputActionThrowInput : MonoBehaviour, IThrowInput
    {
        [SerializeField] private InputActionReference actionReference;

        private static readonly List<RaycastResult> RaycastResults = new List<RaycastResult>();

        private Vector2 lastScreenPosition;

        private InputAction Action => actionReference != null ? actionReference.action : null;

        public event Action<Vector2> Pressed;
        public event Action<Vector2> Released;
        public Vector2 CurrentScreenPosition => ResolveScreenPosition();

        private void OnEnable()
        {
            if (Action == null)
            {
                return;
            }

            Action.started += HandleStarted;
            Action.canceled += HandleCanceled;
            Action.Enable();
        }

        private void OnDisable()
        {
            if (Action == null)
            {
                return;
            }

            Action.started -= HandleStarted;
            Action.canceled -= HandleCanceled;
            Action.Disable();
        }

        public void Enable() => Action?.Enable();

        public void Disable() => Action?.Disable();

        private void HandleStarted(InputAction.CallbackContext context)
        {
            Vector2 screenPosition = ResolveScreenPosition(context);

            if (ShouldIgnorePointerInput(context, screenPosition))
            {
                return;
            }

            lastScreenPosition = screenPosition;
            Pressed?.Invoke(screenPosition);
        }

        private void HandleCanceled(InputAction.CallbackContext context)
        {
            Vector2 screenPosition = ResolveScreenPosition(context);

            if (ShouldIgnorePointerInput(context, screenPosition))
            {
                return;
            }

            lastScreenPosition = screenPosition;
            Released?.Invoke(screenPosition);
        }

        private Vector2 ResolveScreenPosition(InputAction.CallbackContext? context = null)
        {
            if (context.HasValue && TryGetPointerPosition(context.Value, out Vector2 pointerPosition))
            {
                return pointerPosition;
            }

            if (TryGetScreenPosition(out Vector2 actionPosition))
            {
                return actionPosition;
            }

            return lastScreenPosition;
        }

        private bool TryGetPointerPosition(InputAction.CallbackContext context, out Vector2 position)
        {
            position = Vector2.zero;

            if (context.control is TouchControl touchControl)
            {
                position = touchControl.position.ReadValue();
                return position != Vector2.zero;
            }

            if (context.control?.device is Pointer pointer && pointer.position != null)
            {
                position = pointer.position.ReadValue();
                return position != Vector2.zero;
            }

            return false;
        }

        private bool TryGetScreenPosition(out Vector2 position)
        {
            position = Vector2.zero;

            if (Action != null)
            {
                InputControl control = Action.activeControl;

                if (control != null && control.valueType == typeof(Vector2))
                {
                    position = Action.ReadValue<Vector2>();
                    if (position != Vector2.zero)
                    {
                        return true;
                    }
                }
            }

            if (TryGetTouchscreenPosition(out position))
            {
                return true;
            }

            if (Mouse.current != null)
            {
                position = Mouse.current.position.ReadValue();
                return position != Vector2.zero;
            }

            return false;
        }

        private static bool TryGetTouchscreenPosition(out Vector2 position)
        {
            position = Vector2.zero;

            Touchscreen touchscreen = Touchscreen.current;
            if (touchscreen == null)
            {
                return false;
            }

            TouchControl touch = touchscreen.primaryTouch;
            if (touch == null || !touch.press.isPressed)
            {
                return false;
            }

            position = touch.position.ReadValue();
            return position != Vector2.zero;
        }

        private bool ShouldIgnorePointerInput(InputAction.CallbackContext context, Vector2 screenPosition)
        {
            if (EventSystem.current == null || screenPosition == Vector2.zero)
            {
                return false;
            }

            // Only pointer-driven controls (mouse/touch) can interact with UI.
            if (context.control?.device is Pointer == false)
            {
                return false;
            }

            return RaycastUI(screenPosition);
        }

        private bool RaycastUI(Vector2 screenPosition)
        {
            EventSystem eventSystem = EventSystem.current;
            if (eventSystem == null)
            {
                return false;
            }

            PointerEventData pointerData = new PointerEventData(eventSystem)
            {
                position = screenPosition
            };

            RaycastResults.Clear();
            eventSystem.RaycastAll(pointerData, RaycastResults);
            bool hit = RaycastResults.Count > 0;
            RaycastResults.Clear();

            return hit;
        }
    }
}
