using Core.Abstractions;
using Core.Data;
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

        [Inject]
        private void Construct(IEventChannel<KnifeSpawnRequestData> spawnRequestEventChannel)
        {
            _spawnRequestEventChannel = spawnRequestEventChannel;
            SubscribeToEvents();
        }

        private void Start()
        {
            if (_spawnRequestEventChannel != null)
            {
                SubscribeToEvents();
            }
        }

        private void SubscribeToEvents()
        {
            if (_spawnRequestEventChannel != null)
            {
                _spawnRequestEventChannel.Subscribe(HandleKnifeSpawnRequested);
            }
        }

        private void OnDestroy()
        {
            if (_spawnRequestEventChannel != null)
            {
                _spawnRequestEventChannel.Unsubscribe(HandleKnifeSpawnRequested);
            }
        }

        private void HandleKnifeSpawnRequested(KnifeSpawnRequestData spawnData)
        {
            if (knifePrefab == null)
                return;

            Transform spawnTransform = spawnPoint ?? transform;
            Vector3 spawnPosition = spawnTransform.position + new Vector3(spawnData.SpawnOffsetX, spawnData.SpawnOffsetY, 0f);
            KnifeView knifeView = Instantiate(knifePrefab, spawnPosition, Quaternion.identity);
            knifeView.Launch(Vector2.up, spawnData.ThrowSpeed);
        }
    }
}

