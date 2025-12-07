using Core.Data;
using Core.Events;
using Presentation.Gameplay;
using UnityEngine;

namespace Infrastructure.Leveling
{
    /// <summary>
    /// Spawns hitable objects (e.g. tomatoes) around targets based on level data.
    /// </summary>
    public sealed class HitableObjectSpawner
    {
        private readonly ITargetScaleProvider targetProvider;
        private readonly HitableObjectHitEventChannel hitEventChannel;

        /// <summary>
        /// Performs the HitableObjectSpawner operation.
        /// </summary>
        public HitableObjectSpawner(ITargetScaleProvider targetProvider, HitableObjectHitEventChannel hitEventChannel)
        {
            this.targetProvider = targetProvider;
            this.hitEventChannel = hitEventChannel;
        }

        /// <summary>
        /// Performs the Spawn operation.
        /// </summary>
        public void Spawn(LevelData levelData)
        {
            if (levelData?.targetGroup?.targets == null || targetProvider == null)
            {
                return;
            }

            var targets = levelData.targetGroup.targets;
            if (targets.Count == 0 || targetProvider.AllTargets.Count == 0)
            {
                return;
            }

            int targetCount = Mathf.Min(targets.Count, targetProvider.AllTargets.Count);
            for (int i = 0; i < targetCount; i++)
            {
                SpawnForTarget(targets[i], targetProvider.AllTargets[i]);
            }
        }

        private void SpawnForTarget(TargetDefinition definition, Transform targetTransform)
        {
            if (definition?.hitableObjects == null || targetTransform == null || definition.hitableObjects.Count == 0)
            {
                return;
            }

            (Vector3 center, float radius) = TargetGeometry.GetCenterAndRadius(targetTransform);

            foreach (HitableObjectPlacement placement in definition.hitableObjects)
            {
                if (placement == null || placement.config?.prefab == null || placement.count <= 0)
                {
                    continue;
                }

                float spacing = CalculateSpacing(placement);
                float offset = definition.hitableObjectSpacingOffset;

                for (int i = 0; i < placement.count; i++)
                {
                    float angleDegrees = offset + i * spacing;
                    float angleRadians = angleDegrees * Mathf.Deg2Rad;

                    Vector3 position = center + new Vector3(Mathf.Cos(angleRadians), Mathf.Sin(angleRadians), 0f) * radius;
                    Quaternion rotation = placement.orientOutwards
                        ? Quaternion.Euler(0f, 0f, angleDegrees - 90f)
                        : Quaternion.identity;

                    GameObject instance = Object.Instantiate(placement.config.prefab, position, rotation, targetTransform);
                    instance.name = $"{placement.config.prefab.name}_Hitable_{i + 1}";

                    if (instance.TryGetComponent(out HitableObjectView view))
                    {
                        view.Configure(placement.config, hitEventChannel);
                    }
                }
            }
        }

        private static float CalculateSpacing(HitableObjectPlacement placement)
        {
            if (Mathf.Approximately(placement.angularSpacing, 0f))
            {
                return 360f / Mathf.Max(1, placement.count);
            }
            return placement.angularSpacing;
        }
    }
}

