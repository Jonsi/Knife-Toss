using System.Collections.Generic;
using UnityEngine;

namespace Presentation.Gameplay
{
    /// <summary>
    /// Defines the ITargetScaleProvider interface.
    /// </summary>
    public interface ITargetScaleProvider
    {
        Transform PrimaryTarget { get; }
        float PrimaryScale { get; }
        IReadOnlyList<Transform> AllTargets { get; }
    }
}
