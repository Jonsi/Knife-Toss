using UnityEngine;

namespace Game.Entities.Knife.Data
{
    /// <summary>
    /// Payload describing a knife impact outcome.
    /// </summary>
    public struct KnifeImpactEventData
    {
        public readonly KnifeImpactType ImpactType;
        public readonly Vector2 ImpactPosition;
        public readonly bool CountsAsFailure;

        public KnifeImpactEventData(KnifeImpactType impactType)
        {
            ImpactType = impactType;
            ImpactPosition = Vector2.zero;
            CountsAsFailure = true;
        }

        public KnifeImpactEventData(KnifeImpactType impactType, Vector2 impactPosition)
        {
            ImpactType = impactType;
            ImpactPosition = impactPosition;
            CountsAsFailure = true;
        }

        public KnifeImpactEventData(KnifeImpactType impactType, Vector2 impactPosition, bool countsAsFailure)
        {
            ImpactType = impactType;
            ImpactPosition = impactPosition;
            CountsAsFailure = countsAsFailure;
        }
    }
}
