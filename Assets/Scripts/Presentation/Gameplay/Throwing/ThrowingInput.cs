using System;
using UnityEngine;
using KnifeToss.Presentation.Gameplay;

namespace KnifeToss.Presentation.Gameplay.Throwing
{
    /// <summary>
    /// Handles input for throwing knives. Wraps IThrowInput and provides events.
    /// </summary>
    public sealed class ThrowingInput : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour throwInputSource;

        private IThrowInput _throwInput;
        private bool _isEnabled;

        public event Action<Vector2> ThrowPressed;
        public event Action<Vector2> ThrowReleased;
        public event Action<Vector2> AimUpdated;

        public Vector2 CurrentAimDirection { get; private set; }

        private void Awake()
        {
            _throwInput = throwInputSource as IThrowInput ?? new NullThrowInput();
        }

        private void OnEnable()
        {
            if (_throwInput != null)
            {
                _throwInput.Pressed += OnThrowPressed;
                _throwInput.Released += OnThrowReleased;
                _throwInput.Enable();
            }
            _isEnabled = true;
        }

        private void OnDisable()
        {
            if (_throwInput != null)
            {
                _throwInput.Pressed -= OnThrowPressed;
                _throwInput.Released -= OnThrowReleased;
                _throwInput.Disable();
            }
            _isEnabled = false;
        }

        private void Update()
        {
            if (!_isEnabled || _throwInput == null)
            {
                return;
            }

            CurrentAimDirection = _throwInput.CurrentScreenPosition;
            AimUpdated?.Invoke(CurrentAimDirection);
        }

        private void OnThrowPressed(Vector2 screenPosition)
        {
            ThrowPressed?.Invoke(screenPosition);
        }

        private void OnThrowReleased(Vector2 screenPosition)
        {
            ThrowReleased?.Invoke(screenPosition);
        }

        private sealed class NullThrowInput : IThrowInput
        {
            public event Action<Vector2> Pressed { add { } remove { } }
            public event Action<Vector2> Released { add { } remove { } }
            public Vector2 CurrentScreenPosition => Vector2.zero;
            public void Enable() { }
            public void Disable() { }
        }
    }
}

