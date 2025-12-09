using UnityEngine;

namespace KnifeToss.Core.Data.Knife
{
    /// <summary>
    /// Payload describing a knife impact outcome.
    /// </summary>
    public struct KnifeImpactEventData
    {
        public KnifeImpactType ImpactType;
        public Vector2 ImpactPoint;

        public KnifeImpactEventData(KnifeImpactType impactType, Vector2 impactPoint)
        {
            ImpactType = impactType;
            ImpactPoint = impactPoint;
        }
    }
}

