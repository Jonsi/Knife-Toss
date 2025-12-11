using Common.Events;
using UnityEngine;

namespace Game.KnifeThrower.Events
{
    /// <summary>
    /// Event channel raised when a knife is spawned and launched.
    /// </summary>
    [CreateAssetMenu(fileName = "KnifeThrownEventChannel", menuName = "Knife Toss/Events/Knife Thrown Event Channel")]
    public class KnifeThrownEventChannel : VoidEventChannel
    {
    }
}

