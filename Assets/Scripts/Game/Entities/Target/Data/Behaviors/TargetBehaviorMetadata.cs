using System;

namespace Game.Entities.Target.Data.Behaviors
{
    /// <summary>
    /// Provides metadata for target behavior types.
    /// </summary>
    public static class TargetBehaviorMetadata
    {
        /// <summary>
        /// Gets the configuration type associated with the specified kind.
        /// </summary>
        /// <param name="kind">Behavior kind.</param>
        /// <returns>Configuration type.</returns>
        public static Type GetConfigType(TargetBehaviorKind kind)
        {
            return kind switch
            {
                TargetBehaviorKind.ConstantSpin => typeof(Local.ConstantSpinBehaviorConfig),
                TargetBehaviorKind.PulsatingSpin => typeof(Local.PulsatingSpinBehaviorConfig),
                TargetBehaviorKind.FlippingSpin => typeof(Local.FlippingSpinBehaviorConfig),
                TargetBehaviorKind.CurveSpin => typeof(Local.CurveSpinBehaviorConfig),
                TargetBehaviorKind.Orbit => typeof(World.OrbitBehaviorConfig),
                _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
            };
        }

        /// <summary>
        /// Gets the execution scope for the specified behavior kind.
        /// </summary>
        /// <param name="kind">Behavior kind.</param>
        /// <returns>Behavior scope.</returns>
        public static TargetBehaviorScope GetScope(TargetBehaviorKind kind)
        {
            return kind switch
            {
                TargetBehaviorKind.ConstantSpin => TargetBehaviorScope.Local,
                TargetBehaviorKind.PulsatingSpin => TargetBehaviorScope.Local,
                TargetBehaviorKind.FlippingSpin => TargetBehaviorScope.Local,
                TargetBehaviorKind.CurveSpin => TargetBehaviorScope.Local,
                TargetBehaviorKind.Orbit => TargetBehaviorScope.World,
                _ => TargetBehaviorScope.Local
            };
        }
    }
}

