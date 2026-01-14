using System;
using System.Threading;
using Game.KnifeThrower.Interfaces;
using KnifeToss.Systems.Input;
using UnityEngine.InputSystem;

namespace Game.Input
{
    /// <summary>
    /// Adapts Unity Input System input to the game's player throw input listener interface.
    /// Wraps Unity's input system and converts throw input events into game events.
    /// </summary>
    public class InputAdapter : IPlayerThrowInputListener, IDisposable
    {
        private InputSystem_Actions _InputActions;
        private bool _IsInitialized;
        private CancellationTokenSource _CancellationTokenSource;

        /// <summary>
        /// Event raised when the player starts a throw action.
        /// </summary>
        public event Action OnThrowRequested;

        /// <summary>
        /// Event raised when the player completes a throw action.
        /// </summary>
        public event Action OnThrowReleased;

        /// <summary>
        /// Initializes the input system and begins listening for throw input.
        /// </summary>
        public void Initialize()
        {
            if (_IsInitialized)
                return;

            _InputActions = new InputSystem_Actions();
            _InputActions.Player.Throw.started += OnThrowStarted;
            _InputActions.Player.Throw.performed += OnThrowPerformed;
            _InputActions.Player.Throw.canceled += OnThrowCanceledInternal;
            _InputActions.Player.Throw.Enable();
            _CancellationTokenSource = new CancellationTokenSource();
            _IsInitialized = true;
        }

        public void Dispose()
        {
            if (!_IsInitialized)
                return;

            _CancellationTokenSource?.Cancel();
            _CancellationTokenSource?.Dispose();
            _CancellationTokenSource = null;

            if (_InputActions != null)
            {
                _InputActions.Player.Throw.started -= OnThrowStarted;
                _InputActions.Player.Throw.performed -= OnThrowPerformed;
                _InputActions.Player.Throw.canceled -= OnThrowCanceledInternal;
                _InputActions.Player.Throw.Disable();
                _InputActions.Dispose();
                _InputActions = null;
            }

            _IsInitialized = false;
        }

        private void OnThrowStarted(InputAction.CallbackContext context)
        {
            // started can also be used for press if performed is busy
        }

        private void OnThrowPerformed(InputAction.CallbackContext context)
        {
            // With "Press" interaction, performed triggers on down-stroke
            OnThrowRequested?.Invoke();
        }

        private void OnThrowCanceledInternal(InputAction.CallbackContext context)
        {
            // With "Press" interaction, canceled triggers on up-stroke (release)
            OnThrowReleased?.Invoke();
        }
    }
}

