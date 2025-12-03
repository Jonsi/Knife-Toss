using UnityEngine;

namespace KnifeToss.Presentation.Gameplay
{



    internal static class KnifeSetupUtility
    {
        /// <summary>
        /// Performs the EnableColliders operation.
        /// </summary>
        public static void EnableColliders(GameObject knifeObject, bool enabled)
        {
            if (knifeObject == null)
            {
                return;
            }

            KnifeView view = knifeObject.GetComponent<KnifeView>();
            view?.CacheIfNeeded();

            if (view != null)
            {
                view.SetCollidersEnabled(enabled);
                return;
            }

            Collider2D[] colliders = knifeObject.GetComponents<Collider2D>();
            for (int i = 0; i < colliders.Length; i++)
            {
                Collider2D collider = colliders[i];
                if (collider != null)
                {
                    collider.enabled = enabled;
                }
            }
        }

        /// <summary>
        /// Performs the ApplyVisuals operation.
        /// </summary>
        public static void ApplyVisuals(GameObject knifeObject, Vector3 scale, Color color)
        {
            if (knifeObject == null)
            {
                return;
            }

            Transform knifeTransform = knifeObject.transform;
            knifeTransform.localScale = scale;

            KnifeView view = knifeObject.GetComponent<KnifeView>();
            view?.CacheIfNeeded();
            view?.SetColor(color);
        }

        /// <summary>
        /// Performs the PositionAtLauncher operation.
        /// </summary>
        public static void PositionAtLauncher(GameObject knifeObject, Transform launcher)
        {
            if (knifeObject == null || launcher == null)
            {
                return;
            }

            Transform knifeTransform = knifeObject.transform;
            knifeTransform.position = launcher.position;
            knifeTransform.rotation = launcher.rotation;
        }
    }
}
