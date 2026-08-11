using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Game.UI.Services
{
    /// <summary>
    /// Capabilities claimed while a modal presentation is active.
    /// Claims from overlapping presentations are combined, so a fading modal cannot
    /// accidentally return input authority to the content underneath it.
    /// </summary>
    [Flags]
    public enum ModalInteractionAuthority
    {
        None = 0,
        BlocksUnderlyingInput = 1 << 0,
        Modal = BlocksUnderlyingInput
    }

    /// <summary>
    /// Immutable aggregate state exposed to input blockers.
    /// </summary>
    public readonly struct ModalStackState
    {
        public ModalStackState(
            int activeCount,
            object topOwner,
            ModalInteractionAuthority authority,
            int revision)
        {
            ActiveCount = activeCount;
            TopOwner = topOwner;
            Authority = authority;
            Revision = revision;
        }

        public int ActiveCount { get; }
        public object TopOwner { get; }
        public ModalInteractionAuthority Authority { get; }
        public int Revision { get; }
        public bool HasActiveModal => ActiveCount > 0;
        public bool BlocksUnderlyingInput =>
            (Authority & ModalInteractionAuthority.BlocksUnderlyingInput) != 0;
    }

    /// <summary>
    /// Lifetime handle for one modal owner. Disposing the handle is idempotent and
    /// releases only the acquisition that created it.
    /// </summary>
    public interface IModalRegistration : IDisposable
    {
        object Owner { get; }
        bool IsActive { get; }
    }

    /// <summary>
    /// Scene-scoped modal state. UI presentation owns registrations; gameplay,
    /// economy, store, and navigation services only observe their own domain state.
    /// </summary>
    public interface IModalStackCoordinator
    {
        ModalStackState State { get; }
        event Action<ModalStackState> StateChanged;

        IModalRegistration Acquire(
            object owner,
            ModalInteractionAuthority authority = ModalInteractionAuthority.Modal);
    }

    /// <summary>
    /// Deterministic modal stack and aggregate input-authority model.
    /// The most recently acquired active owner is the top owner. Authority flags use
    /// logical OR across every active owner, rather than letting the top owner weaken
    /// an older presentation that is still visible during a transition.
    /// </summary>
    public sealed class ModalStackCoordinator : IModalStackCoordinator, IDisposable
    {
        private readonly List<Registration> _Stack = new List<Registration>();
        private readonly Dictionary<object, Registration> _Registrations =
            new Dictionary<object, Registration>(ReferenceComparer.Instance);

        private ModalStackState _State = new ModalStackState(
            0,
            null,
            ModalInteractionAuthority.None,
            0);
        private bool _IsDisposed;

        public ModalStackState State => _State;

        public event Action<ModalStackState> StateChanged;

        public IModalRegistration Acquire(
            object owner,
            ModalInteractionAuthority authority = ModalInteractionAuthority.Modal)
        {
            if (owner == null)
            {
                throw new ArgumentNullException(nameof(owner));
            }

            if (_IsDisposed)
            {
                throw new ObjectDisposedException(nameof(ModalStackCoordinator));
            }

            if (_Registrations.TryGetValue(owner, out Registration existing))
            {
                return existing;
            }

            var registration = new Registration(this, owner, authority);
            _Registrations.Add(owner, registration);
            _Stack.Add(registration);
            PublishState();
            return registration;
        }

        public void Dispose()
        {
            if (_IsDisposed)
            {
                return;
            }

            _IsDisposed = true;

            if (_Stack.Count == 0)
            {
                StateChanged = null;
                return;
            }

            for (var i = 0; i < _Stack.Count; i++)
            {
                _Stack[i].DeactivateWithoutCallback();
            }

            _Stack.Clear();
            _Registrations.Clear();
            PublishState();
            StateChanged = null;
        }

        private void Release(Registration registration)
        {
            if (_IsDisposed || registration == null || !registration.IsActive)
            {
                return;
            }

            if (!_Registrations.TryGetValue(registration.Owner, out Registration current) ||
                !ReferenceEquals(current, registration))
            {
                registration.DeactivateWithoutCallback();
                return;
            }

            registration.DeactivateWithoutCallback();
            _Registrations.Remove(registration.Owner);
            _Stack.Remove(registration);
            PublishState();
        }

        private void PublishState()
        {
            ModalInteractionAuthority aggregateAuthority = ModalInteractionAuthority.None;
            for (var i = 0; i < _Stack.Count; i++)
            {
                aggregateAuthority |= _Stack[i].Authority;
            }

            object topOwner = _Stack.Count > 0 ? _Stack[_Stack.Count - 1].Owner : null;
            _State = new ModalStackState(
                _Stack.Count,
                topOwner,
                aggregateAuthority,
                _State.Revision + 1);
            NotifyStateChanged();
        }

        private void NotifyStateChanged()
        {
            Delegate[] handlers = StateChanged?.GetInvocationList();
            if (handlers == null)
            {
                return;
            }

            for (var i = 0; i < handlers.Length; i++)
            {
                var handler = (Action<ModalStackState>)handlers[i];
                if (handler.Target is UnityEngine.Object unityTarget && unityTarget == null)
                {
                    StateChanged -= handler;
                    continue;
                }

                handler(_State);
            }
        }

        private sealed class Registration : IModalRegistration
        {
            private ModalStackCoordinator _Coordinator;

            public Registration(
                ModalStackCoordinator coordinator,
                object owner,
                ModalInteractionAuthority authority)
            {
                _Coordinator = coordinator;
                Owner = owner;
                Authority = authority;
            }

            public object Owner { get; }
            public ModalInteractionAuthority Authority { get; }
            public bool IsActive => _Coordinator != null;

            public void Dispose()
            {
                _Coordinator?.Release(this);
            }

            public void DeactivateWithoutCallback()
            {
                _Coordinator = null;
            }
        }

        private sealed class ReferenceComparer : IEqualityComparer<object>
        {
            public static readonly ReferenceComparer Instance = new ReferenceComparer();

            public new bool Equals(object left, object right)
            {
                return ReferenceEquals(left, right);
            }

            public int GetHashCode(object value)
            {
                return RuntimeHelpers.GetHashCode(value);
            }
        }
    }
}
