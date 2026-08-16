using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Popups
{
    /// <summary>
    /// Plays the failure-popup knife impact once, then holds the approved final pose.
    /// Uses unscaled time so the presentation remains visible while gameplay is paused.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class FailureKnifeSpriteAnimator : MonoBehaviour
    {
        [SerializeField] private Image _TargetImage;
        [SerializeField] private Sprite[] _Frames;
        [SerializeField, Min(0f)] private float _StartDelay = 0.08f;
        [SerializeField, Min(0.01f)] private float _SecondsPerFrame = 0.028f;
        [SerializeField] private bool _UseUnscaledTime = true;

        private float _Elapsed;
        private int _CurrentFrameIndex;
        private bool _IsPlaying;

        public int CurrentFrameIndex => _CurrentFrameIndex;
        public bool IsPlaying => _IsPlaying;

        private void OnEnable()
        {
            Restart();
        }

        private void Update()
        {
            AdvanceAnimation(_UseUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime);
        }

        public void Restart()
        {
            _Elapsed = 0f;
            _CurrentFrameIndex = 0;
            _IsPlaying = _TargetImage != null && _Frames is { Length: > 0 };
            ApplyFrame(0);
        }

        public void AdvanceAnimation(float deltaTime)
        {
            if (!_IsPlaying)
                return;

            _Elapsed += Mathf.Max(0f, deltaTime);
            if (_Elapsed < _StartDelay)
                return;

            int nextFrame = Mathf.Min(
                Mathf.FloorToInt((_Elapsed - _StartDelay) / Mathf.Max(0.01f, _SecondsPerFrame)),
                _Frames.Length - 1);
            if (nextFrame != _CurrentFrameIndex)
            {
                _CurrentFrameIndex = nextFrame;
                ApplyFrame(_CurrentFrameIndex);
            }

            if (_CurrentFrameIndex == _Frames.Length - 1)
                _IsPlaying = false;
        }

        private void ApplyFrame(int index)
        {
            if (_TargetImage == null || _Frames == null || index < 0 || index >= _Frames.Length)
                return;

            Sprite frame = _Frames[index];
            if (frame != null)
                _TargetImage.sprite = frame;
        }

        private void OnValidate()
        {
            _StartDelay = Mathf.Max(0f, _StartDelay);
            _SecondsPerFrame = Mathf.Max(0.01f, _SecondsPerFrame);
        }
    }
}
