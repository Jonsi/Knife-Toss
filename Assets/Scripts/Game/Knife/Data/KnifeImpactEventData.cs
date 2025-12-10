using UnityEngine;

namespace KnifeToss.Game.Knife.Data
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

