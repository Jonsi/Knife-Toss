using Common.Events;
using Cysharp.Threading.Tasks;
using Game.Flow.Game;
using Game.Flow.Events;
using Game.Flow.Level;
using Game.UI.Common;
using Game.UI.Popups;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Controllers
{
    public class UIController : MonoBehaviour
    {
        [Header("Background")]
        [SerializeField] private Image _BackgroundImage;

        [Header("Popups")]
        [SerializeField] private StageFailPopup _StageFailPopup;
        [SerializeField] private UiElementAnimator _ReturnButtonAnimator;

        [Header("Startup Animation")]
        [SerializeField] private UiElementAnimator[] _StartupAnimators;

        [Header("Channels")]
        [SerializeField] private LevelFailedEventChannel _LevelFailedChannel;
        [SerializeField] private LevelStartedEventChannel _LevelStartedChannel;
        [SerializeField] private VoidEventChannel _RetryLevelChannel;
        [SerializeField] private VoidEventChannel _RestartStageChannel;

        public void SetBackground(Sprite sprite)
        {
            if (_BackgroundImage != null && sprite != null)
            {
                _BackgroundImage.sprite = sprite;
            }
        }

        private void Start()
        {
            HideReturnButton(immediate: true);
            PlayStartupAnimationsAsync().Forget();
        }

        private void OnEnable()
        {
            if (_LevelFailedChannel != null)
                _LevelFailedChannel.Subscribe(OnLevelFailed);

            if (_LevelStartedChannel != null)
                _LevelStartedChannel.Subscribe(OnLevelStarted);
            
            if (_RetryLevelChannel != null)
                _RetryLevelChannel.Subscribe(OnRetryOrRestart);

            if (_RestartStageChannel != null)
                _RestartStageChannel.Subscribe(OnRetryOrRestart);
        }

        private void OnDisable()
        {
            if (_LevelFailedChannel != null)
                _LevelFailedChannel.Unsubscribe(OnLevelFailed);

            if (_LevelStartedChannel != null)
                _LevelStartedChannel.Unsubscribe(OnLevelStarted);

            if (_RetryLevelChannel != null)
                _RetryLevelChannel.Unsubscribe(OnRetryOrRestart);

            if (_RestartStageChannel != null)
                _RestartStageChannel.Unsubscribe(OnRetryOrRestart);
        }

        private void OnLevelFailed(LevelData data)
        {
            if (_StageFailPopup != null)
                _StageFailPopup.Show();

            ShowReturnButton();
        }

        private void OnLevelStarted(LevelStartedEventData data)
        {
            HideReturnButton();
        }

        private void OnRetryOrRestart()
        {
            if (_StageFailPopup != null)
                _StageFailPopup.Hide();

            HideReturnButton();
        }

        private async UniTaskVoid PlayStartupAnimationsAsync()
        {
            if (_StartupAnimators == null || _StartupAnimators.Length == 0)
            {
                return;
            }

            await DeferredGameSceneVisibility.WaitForReveal();
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

            foreach (UiElementAnimator animator in _StartupAnimators)
            {
                animator?.PlayIn();
            }
        }

        public async UniTask PlayStageOutAsync()
        {
            var tasks = new List<UniTask>();
            var seen = new HashSet<UiElementAnimator>();

            AddPlayOutTasks(_StartupAnimators, tasks, seen);

            if (_ReturnButtonAnimator != null &&
                _ReturnButtonAnimator.gameObject.activeInHierarchy &&
                seen.Add(_ReturnButtonAnimator))
            {
                tasks.Add(_ReturnButtonAnimator.PlayOutAsync());
            }

            if (tasks.Count > 0)
            {
                await UniTask.WhenAll(tasks);
            }
        }

        private void ShowReturnButton()
        {
            _ReturnButtonAnimator?.PlayIn();
        }

        private void HideReturnButton(bool immediate = false)
        {
            if (_ReturnButtonAnimator == null)
            {
                return;
            }

            if (immediate)
            {
                _ReturnButtonAnimator.gameObject.SetActive(false);
                return;
            }

            _ReturnButtonAnimator.PlayOutAsync().Forget();
        }

        private static void AddPlayOutTasks(
            IReadOnlyList<UiElementAnimator> animators,
            List<UniTask> tasks,
            HashSet<UiElementAnimator> seen)
        {
            if (animators == null)
            {
                return;
            }

            for (var i = 0; i < animators.Count; i++)
            {
                UiElementAnimator animator = animators[i];
                if (animator != null && seen.Add(animator))
                {
                    tasks.Add(animator.PlayOutAsync());
                }
            }
        }
    }
}
