using Presentation.Gameplay.Projectiles;
using UnityEngine;

namespace Infrastructure.Leveling
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

            GameObject existing = GameObject.Find(parentName);
            if (existing != null)
            {
                Object.Destroy(existing);
            }

            CleanupLooseKnives();
        }

        /// <summary>
        /// Performs the CleanupLooseKnives operation.
        /// </summary>
        private static void CleanupLooseKnives()
        {
            Knife[] allKnives = Object.FindObjectsByType<Knife>(FindObjectsSortMode.None);
            foreach (Knife knife in allKnives)
            {
                if (knife != null)
                {
                    Object.Destroy(knife.gameObject);
                }
            }
        }
    }
}
