using Core.Abstractions;
using Core.Data;
using Infrastructure.Events;
using UnityEngine;

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
        [SerializeField] private KnifeSpawnRequestEventChannel spawnRequestEventChannel;

        private void Start()
        {
            spawnRequestEventChannel.Subscribe(HandleKnifeSpawnRequested);
        }

        private void OnDestroy()
        {
            spawnRequestEventChannel.Unsubscribe(HandleKnifeSpawnRequested);
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

