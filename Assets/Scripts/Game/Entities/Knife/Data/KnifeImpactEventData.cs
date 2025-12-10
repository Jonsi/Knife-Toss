namespace Game.Entities.Knife.Data
{
    /// <summary>
    /// Payload describing a knife impact outcome.
    /// </summary>
    public struct KnifeImpactEventData
    {
        public readonly KnifeImpactType ImpactType;

        public KnifeImpactEventData(KnifeImpactType impactType)
        {
            ImpactType = impactType;
        }
    }
}

