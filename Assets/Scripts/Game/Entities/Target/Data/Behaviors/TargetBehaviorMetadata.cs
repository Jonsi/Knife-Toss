using System;

namespace Game.Entities.Target.Data.Behaviors
{
    /// <summary>
    /// Provides metadata for target behavior types.
    /// </summary>
    public static class TargetBehaviorMetadata
    {
        public static Type GetLocalConfigType(TargetLocalBehaviorKind kind)
        {
            return kind switch
            {
                TargetLocalBehaviorKind.ConstantSpin => typeof(Local.ConstantSpinBehaviorConfig),
                TargetLocalBehaviorKind.PulsatingSpin => typeof(Local.PulsatingSpinBehaviorConfig),
                TargetLocalBehaviorKind.FlippingSpin => typeof(Local.FlippingSpinBehaviorConfig),
                TargetLocalBehaviorKind.CurveSpin => typeof(Local.CurveSpinBehaviorConfig),
                _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
            };
        }

        public static Type GetWorldConfigType(TargetWorldBehaviorKind kind)
        {
            return kind switch
            {
                TargetWorldBehaviorKind.Orbit => typeof(World.OrbitBehaviorConfig),
                _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
            };
        }

    }
}
