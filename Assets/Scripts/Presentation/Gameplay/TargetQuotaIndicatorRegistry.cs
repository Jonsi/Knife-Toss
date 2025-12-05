using System.Collections.Generic;
using UnityEngine;

namespace Presentation.Gameplay
{
    /// <summary>
    /// Lightweight registry that maps target transforms to their quota indicator.
    /// </summary>
    public static class TargetQuotaIndicatorRegistry
    {
        private static readonly Dictionary<Transform, TargetQuotaIndicator> Indicators = new Dictionary<Transform, TargetQuotaIndicator>();

        public static void Register(TargetQuotaIndicator indicator)
        {
            if (indicator == null)
            {
                return;
            }

            Transform key = indicator.transform;
            if (key == null)
            {
                return;
            }

            Indicators[key] = indicator;
        }

        public static void Unregister(TargetQuotaIndicator indicator)
        {
            if (indicator == null)
            {
                return;
            }

            Transform key = indicator.transform;
            if (key == null)
            {
                return;
            }

            Indicators.Remove(key);
        }

        public static bool TryResolve(Transform startingPoint, out TargetQuotaIndicator indicator)
        {
            indicator = null;
            Transform current = startingPoint;
            while (current != null)
            {
                if (Indicators.TryGetValue(current, out indicator))
                {
                    return true;
                }

                current = current.parent;
            }

            return false;
        }
    }
}

