using UnityEngine;

namespace KnifeToss.Game.Knife.Data
{
    /// <summary>
    /// Payload describing a knife impact outcome.
    /// </summary>
    public struct KnifeImpactEventData
    {
        public KnifeImpactType impactType;
        public Vector2 impactPoint;

        public KnifeImpactEventData(KnifeImpactType impactType, Vector2 impactPoint)
        {
            this.impactType = impactType;
            this.impactPoint = impactPoint;
        }
    }
}

