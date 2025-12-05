using UnityEngine;
using UnityEngine.UI;

namespace Presentation.UI
{
    /// <summary>
    /// Provides shared logic for updating star UI elements based on a star count.
    /// </summary>
    public static class StarDisplay
    {
        /// <summary>
        /// Applies active/inactive visuals to the provided star objects.
        /// </summary>
        public static void SetStars(GameObject[] stars, int count, Color activeColor, Color inactiveColor)
        {
            if (stars == null)
            {
                return;
            }

            int clampedCount = Mathf.Max(0, count);
            for (var i = 0; i < stars.Length; i++)
            {
                GameObject star = stars[i];
                if (star == null)
                {
                    continue;
                }

                bool isActiveStar = i < clampedCount;

                var graphic = star.GetComponent<Graphic>();
                if (graphic != null)
                {
                    if (!star.activeSelf)
                    {
                        star.SetActive(true);
                    }

                    graphic.enabled = true;
                    graphic.color = isActiveStar ? activeColor : inactiveColor;
                }
                else
                {
                    star.SetActive(isActiveStar);
                }
            }
        }
    }
}
