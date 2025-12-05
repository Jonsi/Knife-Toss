using Core.Data;
using Presentation.Gameplay;
using Presentation.Gameplay.Projectiles;
using UnityEngine;

namespace Infrastructure.Leveling
{



    public sealed class PreStuckKnifeSpawner
    {
        private readonly ITargetScaleProvider targetProvider;
        private readonly GameObject knifePrefab;

        /// <summary>
        /// Performs the PreStuckKnifeSpawner operation.
        /// </summary>
        public PreStuckKnifeSpawner(ITargetScaleProvider targetProvider, GameObject prefab)
        {
            this.targetProvider = targetProvider;
            knifePrefab = prefab;
        }

        /// <summary>
        /// Performs the Spawn operation.
        /// </summary>
        public int Spawn(LevelData levelData)
        {
            if (levelData == null || levelData.targetGroup == null || targetProvider == null || knifePrefab == null)
            {
                return 0;
            }

            var targets = levelData.targetGroup.targets;
            if (targets == null || targets.Count == 0)
            {
                return 0;
            }

            int spawned = 0;
            for (int i = 0; i < Mathf.Min(targets.Count, targetProvider.AllTargets.Count); i++)
            {
                TargetDefinition targetDef = targets[i];
                Transform targetTransform = targetProvider.AllTargets[i];
                if (targetDef == null || targetTransform == null || targetDef.preStuckKnives <= 0)
                {
                    continue;
                }

                CircleCollider2D targetCollider = targetTransform.GetComponent<CircleCollider2D>();
                float uniformScale = targetTransform.lossyScale.x;
                float targetRadius = targetCollider != null ? targetCollider.radius * uniformScale : 0.5f;
                float angleStep = targetDef.preStuckKnifeSpacing;
                float startAngle = Random.Range(0f, 360f);
                float embedOffset = ResolveEmbedOffset() * Mathf.Max(0.01f, targetTransform.lossyScale.x);

                for (int k = 0; k < targetDef.preStuckKnives; k++)
                {
                    float angle = (startAngle + k * angleStep) * Mathf.Deg2Rad;
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
            }

            return spawned;
        }

        /// <summary>
        /// Performs the ResolveEmbedOffset operation.
        /// </summary>
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
