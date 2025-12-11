using System;
using Common.Events;
using Game.Entities.Knife.Data;
using Game.Entities.Knife.Feedback;
using Game.KnifeThrower.Interfaces;
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

        private IObjectResolver _Resolver;
        private IEventChannel _KnifeThrownChannel;
        private KnifeView _LoadedKnife;
        private KnifeSpawnRequestData _LastSpawnData;

        public event Action<KnifeSpawnRequestData> OnKnifeSpawned;

        [Inject]
        private void Construct(
            IObjectResolver resolver,
            IEventChannel knifeThrownChannel)
        {
            _Resolver = resolver;
            _KnifeThrownChannel = knifeThrownChannel;
        }

        public void LoadKnife(KnifeSpawnRequestData spawnData)
        {
            if (_KnifePrefab == null || _LoadedKnife != null)
                return;

            _LastSpawnData = spawnData;
            Transform spawnTransform = _SpawnPoint != null ? _SpawnPoint : transform;
            Vector3 spawnPosition = spawnTransform.position + new Vector3(spawnData.SpawnOffset.x, spawnData.SpawnOffset.y, 0f);
            KnifeView knifeView = Instantiate(_KnifePrefab, spawnPosition, Quaternion.identity);

            _Resolver?.Inject(knifeView);
            knifeView.ResetKnife();
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
