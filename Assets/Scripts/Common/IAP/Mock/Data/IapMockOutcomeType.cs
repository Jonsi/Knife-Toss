namespace Common.IAP.Data
{
    /// <summary>
    /// Mock outcome modes for QA purchase simulation.
    /// </summary>
    public enum IapMockOutcomeType
    {
        Success,
        Cancelled,
        Deferred,
        Failed,
        AlreadyOwned
    }
}
