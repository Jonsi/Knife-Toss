using KnifeToss.Presentation.Gameplay.Projectiles;
using UnityEngine;

namespace KnifeToss.Infrastructure.Leveling
{



    public sealed class KnivesHierarchyManager
    {
        private readonly Transform owner;
        private GameObject knivesParent;
        private readonly string parentName;

        public KnivesHierarchyManager(Transform ownerTransform, string defaultParentName = "Knives")
        {
            owner = ownerTransform;
            parentName = defaultParentName;
        }

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
