using KnifeToss.Presentation.Gameplay.Projectiles;
using UnityEngine;

namespace KnifeToss.Infrastructure.Leveling
{
    /// <summary>
    /// Responsible for creating and cleaning up the runtime hierarchy that hosts knives.
    /// </summary>
    public sealed class KnivesHierarchyManager
    {
        private readonly Transform owner;
        private readonly Transform targetTransform;
        private GameObject knivesParent;
        private readonly string parentName;

        public KnivesHierarchyManager(Transform ownerTransform, Transform target, string defaultParentName = "Knives")
        {
            owner = ownerTransform;
            targetTransform = target;
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

            CleanupTargetChildren();
            CleanupLooseKnives();
        }

        private void CleanupTargetChildren()
        {
            if (targetTransform == null)
                return;

            for (int i = targetTransform.childCount - 1; i >= 0; i--)
            {
                Transform child = targetTransform.GetChild(i);
                if (child == null) continue;

                if (child.TryGetComponent(out Knife _))
                {
                    Object.Destroy(child.gameObject);
                }
            }
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
