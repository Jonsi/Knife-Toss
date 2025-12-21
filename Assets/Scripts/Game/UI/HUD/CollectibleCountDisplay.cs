using Common.Events;
using Game.Entities.Collectible.Data;
using Game.Entities.Collectible.Events;
using Game.Flow.Events;
using Game.Flow.Game;
using TMPro;
using UnityEngine;
using VContainer;

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

        private GameData _GameData;
        private int _TotalCollectibles;

        [Inject]
        private void Construct(GameData gameData)
        {
            _GameData = gameData;
            _TotalCollectibles = _GameData.TotalCollected;
            UpdateCountText();
        }

        private void OnEnable()
        {
            if (_LevelStartedChannel != null)
                _LevelStartedChannel.Subscribe(OnLevelStarted);
            if (_CollectibleHitChannel != null)
                _CollectibleHitChannel.Subscribe(OnCollectibleHit);

            SyncFromGameData();
        }

        private void OnDisable()
        {
            if (_LevelStartedChannel != null)
                _LevelStartedChannel.Unsubscribe(OnLevelStarted);
            if (_CollectibleHitChannel != null)
                _CollectibleHitChannel.Unsubscribe(OnCollectibleHit);
        }

        private void OnLevelStarted(LevelStartedEventData data)
        {
            SyncFromGameData();
        }

        private void OnCollectibleHit(CollectibleHitEventData data)
        {
            _TotalCollectibles += data.Value;
            UpdateCountText();
        }

        private void SyncFromGameData()
        {
            if (_GameData == null)
            {
                return;
            }

            _TotalCollectibles = _GameData.TotalCollected;
            UpdateCountText();
        }

        private void UpdateCountText()
        {
            if (_CountText == null)
            {
                return;
            }

            _CountText.text = _TotalCollectibles.ToString();
        }
    }
}
