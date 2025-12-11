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

        public KnifeImpactEventData(KnifeImpactType impactType)
        {
            ImpactType = impactType;
            ImpactPosition = Vector2.zero;
        }

        public KnifeImpactEventData(KnifeImpactType impactType, Vector2 impactPosition)
        {
            ImpactType = impactType;
            ImpactPosition = impactPosition;
        }
    }
}

