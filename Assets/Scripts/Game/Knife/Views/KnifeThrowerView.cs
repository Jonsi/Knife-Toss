using Common.Events;
using Game.Knife.Data;
using Game.Throwing.Interfaces;
using UnityEngine;
using VContainer;

namespace Game.Knife.Views
{
    /// <summary>
    /// MonoBehaviour responsible for spawning knife instances when throw input is received.
    /// Listens to spawn request events and creates knife instances.
    /// </summary>
    public class KnifeThrowerView : MonoBehaviour, IKnifeThrowerView
    {
        [SerializeField] private KnifeView _KnifePrefab;
        [SerializeField] private Transform _SpawnPoint;

        private IEventChannel<KnifeSpawnRequestData> _SpawnRequestEventChannel;
        private IObjectResolver _Resolver;

        [Inject]
        private void Construct(
            IEventChannel<KnifeSpawnRequestData> spawnRequestEventChannel, 
            IObjectResolver resolver)
        {
            _SpawnRequestEventChannel = spawnRequestEventChannel;
            _Resolver = resolver;
        }

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            if (_SpawnRequestEventChannel != null)
            {
                _SpawnRequestEventChannel.Subscribe(SpawnKnife);
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (_SpawnRequestEventChannel != null)
            {
                _SpawnRequestEventChannel.Unsubscribe(SpawnKnife);
            }
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
        }
    }
}
