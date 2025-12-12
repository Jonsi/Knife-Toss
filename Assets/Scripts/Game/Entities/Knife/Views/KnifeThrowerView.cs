using Common.Events;
using Game.Entities.Knife.Data;
using Game.Entities.Knife.Feedback;
using Game.Entities.Knife.Views;
using Game.KnifeThrower.Interfaces;
using UnityEngine;
using VContainer;

namespace Game.Entities.Knife.Views
{
    /// <summary>
    /// MonoBehaviour responsible for positioning and launching knife instances.
    /// </summary>
    public class KnifeThrowerView : MonoBehaviour, IKnifeThrowerView
    {
        [SerializeField] private Transform _SpawnPoint;
        [SerializeField] private KnifeLoadAnimator _LoadAnimator;

        private IEventChannel _KnifeThrownChannel;
        private KnifeView _LoadedKnife;

        [Inject]
        private void Construct(IEventChannel knifeThrownChannel)
        {
            _KnifeThrownChannel = knifeThrownChannel;
        }

        public void LoadKnife(KnifeView knife, KnifeSpawnRequestData spawnData)
        {
            // If there's already a loaded knife, return it to the pool first
            if (_LoadedKnife != null && _LoadedKnife != knife)
            {
                // The old knife should have been returned to pool by level start logic,
                // but just in case, make sure it's not active
                _LoadedKnife.gameObject.SetActive(false);
            }

            _LoadedKnife = knife;
            Transform spawnTransform = _SpawnPoint != null ? _SpawnPoint : transform;
            Vector3 spawnPosition = spawnTransform.position + new Vector3(spawnData.SpawnOffset.x, spawnData.SpawnOffset.y, 0f);
            knife.transform.SetPositionAndRotation(spawnPosition, Quaternion.identity);

            // Parent the knife to the thrower for proper hierarchy organization
            knife.transform.SetParent(transform, true);

            // Ensure the knife is active and visible after parenting
            knife.gameObject.SetActive(true);

            if (_LoadAnimator != null)
            {
                _LoadAnimator.PlayLoadAnimation(knife.VisualRoot, knife.SpriteRenderer);
            }
        }

        public void ThrowLoadedKnife(float throwSpeed)
        {
            if (_LoadedKnife == null)
                return;

            _LoadedKnife.Launch(Vector2.up, throwSpeed);
            _KnifeThrownChannel?.Raise();
            _LoadedKnife = null;
        }
    }
}
