using System;
using Common.Events;
using Game.Entities.Knife.Data;
using Game.Entities.Knife.Feedback;
using Game.KnifeThrower.Interfaces;
using Game.Pooling;
using UnityEngine;
using VContainer;

namespace Game.Entities.Knife.Views
{
    /// <summary>
    /// MonoBehaviour responsible for spawning knife instances when throw input is received.
    /// Exposes an event when a knife is spawned.
    /// </summary>
    public class KnifeThrowerView : MonoBehaviour, IKnifeThrowerView
    {
        [SerializeField] private KnifeView _KnifePrefab;
        [SerializeField] private Transform _SpawnPoint;
        [SerializeField] private KnifeLoadAnimator _LoadAnimator;

        private IEventChannel _KnifeThrownChannel;
        private KnifePoolProvider _KnifePoolProvider;
        private KnifeView _LoadedKnife;
        private KnifeSpawnRequestData _LastSpawnData;

        public event Action<KnifeSpawnRequestData> OnKnifeSpawned;

        /// <summary>
        /// Gets the knife prefab reference used for pooling.
        /// </summary>
        public KnifeView KnifePrefab => _KnifePrefab;

        [Inject]
        private void Construct(
            IEventChannel knifeThrownChannel,
            KnifePoolProvider knifePoolProvider)
        {
            _KnifeThrownChannel = knifeThrownChannel;
            _KnifePoolProvider = knifePoolProvider;
        }

        public void LoadKnife(KnifeSpawnRequestData spawnData)
        {
            if (_LoadedKnife != null)
                return;

            _LastSpawnData = spawnData;
            Transform spawnTransform = _SpawnPoint != null ? _SpawnPoint : transform;
            Vector3 spawnPosition = spawnTransform.position + new Vector3(spawnData.SpawnOffset.x, spawnData.SpawnOffset.y, 0f);
            KnifeView knifeView = _KnifePoolProvider.Get();
            knifeView.transform.SetPositionAndRotation(spawnPosition, Quaternion.identity);
            _LoadedKnife = knifeView;

            if (_LoadAnimator != null)
            {
                _LoadAnimator.PlayLoadAnimation(knifeView.VisualRoot, knifeView.SpriteRenderer);
            }

            OnKnifeSpawned?.Invoke(spawnData);
        }

        public void ThrowLoadedKnife(float throwSpeed)
        {
            if (_LoadedKnife == null)
            {
                LoadKnife(_LastSpawnData);
            }

            if (_LoadedKnife == null)
                return;

            _LoadedKnife.Launch(Vector2.up, throwSpeed);
            _KnifeThrownChannel?.Raise();
            _LoadedKnife = null;
        }
    }
}
