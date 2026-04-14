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
            Heartbeat = 1
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
