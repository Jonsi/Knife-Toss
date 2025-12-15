namespace Game.Entities.Target.Data.Behaviors
{
    /// <summary>
    /// Supported behavior types for targets.
    /// </summary>
    public enum TargetBehaviorKind
    {
        ConstantSpin = 0,
        PulsatingSpin = 1,
        FlippingSpin = 2,
        CurveSpin = 3,
        Orbit = 100
    }

    /// <summary>
    /// Logical scope for a behavior.
    /// </summary>
    public enum TargetBehaviorScope
    {
        Local,
        World
    }
}

