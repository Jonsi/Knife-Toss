using Core.Data;
using Presentation.Gameplay;
using Presentation.Gameplay.Projectiles;
using UnityEngine;

namespace Infrastructure.Leveling
{
    /// <summary>
    /// Spawns pre-stuck knives around targets based on level configuration.
    /// </summary>
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
            if (levelData?.targetGroup?.targets == null || targetProvider == null || knifePrefab == null)
            {
                return 0;
            }

            var targets = levelData.targetGroup.targets;
            if (targets.Count == 0 || targetProvider.AllTargets.Count == 0)
            {
                return 0;
            }

            int spawned = 0;
            int targetCount = Mathf.Min(targets.Count, targetProvider.AllTargets.Count);

            for (int i = 0; i < targetCount; i++)
            {
                TargetDefinition targetDef = targets[i];
                Transform targetTransform = targetProvider.AllTargets[i];

                if (targetDef == null || targetTransform == null || targetDef.preStuckKnives <= 0)
                {
                    continue;
                }

                spawned += SpawnForTarget(targetDef, targetTransform);
            }

            return spawned;
        }

        private int SpawnForTarget(TargetDefinition targetDef, Transform targetTransform)
        {
            (Vector3 center, float radius) = TargetGeometry.GetCenterAndRadius(targetTransform);

            float spacing = targetDef.preStuckKnifeSpacing;
            float offset = targetDef.preStuckKnifeSpacingOffset;

            int spawned = 0;
            for (int i = 0; i < targetDef.preStuckKnives; i++)
            {
                float angleDegrees = offset + i * spacing;
                float angleRadians = angleDegrees * Mathf.Deg2Rad;

                Vector2 direction = new Vector2(Mathf.Cos(angleRadians), Mathf.Sin(angleRadians));
                Vector3 position = center + (Vector3)(direction * radius);
                Vector2 toCenter = -direction;
                float rotationAngle = Mathf.Atan2(toCenter.y, toCenter.x) * Mathf.Rad2Deg - 90f;
                Quaternion rotation = Quaternion.Euler(0, 0, rotationAngle);

                GameObject knife = Object.Instantiate(knifePrefab, position, rotation);
                if (knife.TryGetComponent(out Knife knifeComponent))
                {
                    knifeComponent.SnapToSurface(targetTransform, center, position);
                    knifeComponent.MarkAsPreStuck();
                }
                else
                {
                    knife.transform.SetParent(targetTransform, true);
                }

                spawned++;
            }

            return spawned;
        }
    }
}
