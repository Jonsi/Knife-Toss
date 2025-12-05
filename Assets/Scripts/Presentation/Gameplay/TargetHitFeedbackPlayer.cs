using System;
using System.Collections.Generic;
using Core.Events;
using DG.Tweening;
using UnityEngine;
using VContainer.Unity;

namespace Presentation.Gameplay
{
    /// <summary>
    /// Plays shake/tint feedback on the active target whenever a knife sticks into it.
    /// </summary>
    public sealed class TargetHitFeedbackPlayer : IStartable, IDisposable
    {
        private readonly KnifeImpactEventChannel impactEventChannel;
        private readonly GameplayObjectProvider gameplayObjectProvider;
        private readonly TargetHitAnimationConfig animationConfig;
        private readonly Dictionary<SpriteRenderer, Color> baseColors = new Dictionary<SpriteRenderer, Color>();

        /// <summary>
        /// Performs the TargetHitFeedbackPlayer operation.
        /// </summary>
        public TargetHitFeedbackPlayer(
            KnifeImpactEventChannel impactEventChannel,
            GameplayObjectProvider gameplayObjectProvider,
            TargetHitAnimationConfig animationConfig)
        {
            this.impactEventChannel = impactEventChannel;
            this.gameplayObjectProvider = gameplayObjectProvider;
            this.animationConfig = animationConfig;
        }

        /// <summary>
        /// Performs the Start operation.
        /// </summary>
        public void Start()
        {
            if (impactEventChannel == null)
            {
                return;
            }

            impactEventChannel.KnifeStuckAt += HandleKnifeStuckAt;
        }

        /// <summary>
        /// Performs the Dispose operation.
        /// </summary>
        public void Dispose()
        {
            if (impactEventChannel == null)
            {
                return;
            }

            impactEventChannel.KnifeStuckAt -= HandleKnifeStuckAt;
        }

        /// <summary>
        /// Performs the HandleKnifeStuckAt operation.
        /// </summary>
        private void HandleKnifeStuckAt(Vector3 impactPosition)
        {
            if (animationConfig == null || gameplayObjectProvider == null)
            {
                return;
            }

            TargetGroupManager manager = gameplayObjectProvider.GetTargetGroupManager();
            Transform target = FindImpactedTarget(manager?.AllTargets, impactPosition) ?? manager?.PrimaryTarget;
            if (target == null)
            {
                return;
            }

            PlayShake(target);
            ApplyTint(target);
        }

        /// <summary>
        /// Performs the FindImpactedTarget operation.
        /// </summary>
        private static Transform FindImpactedTarget(IReadOnlyList<Transform> candidates, Vector3 impactPosition)
        {
            if (candidates == null || candidates.Count == 0)
            {
                return null;
            }

            Transform closest = null;
            float bestScore = float.MaxValue;

            for (int i = 0; i < candidates.Count; i++)
            {
                Transform candidate = candidates[i];
                if (candidate == null)
                {
                    continue;
                }

                float distance = (candidate.position - impactPosition).sqrMagnitude;
                if (distance < bestScore)
                {
                    bestScore = distance;
                    closest = candidate;
                }
            }

            return closest;
        }

        /// <summary>
        /// Performs the PlayShake operation.
        /// </summary>
        private void PlayShake(Transform target)
        {
            float duration = animationConfig.ShakeDuration;
            float strength = animationConfig.ShakeStrength;
            if (target == null || duration <= 0f || strength <= 0f)
            {
                return;
            }

            Vector3 shakeStrength = new Vector3(strength, strength, 0f);
            if (TryShakeRenderers(target, duration, shakeStrength))
            {
                return;
            }

            TweenKiller.Kill(target);
            target.DOShakePosition(
                duration,
                shakeStrength,
                animationConfig.ShakeVibrato,
                animationConfig.ShakeRandomness,
                snapping: false,
                fadeOut: animationConfig.ShakeFadeOut)
                .SetRelative();
        }

        /// <summary>
        /// Attempts to shake the sprite renderers for a target to avoid disrupting orbit transforms.
        /// </summary>
        private bool TryShakeRenderers(Transform target, float duration, Vector3 shakeStrength)
        {
            SpriteRenderer[] renderers = target.GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
            if (renderers == null || renderers.Length == 0)
            {
                return false;
            }

            bool shookAny = false;

            for (int i = 0; i < renderers.Length; i++)
            {
                SpriteRenderer renderer = renderers[i];
                Transform rendererTransform = renderer != null ? renderer.transform : null;
                if (rendererTransform == null)
                {
                    continue;
                }

                shookAny = true;
                TweenKiller.Kill(rendererTransform);
                rendererTransform.DOShakePosition(
                        duration,
                        shakeStrength,
                        animationConfig.ShakeVibrato,
                        animationConfig.ShakeRandomness,
                        snapping: false,
                        fadeOut: animationConfig.ShakeFadeOut)
                    .SetRelative()
                    .SetTarget(rendererTransform);
            }

            return shookAny;
        }

        /// <summary>
        /// Performs the ApplyTint operation.
        /// </summary>
        private void ApplyTint(Transform target)
        {
            if (target == null)
            {
                return;
            }

            float fadeIn = animationConfig.TintFadeInDuration;
            float hold = animationConfig.TintHoldDuration;
            float fadeOut = animationConfig.TintFadeOutDuration;

            if (fadeIn <= 0f && hold <= 0f && fadeOut <= 0f)
            {
                return;
            }

            SpriteRenderer[] renderers = target.GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
            if (renderers == null || renderers.Length == 0)
            {
                return;
            }

            Color targetColor = animationConfig.TintColor;
            Ease tintEase = animationConfig.TintEase;

            for (int i = 0; i < renderers.Length; i++)
            {
                SpriteRenderer renderer = renderers[i];
                if (renderer == null)
                {
                    continue;
                }

                Color baseColor = ResolveBaseColor(renderer);
                TweenKiller.Kill(renderer);

                Sequence sequence = DG.Tweening.DOTween.Sequence().SetTarget(renderer);

                if (fadeIn > 0f)
                {
                    sequence.Append(renderer.DOColor(targetColor, fadeIn).SetEase(tintEase));
                }
                else
                {
                    renderer.color = targetColor;
                }

                if (hold > 0f)
                {
                    sequence.AppendInterval(hold);
                }

                if (fadeOut > 0f)
                {
                    sequence.Append(renderer.DOColor(baseColor, fadeOut).SetEase(tintEase));
                }
                else
                {
                    renderer.color = baseColor;
                }

                sequence.OnKill(() =>
                {
                    if (renderer != null)
                    {
                        renderer.color = baseColor;
                    }
                });
            }
        }

        /// <summary>
        /// Performs the ResolveBaseColor operation.
        /// </summary>
        private Color ResolveBaseColor(SpriteRenderer renderer)
        {
            if (renderer == null)
            {
                return Color.white;
            }

            if (baseColors.TryGetValue(renderer, out Color cachedColor))
            {
                return cachedColor;
            }

            Color color = renderer.color;
            baseColors[renderer] = color;
            return color;
        }
    }
}
