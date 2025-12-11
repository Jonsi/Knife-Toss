using Common.Events;
using Game.Entities.Collectible.Data;
using Game.Entities.Collectible.Events;
using Game.Flow.Events;
using TMPro;
using UnityEngine;

namespace Game.UI.HUD
{
    /// <summary>
    /// Displays the total collected currency.
    /// </summary>
    public class CollectibleCountDisplay : MonoBehaviour
    {
        [SerializeField] private TMP_Text _CountText;
        [SerializeField] private LevelStartedEventChannel _LevelStartedChannel;
        [SerializeField] private CollectibleHitEventChannel _CollectibleHitChannel;
        [SerializeField] private VoidEventChannel _RetryChannel;
        [SerializeField] private VoidEventChannel _RestartChannel;

        private int _TotalCollectibles;

        private void Update()
        {
            if (_CountText == null)
                return;

            _CountText.text = _TotalCollectibles.ToString();
        }

        private void OnEnable()
        {
            if (_LevelStartedChannel != null)
                _LevelStartedChannel.Subscribe(OnLevelStarted);
            if (_CollectibleHitChannel != null)
                _CollectibleHitChannel.Subscribe(OnCollectibleHit);
            if (_RetryChannel != null)
                _RetryChannel.Subscribe(ResetCount);
            if (_RestartChannel != null)
                _RestartChannel.Subscribe(ResetCount);
        }

        private void OnDisable()
        {
            if (_LevelStartedChannel != null)
                _LevelStartedChannel.Unsubscribe(OnLevelStarted);
            if (_CollectibleHitChannel != null)
                _CollectibleHitChannel.Unsubscribe(OnCollectibleHit);
            if (_RetryChannel != null)
                _RetryChannel.Unsubscribe(ResetCount);
            if (_RestartChannel != null)
                _RestartChannel.Unsubscribe(ResetCount);
        }

        private void OnLevelStarted(LevelStartedEventData data)
        {
            _TotalCollectibles = 0;
        }

        private void OnCollectibleHit(CollectibleHitEventData data)
        {
            _TotalCollectibles += data.Value;
        }

        private void ResetCount()
        {
            _TotalCollectibles = 0;
        }
    }
}
