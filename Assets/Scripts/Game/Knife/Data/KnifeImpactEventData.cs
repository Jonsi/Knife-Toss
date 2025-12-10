using UnityEngine;

namespace Game.Knife.Data
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
            this.ImpactType = impactType;
            this.ImpactPoint = impactPoint;
        }
    }
}

