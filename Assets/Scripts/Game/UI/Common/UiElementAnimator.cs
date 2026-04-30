using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.UI.Common
{
    /// <summary>
    /// Simple per-element UI animator with matching enter and exit settings.
    /// </summary>
    public sealed class UiElementAnimator : MonoBehaviour
    {
        [SerializeField] private RectTransform _RectTransform;
        [SerializeField] private Graphic _Graphic;
        [SerializeField] private bool _PlayInOnEnable;
        [SerializeField] private bool _UseUnscaledTime = true;
        [SerializeField] private bool _EnableGameObjectOnPlayIn = true;
        [SerializeField] private bool _DisableGameObjectOnPlayOut = true;
        [SerializeField] private IdleAnimationSettings _IdleAnimation = IdleAnimationSettings.Disabled();
        [SerializeField] private AnimationSettings _In = AnimationSettings.DefaultIn();
        [SerializeField] private AnimationSettings _Out = AnimationSettings.DefaultOut();

        private CanvasGroup _CanvasGroup;
        private Sequence _ActiveSequence;
        private Tween _IdleAnimationTween;
        private bool _HasCachedState;
        private Vector2 _BaseAnchoredPosition;
        private Vector3 _BaseScale;
        private Vector3 _BaseEulerAngles;
        private Color _BaseColor = Color.white;
        private float _BaseAlpha = 1f;
        private DirectChildIdleState[] _DirectChildIdleStates;

        private void Awake()
        {
            EnsureReferences();
            CacheBaseState();
        }

        private void OnEnable()
        {
            if (_PlayInOnEnable)
            {
                PlayIn();
            }
        }

        private void OnDisable()
        {
            KillSequence();
        }

        private void OnDestroy()
        {
            KillSequence();
        }

        public void PlayIn()
        {
            if (_EnableGameObjectOnPlayIn && !gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            StartInTween();
        }

        public async UniTask PlayOutAsync()
        {
            StartOutTween();

            float duration = GetTotalDuration(_Out);
            if (duration <= 0f)
            {
                if (_DisableGameObjectOnPlayOut)
                {
                    gameObject.SetActive(false);
                }

                return;
            }

            await UniTask.Delay(Mathf.CeilToInt(duration * 1000f), ignoreTimeScale: _UseUnscaledTime);

            if (_DisableGameObjectOnPlayOut)
            {
                gameObject.SetActive(false);
            }
        }

        public Tween CreateEditorPreviewTween(bool playIn)
        {
            return playIn ? StartInTween() : StartOutTween();
        }

        private Tween StartInTween()
        {
            EnsureReferences();
            CacheBaseState();
            KillSequence();
            ApplyState(_In);
            _ActiveSequence = CreateTweenToBaseState(_In);
            _ActiveSequence.OnComplete(StartIdleAnimationIfNeeded);
            return _ActiveSequence;
        }

        private Tween StartOutTween()
        {
            EnsureReferences();
            CacheBaseState();
            KillSequence();
            _ActiveSequence = CreateTweenToConfiguredState(_Out);
            return _ActiveSequence;
        }

        private Sequence CreateTweenToBaseState(AnimationSettings settings)
        {
            Sequence sequence = DOTween.Sequence();
            ConfigureSequence(sequence, settings);

            if (_RectTransform != null)
            {
                sequence.Join(_RectTransform.DOAnchorPos(_BaseAnchoredPosition, settings.Duration).SetEase(settings.Ease));
                sequence.Join(_RectTransform.DOScale(_BaseScale, settings.Duration).SetEase(settings.Ease));
                sequence.Join(_RectTransform.DOLocalRotate(_BaseEulerAngles, settings.Duration).SetEase(settings.Ease));
            }

            if (_Graphic != null)
            {
                sequence.Join(_Graphic.DOColor(_BaseColor, settings.Duration).SetEase(settings.Ease));
            }

            if (_CanvasGroup != null)
            {
                sequence.Join(_CanvasGroup.DOFade(_BaseAlpha, settings.Duration).SetEase(settings.Ease));
            }

            return sequence;
        }

        private Sequence CreateTweenToConfiguredState(AnimationSettings settings)
        {
            Sequence sequence = DOTween.Sequence();
            ConfigureSequence(sequence, settings);

            if (_RectTransform != null)
            {
                sequence.Join(_RectTransform.DOAnchorPos(_BaseAnchoredPosition + settings.PositionOffset, settings.Duration).SetEase(settings.Ease));
                sequence.Join(_RectTransform.DOScale(Vector3.Scale(_BaseScale, settings.ScaleMultiplier), settings.Duration).SetEase(settings.Ease));
                sequence.Join(_RectTransform.DOLocalRotate(_BaseEulerAngles + settings.RotationOffset, settings.Duration).SetEase(settings.Ease));
            }

            if (_Graphic != null)
            {
                sequence.Join(_Graphic.DOColor(settings.Color, settings.Duration).SetEase(settings.Ease));
            }

            if (_CanvasGroup != null)
            {
                sequence.Join(_CanvasGroup.DOFade(settings.Alpha, settings.Duration).SetEase(settings.Ease));
            }

            return sequence;
        }

        private void ConfigureSequence(Sequence sequence, AnimationSettings settings)
        {
            if (_UseUnscaledTime)
            {
                sequence.SetUpdate(true);
            }

            if (settings.Delay > 0f)
            {
                sequence.SetDelay(settings.Delay);
            }

            sequence.OnKill(() =>
            {
                if (_ActiveSequence == sequence)
                {
                    _ActiveSequence = null;
                }
            });
        }

        private void ApplyState(AnimationSettings settings)
        {
            if (_RectTransform != null)
            {
                _RectTransform.anchoredPosition = _BaseAnchoredPosition + settings.PositionOffset;
                _RectTransform.localScale = Vector3.Scale(_BaseScale, settings.ScaleMultiplier);
                _RectTransform.localEulerAngles = _BaseEulerAngles + settings.RotationOffset;
            }

            if (_Graphic != null)
            {
                _Graphic.color = settings.Color;
            }

            if (_CanvasGroup != null)
            {
                _CanvasGroup.alpha = settings.Alpha;
            }
        }

        private float GetTotalDuration(AnimationSettings settings)
        {
            return Mathf.Max(0f, settings.Delay) + Mathf.Max(0f, settings.Duration);
        }

        private void EnsureReferences()
        {
            if (_RectTransform == null)
            {
                _RectTransform = transform as RectTransform;
            }

            if (_Graphic == null)
            {
                _Graphic = GetComponent<Graphic>();
            }

            if (_CanvasGroup == null)
            {
                _CanvasGroup = GetComponent<CanvasGroup>();
                if (_CanvasGroup == null)
                {
                    _CanvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }
        }

        private void CacheBaseState()
        {
            if (_HasCachedState || _RectTransform == null)
            {
                return;
            }

            _BaseAnchoredPosition = _RectTransform.anchoredPosition;
            _BaseScale = _RectTransform.localScale;
            _BaseEulerAngles = _RectTransform.localEulerAngles;

            if (_Graphic != null)
            {
                _BaseColor = _Graphic.color;
            }

            if (_CanvasGroup != null)
            {
                _BaseAlpha = _CanvasGroup.alpha;
            }

            _HasCachedState = true;
        }

        private void KillSequence()
        {
            KillIdleAnimation();

            if (_ActiveSequence != null)
            {
                _ActiveSequence.Kill();
                _ActiveSequence = null;
            }

            if (_RectTransform != null)
            {
                DOTween.Kill(_RectTransform);
            }

            if (_Graphic != null)
            {
                DOTween.Kill(_Graphic);
            }

            if (_CanvasGroup != null)
            {
                DOTween.Kill(_CanvasGroup);
            }
        }

        private void StartIdleAnimationIfNeeded()
        {
            KillIdleAnimation();

            if (_IdleAnimation.Mode == IdleAnimationMode.None || _RectTransform == null)
            {
                return;
            }

            switch (_IdleAnimation.Mode)
            {
                case IdleAnimationMode.Heartbeat:
                    _IdleAnimationTween = CreateHeartbeatIdleTween();
                    break;
                case IdleAnimationMode.HorizontalSway:
                    _IdleAnimationTween = CreateHorizontalSwayIdleTween();
                    break;
            }
        }

        private Tween CreateHeartbeatIdleTween()
        {
            Sequence sequence = DOTween.Sequence();

            if (_UseUnscaledTime)
            {
                sequence.SetUpdate(true);
            }

            sequence.AppendInterval(Mathf.Max(0f, _IdleAnimation.InitialDelay));
            sequence.Append(_RectTransform.DOScale(_BaseScale * _IdleAnimation.PrimaryScaleMultiplier, _IdleAnimation.UpDuration).SetEase(_IdleAnimation.UpEase));
            sequence.Append(_RectTransform.DOScale(_BaseScale, _IdleAnimation.DownDuration).SetEase(_IdleAnimation.DownEase));
            sequence.Append(_RectTransform.DOScale(_BaseScale * _IdleAnimation.SecondaryScaleMultiplier, _IdleAnimation.SecondaryUpDuration).SetEase(_IdleAnimation.UpEase));
            sequence.Append(_RectTransform.DOScale(_BaseScale, _IdleAnimation.SecondaryDownDuration).SetEase(_IdleAnimation.DownEase));
            sequence.AppendInterval(Mathf.Max(0f, _IdleAnimation.BetweenLoopsDelay));
            sequence.SetLoops(-1, LoopType.Restart);
            sequence.OnKill(() =>
            {
                if (_RectTransform != null)
                {
                    _RectTransform.localScale = _BaseScale;
                }

                if (_IdleAnimationTween == sequence)
                {
                    _IdleAnimationTween = null;
                }
            });

            return sequence;
        }

        private Tween CreateHorizontalSwayIdleTween()
        {
            if (_IdleAnimation.AnimateChildrenSeparately && TryCacheDirectChildIdleStates())
            {
                return CreateChildHorizontalSwayIdleTween();
            }

            var phase = 0f;
            Tweener tween = DOTween.To(
                () => phase,
                value =>
                {
                    phase = value;

                    if (_RectTransform == null)
                    {
                        return;
                    }

                    float offset = Mathf.Sin(phase) * _IdleAnimation.HorizontalTravel;
                    float tilt = Mathf.Sin(phase) * _IdleAnimation.HorizontalTilt;
                    _RectTransform.anchoredPosition = _BaseAnchoredPosition + Vector2.right * offset;
                    _RectTransform.localEulerAngles = _BaseEulerAngles + Vector3.forward * tilt;
                },
                Mathf.PI * 2f,
                Mathf.Max(0.01f, _IdleAnimation.HorizontalCycleDuration));

            tween.SetEase(Ease.Linear);
            tween.SetLoops(-1, LoopType.Restart);

            if (_UseUnscaledTime)
            {
                tween.SetUpdate(true);
            }

            if (_IdleAnimation.InitialDelay > 0f)
            {
                tween.SetDelay(_IdleAnimation.InitialDelay);
            }

            tween.OnKill(() =>
            {
                if (_RectTransform != null)
                {
                    _RectTransform.anchoredPosition = _BaseAnchoredPosition;
                    _RectTransform.localEulerAngles = _BaseEulerAngles;
                }

                if (_IdleAnimationTween == tween)
                {
                    _IdleAnimationTween = null;
                }
            });

            return tween;
        }

        private Tween CreateChildHorizontalSwayIdleTween()
        {
            if (_RectTransform != null)
            {
                _RectTransform.anchoredPosition = _BaseAnchoredPosition;
                _RectTransform.localEulerAngles = _BaseEulerAngles;
            }

            var phase = 0f;
            float cycleDuration = Mathf.Max(0.01f, _IdleAnimation.HorizontalCycleDuration);
            Tweener tween = DOTween.To(
                () => phase,
                value =>
                {
                    phase = value;
                    UpdateChildHorizontalSway(phase);
                },
                Mathf.PI * 2000f,
                cycleDuration * 1000f);

            tween.SetEase(Ease.Linear);
            tween.SetLoops(-1, LoopType.Restart);

            if (_UseUnscaledTime)
            {
                tween.SetUpdate(true);
            }

            if (_IdleAnimation.InitialDelay > 0f)
            {
                tween.SetDelay(_IdleAnimation.InitialDelay);
            }

            tween.OnKill(() =>
            {
                RestoreChildIdleStates();

                if (_IdleAnimationTween == tween)
                {
                    _IdleAnimationTween = null;
                }
            });

            return tween;
        }

        private void UpdateChildHorizontalSway(float phase)
        {
            if (_DirectChildIdleStates == null)
            {
                return;
            }

            for (var i = 0; i < _DirectChildIdleStates.Length; i++)
            {
                DirectChildIdleState childState = _DirectChildIdleStates[i];
                if (childState.RectTransform == null)
                {
                    continue;
                }

                float childCycleMultiplier = 1f + _IdleAnimation.ChildCycleDurationVariance * i;
                float basePhase = _IdleAnimation.ChildPhaseOffset * i;
                float childPhase = phase / Mathf.Max(0.1f, childCycleMultiplier) + basePhase;
                float travel = _IdleAnimation.HorizontalTravel * Mathf.Max(0f, 1f - _IdleAnimation.ChildTravelFalloff * i);
                float offset = (Mathf.Sin(childPhase) - Mathf.Sin(basePhase)) * travel;
                float verticalOffset = (Mathf.Cos(childPhase * 0.5f) - Mathf.Cos(basePhase * 0.5f)) * _IdleAnimation.ChildVerticalTravel;
                float tilt = (Mathf.Sin(childPhase + Mathf.PI * 0.35f) - Mathf.Sin(basePhase + Mathf.PI * 0.35f)) * _IdleAnimation.HorizontalTilt;

                childState.RectTransform.anchoredPosition = childState.AnchoredPosition + new Vector2(offset, verticalOffset);
                childState.RectTransform.localEulerAngles = childState.EulerAngles + Vector3.forward * tilt;
            }
        }

        private bool TryCacheDirectChildIdleStates()
        {
            if (_RectTransform == null || _RectTransform.childCount == 0)
            {
                return false;
            }

            _DirectChildIdleStates = new DirectChildIdleState[_RectTransform.childCount];
            for (var i = 0; i < _RectTransform.childCount; i++)
            {
                var child = _RectTransform.GetChild(i) as RectTransform;
                if (child == null)
                {
                    return false;
                }

                _DirectChildIdleStates[i] = new DirectChildIdleState(child);
            }

            return true;
        }

        private void RestoreChildIdleStates()
        {
            if (_DirectChildIdleStates == null)
            {
                return;
            }

            for (var i = 0; i < _DirectChildIdleStates.Length; i++)
            {
                DirectChildIdleState childState = _DirectChildIdleStates[i];
                if (childState.RectTransform == null)
                {
                    continue;
                }

                childState.RectTransform.anchoredPosition = childState.AnchoredPosition;
                childState.RectTransform.localEulerAngles = childState.EulerAngles;
            }
        }

        private void KillIdleAnimation()
        {
            if (_IdleAnimationTween == null)
            {
                return;
            }

            _IdleAnimationTween.Kill();
            _IdleAnimationTween = null;
        }

        [Serializable]
        public sealed class AnimationSettings
        {
            [SerializeField] private Vector2 _PositionOffset;
            [SerializeField] private Vector3 _ScaleMultiplier = Vector3.one;
            [SerializeField] private Vector3 _RotationOffset;
            [SerializeField] private Color _Color = Color.white;
            [SerializeField, Range(0f, 1f)] private float _Alpha = 1f;
            [SerializeField, Min(0f)] private float _Duration = 0.3f;
            [SerializeField, Min(0f)] private float _Delay;
            [SerializeField] private Ease _Ease = Ease.OutQuad;

            public Vector2 PositionOffset => _PositionOffset;
            public Vector3 ScaleMultiplier => _ScaleMultiplier;
            public Vector3 RotationOffset => _RotationOffset;
            public Color Color => _Color;
            public float Alpha => _Alpha;
            public float Duration => _Duration;
            public float Delay => _Delay;
            public Ease Ease => _Ease;

            public static AnimationSettings DefaultIn()
            {
                return new AnimationSettings
                {
                    _ScaleMultiplier = new Vector3(0.9f, 0.9f, 1f),
                    _Alpha = 0f,
                    _Duration = 0.35f,
                    _Ease = Ease.OutBack
                };
            }

            public static AnimationSettings DefaultOut()
            {
                return new AnimationSettings
                {
                    _ScaleMultiplier = new Vector3(0.9f, 0.9f, 1f),
                    _Alpha = 0f,
                    _Duration = 0.22f,
                    _Ease = Ease.InBack
                };
            }
        }

        [Serializable]
        public enum IdleAnimationMode
        {
            None = 0,
            Heartbeat = 1,
            HorizontalSway = 2
        }

        [Serializable]
        public sealed class IdleAnimationSettings
        {
            [SerializeField] private IdleAnimationMode _Mode;
            [SerializeField, Min(1f)] private float _PrimaryScaleMultiplier = 1.04f;
            [SerializeField, Min(1f)] private float _SecondaryScaleMultiplier = 1.02f;
            [SerializeField, Min(0f)] private float _UpDuration = 0.12f;
            [SerializeField, Min(0f)] private float _DownDuration = 0.1f;
            [SerializeField, Min(0f)] private float _SecondaryUpDuration = 0.09f;
            [SerializeField, Min(0f)] private float _SecondaryDownDuration = 0.08f;
            [SerializeField, Min(0f)] private float _BetweenLoopsDelay = 0.72f;
            [SerializeField, Min(0f)] private float _InitialDelay = 0.18f;
            [SerializeField, Min(0f)] private float _HorizontalTravel = 28f;
            [SerializeField, Min(0.01f)] private float _HorizontalCycleDuration = 5.6f;
            [SerializeField] private float _HorizontalTilt = 1.2f;
            [SerializeField] private bool _AnimateChildrenSeparately;
            [SerializeField, Min(0f)] private float _ChildPhaseOffset = 1.35f;
            [SerializeField, Min(0f)] private float _ChildCycleDurationVariance = 0.18f;
            [SerializeField, Min(0f)] private float _ChildTravelFalloff = 0.18f;
            [SerializeField, Min(0f)] private float _ChildVerticalTravel = 3f;
            [SerializeField] private Ease _UpEase = Ease.OutQuad;
            [SerializeField] private Ease _DownEase = Ease.InQuad;

            public IdleAnimationMode Mode => _Mode;
            public float PrimaryScaleMultiplier => _PrimaryScaleMultiplier;
            public float SecondaryScaleMultiplier => _SecondaryScaleMultiplier;
            public float UpDuration => _UpDuration;
            public float DownDuration => _DownDuration;
            public float SecondaryUpDuration => _SecondaryUpDuration;
            public float SecondaryDownDuration => _SecondaryDownDuration;
            public float BetweenLoopsDelay => _BetweenLoopsDelay;
            public float InitialDelay => _InitialDelay;
            public float HorizontalTravel => _HorizontalTravel;
            public float HorizontalCycleDuration => _HorizontalCycleDuration;
            public float HorizontalTilt => _HorizontalTilt;
            public bool AnimateChildrenSeparately => _AnimateChildrenSeparately;
            public float ChildPhaseOffset => _ChildPhaseOffset;
            public float ChildCycleDurationVariance => _ChildCycleDurationVariance;
            public float ChildTravelFalloff => _ChildTravelFalloff;
            public float ChildVerticalTravel => _ChildVerticalTravel;
            public Ease UpEase => _UpEase;
            public Ease DownEase => _DownEase;

            public static IdleAnimationSettings Disabled()
            {
                return new IdleAnimationSettings
                {
                    _Mode = IdleAnimationMode.None
                };
            }
        }

        private readonly struct DirectChildIdleState
        {
            public readonly RectTransform RectTransform;
            public readonly Vector2 AnchoredPosition;
            public readonly Vector3 EulerAngles;

            public DirectChildIdleState(RectTransform rectTransform)
            {
                RectTransform = rectTransform;
                AnchoredPosition = rectTransform.anchoredPosition;
                EulerAngles = rectTransform.localEulerAngles;
            }
        }
    }
}

#if UNITY_EDITOR
namespace Game.UI.Common
{
    [CustomEditor(typeof(UiElementAnimator))]
    public sealed class UiElementAnimatorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();

            using (new EditorGUI.DisabledScope(!Application.isPlaying))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Play In"))
                    {
                        PlayPreview(playIn: true);
                    }

                    if (GUILayout.Button("Play Out"))
                    {
                        PlayPreview(playIn: false);
                    }
                }
            }
        }

        private void PlayPreview(bool playIn)
        {
            var animator = (UiElementAnimator)target;
            if (animator == null)
            {
                return;
            }

            Tween tween = animator.CreateEditorPreviewTween(playIn);
            if (tween == null)
            {
                return;
            }

            if (!Application.isPlaying)
            {
                return;
            }
        }
    }
}
#endif
