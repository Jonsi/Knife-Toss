using KnifeToss.Core.Data.Commands;
using UnityEngine;

namespace Infrastructure.Events
{
    [CreateAssetMenu(fileName = "CommandEventChannel", menuName = "Knife Toss/Events/Command Event Channel")]
    public class CommandEventChannel : EventChannel<CommandEventData>
    {
    }
}

