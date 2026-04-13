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
        private int _TotalKnives = 0;
        private int _KnivesRemaining = 0;

        private void OnEnable()
        {
            if (_LevelStartedChannel != null)
            {
                _LevelStartedChannel.Subscribe(OnLevelStarted);
                if (_LevelStartedChannel.HasLastEvent)
                    OnLevelStarted(_LevelStartedChannel.LastEvent);
            }
            if (_KnifeThrownChannel != null)
                _KnifeThrownChannel.Subscribe(OnKnifeThrown);
            if (_RetryChannel != null)
                _RetryChannel.Subscribe(OnReset);
            if (_RestartChannel != null)
                _RestartChannel.Subscribe(OnReset);
        }

        private void OnDisable()
        {
            if (_LevelStartedChannel != null)
                _LevelStartedChannel.Unsubscribe(OnLevelStarted);
            if (_KnifeThrownChannel != null)
                _KnifeThrownChannel.Unsubscribe(OnKnifeThrown);
            if (_RetryChannel != null)
                _RetryChannel.Unsubscribe(OnReset);
            if (_RestartChannel != null)
                _RestartChannel.Unsubscribe(OnReset);
        }

        private void OnLevelStarted(LevelStartedEventData data)
        {
            _TotalKnives = data.TotalKnives;
            _KnivesRemaining = data.TotalKnives;

            // Create icons if we don't have enough
            while (_Icons.Count < _TotalKnives)
            {
                var iconObject = Instantiate(_IconPrefab, _Container);
                iconObject.transform.localScale = Vector3.one;
                var image = iconObject.GetComponent<Image>();
                if (image != null)
                {
                    _Icons.Add(image);
                }
            }

            // Hide excess icons if we have too many
            for (int i = _TotalKnives; i < _Icons.Count; i++)
            {
                if (_Icons[i] != null)
                    _Icons[i].gameObject.SetActive(false);
            }

            UpdateVisuals();
        }

        private void OnKnifeThrown()
        {
            if (_KnivesRemaining > 0)
            {
                _KnivesRemaining--;
                UpdateVisuals();
            }
        }

        private void OnReset()
        {
            _KnivesRemaining = _TotalKnives;
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            for (int i = 0; i < _Icons.Count; i++)
            {
                var icon = _Icons[i];
                if (icon == null)
                    continue;

                bool isActive = i < _KnivesRemaining;
                icon.gameObject.SetActive(i < _TotalKnives);
                icon.color = isActive ? _ActiveColor : _InactiveColor;
            }
        }
    }
}
