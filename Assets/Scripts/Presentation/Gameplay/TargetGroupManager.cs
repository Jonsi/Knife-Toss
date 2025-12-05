using System.Collections.Generic;
using Core.Data;
using Presentation.Gameplay.Rotation;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Presentation.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class TargetGroupManager : MonoBehaviour, ITargetScaleProvider
    {
        [SerializeField] private Transform pivot;
        [SerializeField] private Vector3 pivotPosition = Vector3.zero;

        private readonly List<TargetEntry> entries = new List<TargetEntry>();
        private readonly List<Transform> targetTransforms = new List<Transform>();

        private IObjectResolver objectResolver;
        private GameObject fallbackPrefab;
        private RotationBehaviourFactory rotationFactory = new RotationBehaviourFactory();

        public Transform PrimaryTarget => entries.Count > 0 ? entries[0].Transform : null;
        public float PrimaryScale => entries.Count > 0 ? entries[0].CurrentScale : 1f;
        public IReadOnlyList<Transform> AllTargets => targetTransforms;

        /// <summary>
        /// Performs the Initialize operation.
        /// </summary>
        public void Initialize(GameObject defaultTargetPrefab, IObjectResolver resolver)
        {
            fallbackPrefab = defaultTargetPrefab;
            objectResolver = resolver;
            EnsurePivot();
        }

        /// <summary>
        /// Performs the Configure operation.
        /// </summary>
        public void Configure(LevelData levelData, int activeTarget)
        {
            EnsurePivot();
            ClearEntries();

            IReadOnlyList<TargetDefinition> definitions = ResolveDefinitions(levelData);

            for (int i = 0; i < definitions.Count; i++)
            {
                TargetDefinition definition = definitions[i];
                if (definition == null)
                {
                    continue;
                }

                CreateTarget(definition, i);
            }
        }

        /// <summary>
        /// Performs the SetActiveTarget operation.
        /// </summary>
        public void SetActiveTarget(int index)
        {
        }

        /// <summary>
        /// Performs the FreezeAll operation.
        /// </summary>
        public void FreezeAll()
        {
            for (int i = 0; i < entries.Count; i++)
            {
                entries[i].Target?.Freeze();
            }
        }

        /// <summary>
        /// Performs the ResumeAll operation.
        /// </summary>
        public void ResumeAll()
        {
            for (int i = 0; i < entries.Count; i++)
            {
                entries[i].Target?.Resume();
            }
        }

        /// <summary>
        /// Performs the Update operation.
        /// </summary>
        private void Update()
        {
            float delta = Time.deltaTime;

            for (int i = 0; i < entries.Count; i++)
            {
                TargetEntry entry = entries[i];
                if (entry.Transform == null)
                {
                    continue;
                }

                RefreshRotationBehaviourIfNeeded(ref entry);

                entry.Angle += entry.Definition.orbitDegreesPerSecond * delta;
                entry.Transform.localPosition = ResolveOrbitPosition(entry.Definition.orbitRadius, entry.Angle);

                float scale = ResolveScale(entry.Definition.orbitRadius, entry.Definition);
                entry.CurrentScale = scale;
                entry.Transform.localScale = Vector3.one * scale;

                entries[i] = entry;
            }
        }

        /// <summary>
        /// Performs the CreateTarget operation.
        /// </summary>
        private void CreateTarget(TargetDefinition definition, int index)
        {
            GameObject prefab = definition.prefab != null ? definition.prefab : fallbackPrefab;
            if (prefab == null)
            {
                Debug.LogWarning("No target prefab available to create target.");
                return;
            }

            GameObject instance = Instantiate(prefab, pivot);
            instance.name = $"Target_{index + 1}";
            float orbitRadius = Mathf.Max(0f, definition.orbitRadius);
            instance.transform.localPosition = ResolveOrbitPosition(orbitRadius, definition.startingAngle);
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one * ResolveScale(orbitRadius, definition);

            objectResolver?.InjectGameObject(instance);

            RotatingTarget rotatingTarget = instance.GetComponent<RotatingTarget>();
            if (rotatingTarget == null)
            {
                rotatingTarget = instance.AddComponent<RotatingTarget>();
            }

            IRotationBehaviour behaviour = rotationFactory.Create(definition.rotation);
            rotatingTarget.SetRotationBehaviour(behaviour);
            rotatingTarget.Resume();
            rotatingTarget.ResetRotation();

            TargetEntry entry = new TargetEntry
            {
                Definition = definition,
                Transform = instance.transform,
                Target = rotatingTarget,
                RotationBehaviour = behaviour,
                RotationSnapshot = TargetRotationConfigSnapshot.From(definition.rotation),
                Angle = definition.startingAngle,
                CurrentScale = ResolveScale(orbitRadius, definition)
            };

            entries.Add(entry);
            targetTransforms.Add(instance.transform);
        }

        /// <summary>
        /// Performs the ResolveOrbitPosition operation.
        /// </summary>
        private Vector3 ResolveOrbitPosition(float radius, float angleDegrees)
        {
            float rad = angleDegrees * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * radius;
            return offset;
        }

        /// <summary>
        /// Performs the ResolveScale operation.
        /// </summary>
        private float ResolveScale(float orbitRadius, TargetDefinition definition)
        {
            if (definition == null)
            {
                return 1f;
            }

            return Mathf.Max(0.01f, definition.baseScale <= 0f ? 1f : definition.baseScale);
        }


        /// <summary>
        /// Performs the EnsurePivot operation.
        /// </summary>
        private void EnsurePivot()
        {
            if (pivot == null)
            {
                GameObject pivotObject = new GameObject("TargetPivot");
                pivot = pivotObject.transform;
                pivot.SetParent(transform, false);
            }

            pivot.localPosition = pivotPosition;
        }

        /// <summary>
        /// Performs the ClearEntries operation.
        /// </summary>
        private void ClearEntries()
        {
            for (int i = 0; i < entries.Count; i++)
            {
                TargetEntry entry = entries[i];
                if (entry.Transform != null)
                {
                    TweenKiller.Kill(entry.Transform.gameObject);
                    Destroy(entry.Transform.gameObject);
                }
            }

            entries.Clear();
            targetTransforms.Clear();
        }

        /// <summary>
        /// Performs the ResolveDefinitions operation.
        /// </summary>
        private static IReadOnlyList<TargetDefinition> ResolveDefinitions(LevelData data)
        {
            if (data != null && data.targetGroup != null && data.targetGroup.targets != null && data.targetGroup.targets.Count > 0)
            {
                return data.targetGroup.targets;
            }

            TargetDefinition fallback = new TargetDefinition
            {
                baseScale = 1f,
                orbitRadius = 0f,
                startingAngle = 0f,
                rotation = new TargetRotationConfig
                {
                    speed = 90f,
                    behaviourType = RotationBehaviourType.Constant
                }
            };

            return new List<TargetDefinition> { fallback };
        }

        /// <summary>
        /// Represents the TargetEntry struct.
        /// </summary>
        private struct TargetEntry
        {
            public TargetDefinition Definition;
            public Transform Transform;
            public RotatingTarget Target;
            public IRotationBehaviour RotationBehaviour;
            public TargetRotationConfigSnapshot RotationSnapshot;
            public float Angle;
            public float CurrentScale;
        }

        /// <summary>
        /// Performs the RefreshRotationBehaviourIfNeeded operation.
        /// </summary>
        private void RefreshRotationBehaviourIfNeeded(ref TargetEntry entry)
        {
            TargetRotationConfig config = entry.Definition.rotation;
            if (entry.Target == null || config == null)
            {
                return;
            }

            TargetRotationConfigSnapshot currentSnapshot = TargetRotationConfigSnapshot.From(config);
            if (currentSnapshot.Equals(entry.RotationSnapshot))
            {
                return;
            }

            entry.RotationBehaviour = rotationFactory.Create(config);
            entry.RotationSnapshot = currentSnapshot;
            entry.Target.SetRotationBehaviour(entry.RotationBehaviour);
        }

        private readonly struct TargetRotationConfigSnapshot
        {
            private readonly RotationBehaviourType type;
            private readonly float speed;
            private readonly float pulsateAmplitude;
            private readonly float pulsateFrequency;
            private readonly float flipInterval;
            private readonly float flipProbability;
            private readonly float curveLoopDuration;

            /// <summary>
            /// Performs the TargetRotationConfigSnapshot operation.
            /// </summary>
            private TargetRotationConfigSnapshot(
                RotationBehaviourType type,
                float speed,
                float pulsateAmplitude,
                float pulsateFrequency,
                float flipInterval,
                float flipProbability,
                float curveLoopDuration)
            {
                this.type = type;
                this.speed = speed;
                this.pulsateAmplitude = pulsateAmplitude;
                this.pulsateFrequency = pulsateFrequency;
                this.flipInterval = flipInterval;
                this.flipProbability = flipProbability;
                this.curveLoopDuration = curveLoopDuration;
            }

            /// <summary>
            /// Performs the From operation.
            /// </summary>
            public static TargetRotationConfigSnapshot From(TargetRotationConfig config)
            {
                if (config == null)
                {
                    return default;
                }

                return new TargetRotationConfigSnapshot(
                    config.behaviourType,
                    config.speed,
                    config.pulsating != null ? config.pulsating.amplitude : 0f,
                    config.pulsating != null ? config.pulsating.frequency : 0f,
                    config.flipping != null ? config.flipping.flipInterval : 0f,
                    config.flipping != null ? config.flipping.flipProbability : 0f,
                    config.acceleration != null ? config.acceleration.curveLoopDuration : 0f);
            }

            /// <summary>
            /// Performs the Equals operation.
            /// </summary>
            public bool Equals(TargetRotationConfigSnapshot other)
            {
                return type == other.type
                    && Mathf.Approximately(speed, other.speed)
                    && Mathf.Approximately(pulsateAmplitude, other.pulsateAmplitude)
                    && Mathf.Approximately(pulsateFrequency, other.pulsateFrequency)
                    && Mathf.Approximately(flipInterval, other.flipInterval)
                    && Mathf.Approximately(flipProbability, other.flipProbability)
                    && Mathf.Approximately(curveLoopDuration, other.curveLoopDuration);
            }
        }
    }
}
