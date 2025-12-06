using System.Collections.Generic;
using UnityEngine;

namespace Presentation.Gameplay
{
    /// <summary>
    /// Lightweight registry that maps target transforms to their knives to break indicator.
    /// </summary>
    public static class TargetKnivesToBreakIndicatorRegistry
    {
        private static readonly Dictionary<Transform, TargetKnivesToBreakIndicator> Indicators = new Dictionary<Transform, TargetKnivesToBreakIndicator>();

        public static void Register(TargetKnivesToBreakIndicator indicator)
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

        public static void Unregister(TargetKnivesToBreakIndicator indicator)
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

        public static bool TryResolve(Transform startingPoint, out TargetKnivesToBreakIndicator indicator)
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

