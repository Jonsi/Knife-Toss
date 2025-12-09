using System;
using System.Threading;
using Core.Abstractions;
using Infrastructure.Input;
using UnityEngine.InputSystem;

namespace Infrastructure.Input
{
    /// <summary>
    /// Adapts Unity Input System input to the game's player throw input listener interface.
    /// Wraps Unity's input system and converts throw input events into game events.
    /// </summary>
    public class InputAdapter : IPlayerThrowInputListener, IDisposable
    {
        private InputSystem_Actions _inputActions;
        private bool _isInitialized;
        private CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        /// Event raised when the player requests a knife throw.
        /// </summary>
        public event Action OnThrowRequested;

        /// <summary>
        /// Initializes the input system and begins listening for throw input.
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized)
                return;

            _inputActions = new InputSystem_Actions();
            _inputActions.Player.Throw.performed += OnThrowPerformed;
            _inputActions.Player.Throw.Enable();
            _cancellationTokenSource = new CancellationTokenSource();
            _isInitialized = true;
        }

        /// <summary>
        /// Disposes of resources and stops listening for input.
        /// </summary>
        public void Dispose()
        {
            if (!_isInitialized)
                return;

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;

            if (_inputActions != null)
            {
                _inputActions.Player.Throw.performed -= OnThrowPerformed;
                _inputActions.Player.Throw.Disable();
                _inputActions.Dispose();
                _inputActions = null;
            }

            _isInitialized = false;
        }

        private void OnThrowPerformed(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                OnThrowRequested?.Invoke();
            }
        }
    }
}

