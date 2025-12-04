using KnifeToss.Core.Abstractions;
using KnifeToss.Presentation.Gameplay.Projectiles;
using UnityEngine;

namespace KnifeToss.Infrastructure.Leveling
{



    public sealed class KnivesHierarchyManager
    {
        private readonly Transform owner;
        private GameObject knivesParent;
        private readonly string parentName;

        /// <summary>
        /// Performs the KnivesHierarchyManager operation.
        /// </summary>
        public KnivesHierarchyManager(Transform ownerTransform, string defaultParentName = "Knives")
        {
            owner = ownerTransform;
            parentName = defaultParentName;
        }

        /// <summary>
        /// Performs the EnsureParent operation.
        /// </summary>
        public Transform EnsureParent()
        {
            if (knivesParent == null)
            {
                knivesParent = new GameObject(parentName);
                if (owner != null)
                {
                    knivesParent.transform.SetParent(owner);
                }
            }

            return knivesParent.transform;
        }

        /// <summary>
        /// Performs the ResetHierarchy operation.
        /// </summary>
        public void ResetHierarchy()
        {
            if (knivesParent != null)
            {
                Object.Destroy(knivesParent);
                knivesParent = null;
            }

            // Note: GameObject.Find removed - we track our own parent reference
            // If cleanup is needed, use a registry service instead
        }

        /// <summary>
        /// Cleans up loose knives using a provided registry.
        /// </summary>
        public void CleanupLooseKnives(IKnifeTracker tracker)
        {
            if (tracker == null)
            {
                return;
            }

            // Use tracker's snapshot instead of FindObjectsByType
            var knives = tracker.Snapshot();
            foreach (var knife in knives)
            {
                if (knife != null && knife.gameObject != null)
                {
                    Object.Destroy(knife.gameObject);
                }
            }
        }
    }
}
