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
                Vector3 circleCenter = targetTransform.position;
                float uniformScale = Mathf.Max(targetTransform.lossyScale.x, targetTransform.lossyScale.y);
                float targetRadius = 0.5f * uniformScale;
                if (targetCollider != null)
                {
                    circleCenter = targetCollider.transform.TransformPoint(targetCollider.offset);
                    uniformScale = Mathf.Max(targetCollider.transform.lossyScale.x, targetCollider.transform.lossyScale.y);
                    targetRadius = targetCollider.radius * uniformScale;
                }

                float angleStep = targetDef.preStuckKnifeSpacing;
                float startAngle = Random.Range(0f, 360f);

                for (int k = 0; k < targetDef.preStuckKnives; k++)
                {
                    float angle = (startAngle + k * angleStep) * Mathf.Deg2Rad;
                    Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                    Vector3 surfacePoint = circleCenter + (Vector3)(direction * targetRadius);
                    Vector2 directionToCenter = -direction;
                    float knifeAngle = Mathf.Atan2(directionToCenter.y, directionToCenter.x) * Mathf.Rad2Deg - 90f;
                    Quaternion rotation = Quaternion.Euler(0, 0, knifeAngle);

                    GameObject knife = Object.Instantiate(knifePrefab, surfacePoint, rotation);
                    if (knife.TryGetComponent(out Knife knifeComponent))
                    {
                        knifeComponent.SnapToSurface(targetTransform, circleCenter, surfacePoint);
                        knifeComponent.MarkAsPreStuck();
                    }
                    else
                    {
                        knife.transform.SetParent(targetTransform, true);
                    }

                    spawned++;
                }
            }

            return spawned;
        }
    }
}
