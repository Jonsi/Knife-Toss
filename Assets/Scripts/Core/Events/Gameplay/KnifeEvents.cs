using UnityEngine;

namespace KnifeToss.Core.Events.Gameplay
{
    /// <summary>
    /// Event published when a knife gets stuck.
    /// </summary>
    public readonly struct KnifeStuckEvent { }

    /// <summary>
    /// Event published when a knife gets stuck at a specific position.
    /// </summary>
    public readonly struct KnifeStuckAtEvent
    {
        public readonly Vector3 Position;
        public KnifeStuckAtEvent(Vector3 pos) => Position = pos;
    }
}

