namespace Game.Entities.Target.Data.Behaviors
{
    /// <summary>
    /// Supported local behavior types.
    /// </summary>
    public enum TargetLocalBehaviorKind
    {
        ConstantSpin = 0,
        PulsatingSpin = 1,
        FlippingSpin = 2,
        CurveSpin = 3
    }

    /// <summary>
    /// Supported world behavior types.
    /// </summary>
    public enum TargetWorldBehaviorKind
    {
        Orbit = 0
    }

}
