using System.Collections.Generic;
using Core.Data;
using Core.Domain;
using Presentation.Gameplay.Projectiles;
using UnityEngine;

namespace Presentation.Gameplay
{
    /// <summary>
    /// Tracks knife hits per target and handles target destruction when knivesToBreak threshold is reached.
    /// </summary>
    public sealed class TargetHitTracker
    {
        private readonly Dictionary<Transform, TargetHitData> targetHits = new Dictionary<Transform, TargetHitData>();
        private readonly LevelSession session;
        private readonly GameplayObjectProvider gameplayObjectProvider;

        /// <summary>
        /// Performs the TargetHitTracker operation.
        /// </summary>
        public TargetHitTracker(LevelSession levelSession, GameplayObjectProvider provider)
        {
            session = levelSession;
            gameplayObjectProvider = provider;
        }

        /// <summary>
        /// Initializes tracking for all targets in the current level.
        /// </summary>
        public void InitializeTargets(LevelData levelData)
        {
            targetHits.Clear();

            if (levelData?.targetGroup?.targets == null)
            {
                return;
            }

            TargetGroupManager manager = gameplayObjectProvider?.GetTargetGroupManager();
            if (manager == null)
            {
                return;
            }

            IReadOnlyList<Transform> targets = manager.AllTargets;
            IReadOnlyList<TargetDefinition> definitions = levelData.targetGroup.targets;

            for (int i = 0; i < targets.Count && i < definitions.Count; i++)
            {
                Transform target = targets[i];
                TargetDefinition definition = definitions[i];
                if (target != null && definition != null)
                {
                    targetHits[target] = new TargetHitData
                    {
                        Definition = definition,
                        HitCount = 0
                    };
                }
            }
        }

        /// <summary>
        /// Records a knife hit on a target and checks if it should break.
        /// </summary>
        public bool RecordHit(Transform targetTransform)
        {
            if (targetTransform == null)
            {
                return false;
            }

            Transform target = FindTargetRoot(targetTransform);
            if (target == null || !targetHits.TryGetValue(target, out TargetHitData data))
            {
                return false;
            }

            data.HitCount++;
            targetHits[target] = data;

            int threshold = data.Definition.knivesToBreak;
            if (threshold > 0 && data.HitCount >= threshold)
            {
                BreakTarget(target);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Finds the root target transform from a potentially nested transform.
        /// </summary>
        private Transform FindTargetRoot(Transform start)
        {
            Transform current = start;
            while (current != null)
            {
                if (targetHits.ContainsKey(current))
                {
                    return current;
                }
                current = current.parent;
            }
            return null;
        }

        /// <summary>
        /// Breaks a target by destroying it and updating session state.
        /// </summary>
        private void BreakTarget(Transform target)
        {
            if (target == null || !targetHits.ContainsKey(target))
            {
                return;
            }

            targetHits.Remove(target);
            TargetGroupManager manager = gameplayObjectProvider?.GetTargetGroupManager();
            manager?.DestroyTarget(target);
            session?.MarkTargetCompleted();
            session?.MarkCompleteIfNeeded();
        }

        private struct TargetHitData
        {
            public TargetDefinition Definition;
            public int HitCount;
        }
    }
}

