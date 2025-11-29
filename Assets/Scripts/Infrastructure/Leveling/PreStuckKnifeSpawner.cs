using KnifeToss.Core.Domain;
using KnifeToss.Presentation.Gameplay.Projectiles;
using UnityEngine;

namespace KnifeToss.Infrastructure.Leveling
{
    /// <summary>
    /// Handles pre-populating the target with knives according to level rules.
    /// </summary>
    public sealed class PreStuckKnifeSpawner
    {
        private readonly Transform targetTransform;
        private readonly GameObject knifePrefab;

        public PreStuckKnifeSpawner(Transform target, GameObject prefab)
        {
            targetTransform = target;
            knifePrefab = prefab;
        }

        public int Spawn(LevelRules rules)
        {
            if (rules.PreStuckKnives <= 0 || targetTransform == null || knifePrefab == null)
            {
                return 0;
            }

            CircleCollider2D targetCollider = targetTransform.GetComponent<CircleCollider2D>();
            float targetRadius = targetCollider != null ? targetCollider.radius : 0.5f;
            float angleStep = rules.PreStuckKnifeSpacing;
            float startAngle = Random.Range(0f, 360f);
            float embedOffset = ResolveEmbedOffset();

            int spawned = 0;
            for (int i = 0; i < rules.PreStuckKnives; i++)
            {
                float angle = (startAngle + i * angleStep) * Mathf.Deg2Rad;
                Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                Vector2 position = (Vector2)targetTransform.position + direction * (targetRadius + embedOffset);
                Vector2 directionToCenter = ((Vector2)targetTransform.position - position).normalized;
                float knifeAngle = Mathf.Atan2(directionToCenter.y, directionToCenter.x) * Mathf.Rad2Deg - 90f;
                Quaternion rotation = Quaternion.Euler(0, 0, knifeAngle);

                GameObject knife = Object.Instantiate(knifePrefab, position, rotation);
                knife.transform.SetParent(targetTransform);
                if (knife.TryGetComponent(out Knife knifeComponent))
                {
                    knifeComponent.MarkAsPreStuck();
                }

                spawned++;
            }

            return spawned;
        }

        private float ResolveEmbedOffset()
        {
            if (knifePrefab != null && knifePrefab.TryGetComponent(out Knife knife))
            {
                return knife.EmbedOffset;
            }

            return 0.02f;
        }
    }
}
