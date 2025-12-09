using KnifeToss.Core.Abstractions.Events;
using KnifeToss.Core.Abstractions.Throwing;
using KnifeToss.Core.Data.Knife;
using UnityEngine;
using VContainer;

namespace Presentation.Gameplay.Knife
{
    /// <summary>
    /// MonoBehaviour responsible for spawning knife instances when throw input is received.
    /// Listens to spawn request events and creates knife instances.
    /// </summary>
    public class KnifeThrowerView : MonoBehaviour, IKnifeThrowerView
    {
        [SerializeField] private KnifeView knifePrefab;
        [SerializeField] private Transform spawnPoint;

        private IEventChannel<KnifeSpawnRequestData> _spawnRequestEventChannel;
        private IObjectResolver _resolver;

        [Inject]
        private void Construct(
            IEventChannel<KnifeSpawnRequestData> spawnRequestEventChannel, 
            IObjectResolver resolver)
        {
            _spawnRequestEventChannel = spawnRequestEventChannel;
            _resolver = resolver;
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
            if (_spawnRequestEventChannel != null)
            {
                _spawnRequestEventChannel.Subscribe(SpawnKnife);
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (_spawnRequestEventChannel != null)
            {
                _spawnRequestEventChannel.Unsubscribe(SpawnKnife);
            }
        }

        public void SpawnKnife(KnifeSpawnRequestData spawnData)
        {
            if (knifePrefab == null)
                return;

            Transform spawnTransform = spawnPoint != null ? spawnPoint : transform;
            Vector3 spawnPosition = spawnTransform.position + new Vector3(spawnData.SpawnOffset.x, spawnData.SpawnOffset.y, 0f);
            KnifeView knifeView = Instantiate(knifePrefab, spawnPosition, Quaternion.identity);
            
            _resolver?.Inject(knifeView);
            knifeView.Launch(Vector2.up, spawnData.ThrowSpeed);
        }
    }
}
