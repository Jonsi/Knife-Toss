using System;
using Game.Entities.Knife.Data;
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

        private IObjectResolver _Resolver;

        public event Action<KnifeSpawnRequestData> OnKnifeSpawned;

        [Inject]
        private void Construct(IObjectResolver resolver)
        {
            _Resolver = resolver;
        }

        public void SpawnKnife(KnifeSpawnRequestData spawnData)
        {
            if (_KnifePrefab == null)
                return;

            Transform spawnTransform = _SpawnPoint != null ? _SpawnPoint : transform;
            Vector3 spawnPosition = spawnTransform.position + new Vector3(spawnData.SpawnOffset.x, spawnData.SpawnOffset.y, 0f);
            KnifeView knifeView = Instantiate(_KnifePrefab, spawnPosition, Quaternion.identity);
            
            _Resolver?.Inject(knifeView);
            knifeView.Launch(Vector2.up, spawnData.ThrowSpeed);
            
            OnKnifeSpawned?.Invoke(spawnData);
        }
    }
}
