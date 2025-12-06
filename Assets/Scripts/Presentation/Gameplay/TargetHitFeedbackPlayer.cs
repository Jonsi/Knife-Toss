using System;
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

        public TargetHitFeedbackPlayer(
            KnifeImpactEventChannel impactEventChannel,
            GameplayObjectProvider gameplayObjectProvider,
            TargetHitAnimationConfig animationConfig)
        {
            this.impactEventChannel = impactEventChannel;
            this.gameplayObjectProvider = gameplayObjectProvider;
            this.animationConfig = animationConfig;
        }

        public void Start()
        {
            impactEventChannel.KnifeStuckAt += HandleKnifeStuckAt;
        }

        public void Dispose()
        {
            impactEventChannel.KnifeStuckAt -= HandleKnifeStuckAt;
        }

        private void HandleKnifeStuckAt(Vector3 impactPosition)
        {
            TargetGroupManager manager = gameplayObjectProvider.GetTargetGroupManager();
            if (manager == null)
            {
                return;
            }

            Transform target = FindImpactedTarget(manager.AllTargets, impactPosition) ?? manager.PrimaryTarget;
            if (target == null)
            {
                return;
            }

            PlayShake(target);
            ApplyTint(target);
        }

        private static Transform FindImpactedTarget(System.Collections.Generic.IReadOnlyList<Transform> candidates, Vector3 impactPosition)
        {
            if (candidates == null || candidates.Count == 0)
            {
                return null;
            }

            Transform closest = null;
            float closestDistance = float.MaxValue;

            for (int i = 0; i < candidates.Count; i++)
            {
                Transform candidate = candidates[i];
                if (candidate == null)
                {
                    continue;
                }

                float distance = (candidate.position - impactPosition).sqrMagnitude;
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = candidate;
                }
            }

            return closest;
        }

        private void PlayShake(Transform target)
        {
            if (target == null || animationConfig.ShakeDuration <= 0f || animationConfig.ShakeStrength <= 0f)
            {
                return;
            }

            SpriteRenderer[] renderers = target.GetComponentsInChildren<SpriteRenderer>();
            if (renderers != null && renderers.Length > 0)
            {
                Vector3 shakeStrength = new Vector3(animationConfig.ShakeStrength, animationConfig.ShakeStrength, 0f);
                for (int i = 0; i < renderers.Length; i++)
                {
                    SpriteRenderer renderer = renderers[i];
                    if (renderer != null && renderer.transform != null)
                    {
                        TweenKiller.Kill(renderer.transform);
                        renderer.transform
                            .DOShakePosition(
                                animationConfig.ShakeDuration,
                                shakeStrength,
                                animationConfig.ShakeVibrato,
                                animationConfig.ShakeRandomness,
                                snapping: false,
                                fadeOut: animationConfig.ShakeFadeOut)
                            .SetRelative();
                    }
                }
            }
        }

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

            SpriteRenderer[] renderers = target.GetComponentsInChildren<SpriteRenderer>();
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

                Color baseColor = renderer.color;
                TweenKiller.Kill(renderer);

                Sequence sequence = DOTween.Sequence().SetTarget(renderer).SetAutoKill(true);

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
                else if (fadeIn > 0f || hold > 0f)
                {
                    sequence.AppendCallback(() => { if (renderer != null) renderer.color = baseColor; });
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
    }
}
