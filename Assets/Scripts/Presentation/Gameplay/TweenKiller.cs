using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace KnifeToss.Presentation.Gameplay
{



    internal static class TweenKiller
    {
        /// <summary>
        /// Performs the Kill operation.
        /// </summary>
        public static void Kill(object target)
        {
            if (target == null)
            {
                return;
            }

            DOTween.Kill(target, true);
        }

        /// <summary>
        /// Performs the Kill operation.
        /// </summary>
        public static void Kill(GameObject gameObject)
        {
            if (gameObject == null)
            {
                return;
            }

            Kill((object)gameObject);
            Kill(gameObject.transform);
            Kill(gameObject.GetComponent<SpriteRenderer>());
        }

        /// <summary>
        /// Performs the Kill operation.
        /// </summary>
        public static void Kill(Transform transform)
        {
            if (transform == null)
            {
                return;
            }

            Kill((object)transform);
            transform.DOKill(true);
        }

        /// <summary>
        /// Performs the Kill operation.
        /// </summary>
        public static void Kill(SpriteRenderer renderer)
        {
            if (renderer == null)
            {
                return;
            }

            Kill((object)renderer);
            renderer.DOKill(true);
        }

        /// <summary>
        /// Performs the Kill operation.
        /// </summary>
        public static void Kill(IEnumerable<GameObject> gameObjects)
        {
            if (gameObjects == null)
            {
                return;
            }

            foreach (GameObject gameObject in gameObjects)
            {
                Kill(gameObject);
            }
        }
    }
}
