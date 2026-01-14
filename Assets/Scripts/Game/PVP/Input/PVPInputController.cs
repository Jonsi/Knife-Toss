using System;
using Game.Input;

namespace Game.PVP.Input
{
    public class PVPInputController : IDisposable
    {
        private readonly InputAdapter _InputAdapter;
        private bool _IsInputLocked;

        public event Action OnThrowRequested;

        public PVPInputController(InputAdapter inputAdapter)
        {
            _InputAdapter = inputAdapter;
            _InputAdapter.OnThrowRequested += HandleThrowRequested;
        }

        public void SetInputLocked(bool locked)
        {
            _IsInputLocked = locked;
        }
        
        public void Initialize()
        {
            _InputAdapter.Initialize();
        }

        private void HandleThrowRequested()
        {
            if (_IsInputLocked) return;
            OnThrowRequested?.Invoke();
        }

        public void Dispose()
        {
            if (_InputAdapter != null)
            {
                _InputAdapter.OnThrowRequested -= HandleThrowRequested;
                _InputAdapter.Dispose();
            }
        }
    }
}
