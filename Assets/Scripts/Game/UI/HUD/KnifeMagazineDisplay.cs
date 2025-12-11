using System.Collections.Generic;
using Common.Events;
using Game.Entities.Knife.Events;
using Game.Flow.Events;
using Game.KnifeThrower.Events;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.HUD
{
    /// <summary>
    /// Displays remaining knives for the current level.
    /// </summary>
    public class KnifeMagazineDisplay : MonoBehaviour
    {
        [SerializeField] private Transform _Container;
        [SerializeField] private GameObject _IconPrefab;
        [SerializeField] private Color _ActiveColor = Color.white;
        [SerializeField] private Color _InactiveColor = Color.gray;
        [SerializeField] private LevelStartedEventChannel _LevelStartedChannel;
        [SerializeField] private KnifeThrownEventChannel _KnifeThrownChannel;
        [SerializeField] private VoidEventChannel _RetryChannel;
        [SerializeField] private VoidEventChannel _RestartChannel;

        private readonly List<Image> _Icons = new List<Image>();
        private int _CachedTotalKnives = -1;
        private int _KnivesRemaining = 0;

        private void OnEnable()
        {
            if (_LevelStartedChannel != null)
                _LevelStartedChannel.Subscribe(OnLevelStarted);
            if (_KnifeThrownChannel != null)
                _KnifeThrownChannel.Subscribe(OnKnifeThrown);
            if (_RetryChannel != null)
                _RetryChannel.Subscribe(ResetCounts);
            if (_RestartChannel != null)
                _RestartChannel.Subscribe(ResetCounts);

            RebuildIfNeeded();
            Refresh();
        }

        private void Update()
        {
            RebuildIfNeeded();
            Refresh();
        }

        private void OnDisable()
        {
            if (_LevelStartedChannel != null)
                _LevelStartedChannel.Unsubscribe(OnLevelStarted);
            if (_KnifeThrownChannel != null)
                _KnifeThrownChannel.Unsubscribe(OnKnifeThrown);
            if (_RetryChannel != null)
                _RetryChannel.Unsubscribe(ResetCounts);
            if (_RestartChannel != null)
                _RestartChannel.Unsubscribe(ResetCounts);
        }

        private void OnLevelStarted(LevelStartedEventData data)
        {
            _CachedTotalKnives = data.TotalKnives;
            _KnivesRemaining = data.TotalKnives;
            Rebuild();
            Refresh();
        }

        private void OnKnifeThrown()
        {
            if (_KnivesRemaining > 0)
                _KnivesRemaining--;
        }

        private void ResetCounts()
        {
            _KnivesRemaining = _CachedTotalKnives;
            Refresh();
        }

        private void RebuildIfNeeded()
        {
            if (_Container == null || _IconPrefab == null)
                return;

            if (_CachedTotalKnives < 0)
                return;

            Rebuild();
        }

        private void Rebuild()
        {
            for (int i = 0; i < _Icons.Count; i++)
            {
                if (_Icons[i] != null)
                    Destroy(_Icons[i].gameObject);
            }
            _Icons.Clear();

            for (int i = 0; i < _CachedTotalKnives; i++)
            {
                var iconObject = Instantiate(_IconPrefab, _Container);
                iconObject.transform.localScale = Vector3.one;
                var image = iconObject.GetComponent<Image>();
                if (image != null)
                {
                    _Icons.Add(image);
                }
            }
        }

        private void Refresh()
        {
            if (_CachedTotalKnives < 0)
                return;

            for (int i = 0; i < _Icons.Count; i++)
            {
                var icon = _Icons[i];
                if (icon == null)
                    continue;

                icon.color = i < _KnivesRemaining ? _ActiveColor : _InactiveColor;
            }
        }
    }
}
