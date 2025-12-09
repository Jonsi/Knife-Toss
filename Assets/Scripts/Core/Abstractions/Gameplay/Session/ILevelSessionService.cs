using UnityEngine;

namespace KnifeToss.Core.Abstractions.Gameplay.Session
{
    /// <summary>
    /// Contract for managing per-level session state and related events.
    /// </summary>
    public interface ILevelSessionService
    {
        bool IsPlaying { get; }
        int KnivesThrown { get; }
        int KnivesStuck { get; }
        int KnivesRemaining { get; }

        /// <summary>
        /// Initializes or restarts the session with current rules.
        /// </summary>
        void StartSession();

        /// <summary>
        /// Attempts to register a throw; returns false if throws are blocked.
        /// </summary>
        bool TryRegisterThrow();

        /// <summary>
        /// Registers a successful stick at the given impact point.
        /// </summary>
        void RegisterStick(Vector2 impactPoint);

        /// <summary>
        /// Registers a bounce/fail at the given impact point.
        /// </summary>
        void RegisterBounce(Vector2 impactPoint);
    }
}

