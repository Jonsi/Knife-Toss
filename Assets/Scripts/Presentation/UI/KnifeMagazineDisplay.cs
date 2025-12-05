using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Presentation.UI
{



    /// <summary>
    /// Simple pooled UI list that mirrors the current knife magazine.
    /// </summary>
    public sealed class KnifeMagazineDisplay : MonoBehaviour
    {
        [SerializeField] private RectTransform iconContainer;
        [SerializeField] private Transform poolRoot;
        [SerializeField] private GameObject iconPrefab;
        [SerializeField] private int poolWarmupCount = 8;
        [SerializeField] private Color availableColor = Color.white;
        [SerializeField] private Color spentColor = new Color(1f, 1f, 1f, 0.25f);
        [SerializeField] private bool reverseOrder;

        private readonly Queue<Image> pooledIcons = new Queue<Image>();
        private readonly List<Image> activeIcons = new List<Image>();
        private int cachedTotal;
        private Transform resolvedPoolRoot;

        private void Awake()
        {
            ResolvePoolRoot();
            WarmPool(poolWarmupCount);
        }

        /// <summary>
        /// Rebuilds the list (if needed) and sets filled/empty states.
        /// </summary>
        public void Render(int totalKnives, int knivesThrown)
        {
            if (iconContainer == null || iconPrefab == null)
            {
                return;
            }

            ResolvePoolRoot();
            totalKnives = Mathf.Max(0, totalKnives);
            knivesThrown = Mathf.Clamp(knivesThrown, 0, totalKnives);

            if (totalKnives != cachedTotal)
            {
                RebuildIcons(totalKnives);
            }

            UpdateIconStates(totalKnives, knivesThrown);
        }

        /// <summary>
        /// Clears any active icons and returns them to the pool.
        /// </summary>
        public void ResetDisplay()
        {
            cachedTotal = 0;
            ResolvePoolRoot();
            ReturnActiveIcons();
            SetContainerVisible(false);
        }

        private void RebuildIcons(int targetCount)
        {
            ReturnActiveIcons();
            cachedTotal = targetCount;

            if (targetCount <= 0)
            {
                SetContainerVisible(false);
                return;
            }

            for (int i = 0; i < targetCount; i++)
            {
                Image icon = GetIcon();
                if (icon == null)
                {
                    continue;
                }

                activeIcons.Add(icon);
            }

            SetContainerVisible(true);
        }

        private void UpdateIconStates(int totalKnives, int knivesThrown)
        {
            if (activeIcons.Count == 0)
            {
                SetContainerVisible(false);
                return;
            }

            int remaining = Mathf.Clamp(totalKnives - knivesThrown, 0, totalKnives);
            for (int i = 0; i < activeIcons.Count; i++)
            {
                Image icon = activeIcons[i];
                if (icon == null)
                {
                    continue;
                }

                int slotIndex = reverseOrder ? activeIcons.Count - 1 - i : i;
                bool hasKnife = slotIndex < remaining;
                icon.color = hasKnife ? availableColor : spentColor;
            }

            SetContainerVisible(true);
        }

        private Image GetIcon()
        {
            if (pooledIcons.Count == 0)
            {
                WarmPool(1);
            }

            if (pooledIcons.Count == 0)
            {
                return null;
            }

            Image icon = pooledIcons.Dequeue();
            Transform iconTransform = icon.transform;
            iconTransform.SetParent(iconContainer, false);
            iconTransform.localScale = Vector3.one;
            icon.gameObject.SetActive(true);
            icon.color = availableColor;
            return icon;
        }

        private void ReturnActiveIcons()
        {
            Transform poolParent = ResolvePoolRoot();
            for (int i = 0; i < activeIcons.Count; i++)
            {
                Image icon = activeIcons[i];
                if (icon == null)
                {
                    continue;
                }

                icon.gameObject.SetActive(false);
                icon.transform.SetParent(poolParent, false);
                pooledIcons.Enqueue(icon);
            }

            activeIcons.Clear();
        }

        private void WarmPool(int count)
        {
            if (iconPrefab == null)
            {
                return;
            }

            Transform targetPoolParent = ResolvePoolRoot();
            while (pooledIcons.Count < count)
            {
                GameObject iconObject = Instantiate(iconPrefab, targetPoolParent);
                iconObject.SetActive(false);
                Image iconImage = iconObject.GetComponent<Image>() ?? iconObject.AddComponent<Image>();
                iconImage.color = availableColor;
                pooledIcons.Enqueue(iconImage);
            }
        }

        private void SetContainerVisible(bool visible)
        {
            if (iconContainer == null)
            {
                return;
            }

            iconContainer.gameObject.SetActive(visible);
        }

        private Transform ResolvePoolRoot()
        {
            if (resolvedPoolRoot == null)
            {
                resolvedPoolRoot = poolRoot != null ? poolRoot : transform;
            }

            return resolvedPoolRoot;
        }
    }
}
