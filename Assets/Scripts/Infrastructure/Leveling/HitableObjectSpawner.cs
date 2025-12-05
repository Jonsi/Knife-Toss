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
        private readonly SoundSettings soundSettings;
        private readonly HitableObjectHitEventChannel hitEventChannel;

        /// <summary>
        /// Performs the HitableObjectSpawner operation.
        /// </summary>
        public HitableObjectSpawner(ITargetScaleProvider targetProvider, SoundSettings soundSettings, HitableObjectHitEventChannel hitEventChannel)
        {
            this.targetProvider = targetProvider;
            this.soundSettings = soundSettings;
            this.hitEventChannel = hitEventChannel;
        }

        /// <summary>
        /// Performs the Spawn operation.
        /// </summary>
        public void Spawn(LevelData levelData)
        {
            if (levelData == null || levelData.targetGroup == null || targetProvider == null)
            {
                return;
            }

            var targets = levelData.targetGroup.targets;
            if (targets == null || targets.Count == 0 || targetProvider.AllTargets.Count == 0)
            {
                return;
            }

            int count = Mathf.Min(targets.Count, targetProvider.AllTargets.Count);
            for (int i = 0; i < count; i++)
            {
                SpawnForTarget(targets[i], targetProvider.AllTargets[i]);
            }
        }

        /// <summary>
        /// Performs the SpawnForTarget operation.
        /// </summary>
        private void SpawnForTarget(TargetDefinition definition, Transform targetTransform)
        {
            if (definition == null || targetTransform == null || definition.hitableObjects == null || definition.hitableObjects.Count == 0)
            {
                return;
            }

            CircleCollider2D targetCollider = targetTransform.GetComponent<CircleCollider2D>();
            float uniformScale = Mathf.Max(targetTransform.lossyScale.x, targetTransform.lossyScale.y);
            float targetRadius = targetCollider != null ? targetCollider.radius * uniformScale : 0.5f;

            for (int j = 0; j < definition.hitableObjects.Count; j++)
            {
                HitableObjectPlacement hitable = definition.hitableObjects[j];
                if (hitable == null || hitable.config == null || hitable.config.prefab == null || hitable.count <= 0)
                {
                    continue;
                }

                float spacing = Mathf.Approximately(hitable.angularSpacing, 0f)
                    ? 360f / Mathf.Max(1, hitable.count)
                    : hitable.angularSpacing;

                float radius = Mathf.Max(0f, targetRadius + hitable.radialOffset);
                float startAngle = Random.Range(0f, 360f);

                for (int k = 0; k < hitable.count; k++)
                {
                    float angle = startAngle + k * spacing;

                    float rad = angle * Mathf.Deg2Rad;
                    Vector3 position = targetTransform.position + new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * radius;

                    Quaternion rotation = hitable.orientOutwards
                        ? Quaternion.Euler(0f, 0f, angle - 90f)
                        : Quaternion.identity;

                    GameObject instance = Object.Instantiate(hitable.config.prefab, position, rotation, targetTransform);
                    instance.name = $"{hitable.config.prefab.name}_Hitable_{k + 1}";

                    if (instance.TryGetComponent(out HitableObjectView view))
                    {
                        view.Configure(hitable.config, hitEventChannel);
                    }
                }
            }
        }
    }
}
