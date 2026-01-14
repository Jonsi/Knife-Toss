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
        private readonly System.Collections.Generic.List<KnifeView> _LoadedKnives = new System.Collections.Generic.List<KnifeView>();

        [Inject]
        private void Construct(IEventChannel knifeThrownChannel)
        {
            _KnifeThrownChannel = knifeThrownChannel;
        }

        public void LoadKnife(KnifeView knife, KnifeSpawnRequestData spawnData)
        {
            if (knife == null)
                return;

            LoadKnives(new[] { knife }, spawnData, 0f);
        }

        public void LoadKnives(System.Collections.Generic.IReadOnlyList<KnifeView> knives, KnifeSpawnRequestData spawnData, float spacing)
        {
            if (knives == null || knives.Count == 0)
            {
                ClearLoadedKnives();
                return;
            }

            DeactivateOldKnivesExcept(knives);
            _LoadedKnives.Clear();
            _LoadedKnives.AddRange(knives);
            _LoadedKnife = _LoadedKnives[0];

            Transform spawnTransform = _SpawnPoint != null ? _SpawnPoint : transform;
            float centeredOffset = (knives.Count - 1) * 0.5f;
            for (int i = 0; i < knives.Count; i++)
            {
                var knife = knives[i];
                if (knife == null)
                    continue;

                float offsetX = (i - centeredOffset) * spacing;
                Vector3 spawnPosition = spawnTransform.position +
                    new Vector3(spawnData.SpawnOffset.x + offsetX, spawnData.SpawnOffset.y, 0f);
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
        }

        public void ThrowLoadedKnife(float throwSpeed)
        {
            ThrowLoadedKnives(throwSpeed);
        }

        public int ThrowLoadedKnives(float throwSpeed)
        {
            return ThrowLoadedKnivesWithDirectionInternal(Vector2.up, throwSpeed);
        }

        /// <summary>
        /// Launches the currently loaded knife with the given direction and speed.
        /// </summary>
        /// <param name="direction">Launch direction.</param>
        /// <param name="throwSpeed">Launch speed.</param>
        public void ThrowLoadedKnifeWithDirection(Vector2 direction, float throwSpeed)
        {
            if (_LoadedKnives.Count == 0)
                return;

            _ = ThrowLoadedKnivesWithDirectionInternal(direction, throwSpeed);
        }

        private int ThrowLoadedKnivesWithDirectionInternal(Vector2 direction, float throwSpeed)
        {
            if (_LoadedKnives.Count == 0)
                return 0;

            int thrownCount = 0;
            for (int i = 0; i < _LoadedKnives.Count; i++)
            {
                var knife = _LoadedKnives[i];
                if (knife == null)
                    continue;

                knife.Launch(direction, throwSpeed);
                _KnifeThrownChannel?.Raise();
                thrownCount++;
            }

            _LoadedKnives.Clear();
            _LoadedKnife = null;
            return thrownCount;
        }

        /// <summary>
        /// Gets the transform used for spawning knives.
        /// </summary>
        public Transform SpawnPoint => _SpawnPoint != null ? _SpawnPoint : transform;

        /// <summary>
        /// Gets the knife currently loaded into the thrower.
        /// </summary>
        public KnifeView LoadedKnife => _LoadedKnife;

        public System.Collections.Generic.IReadOnlyList<KnifeView> LoadedKnives => _LoadedKnives;

        private void ClearLoadedKnives()
        {
            for (int i = 0; i < _LoadedKnives.Count; i++)
            {
                var knife = _LoadedKnives[i];
                if (knife != null)
                {
                    knife.gameObject.SetActive(false);
                }
            }

            _LoadedKnives.Clear();
            _LoadedKnife = null;
        }

        private void DeactivateOldKnivesExcept(System.Collections.Generic.IReadOnlyList<KnifeView> knives)
        {
            for (int i = 0; i < _LoadedKnives.Count; i++)
            {
                var oldKnife = _LoadedKnives[i];
                if (oldKnife == null)
                    continue;

                bool stillLoaded = false;
                for (int j = 0; j < knives.Count; j++)
                {
                    if (knives[j] == oldKnife)
                    {
                        stillLoaded = true;
                        break;
                    }
                }

                if (!stillLoaded)
                {
                    oldKnife.gameObject.SetActive(false);
                }
            }
        }
    }
}
