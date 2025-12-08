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
    public class KnifeThrowerView : MonoBehaviour
    {
        [SerializeField] private KnifeView knifePrefab;
        [SerializeField] private Transform spawnPoint;

        private IEventChannel<KnifeSpawnRequestData> _spawnRequestEventChannel;

        [Inject]
        private void Construct(IEventChannel<KnifeSpawnRequestData> spawnRequestEventChannel)
        {
            _spawnRequestEventChannel = spawnRequestEventChannel;
        }

        private void Start()
        {
            _spawnRequestEventChannel.Subscribe(HandleKnifeSpawnRequested);
        }

        private void OnDestroy()
        {
            _spawnRequestEventChannel.Unsubscribe(HandleKnifeSpawnRequested);
        }

        private void HandleKnifeSpawnRequested(KnifeSpawnRequestData spawnData)
        {
            Transform spawnTransform = spawnPoint ?? transform;
            Vector3 spawnPosition = spawnTransform.position + new Vector3(spawnData.SpawnOffsetX, spawnData.SpawnOffsetY, 0f);
            KnifeView knifeView = Instantiate(knifePrefab, spawnPosition, Quaternion.identity);
            knifeView.Launch(Vector2.up, spawnData.ThrowSpeed);
        }
    }
}

