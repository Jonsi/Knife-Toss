using System.Collections.Generic;
using Game.Entities.Knife.Data;
using Game.Entities.Knife.Feedback;
using Game.Entities.Knife.Views;
using Game.PowerUps.Interfaces;
using UnityEngine;

namespace Game.PowerUps.Views
{
    /// <summary>
    /// Specialized thrower for power-ups that launches knives without triggering magazine events.
    /// </summary>
    public class PowerUpThrowerView : MonoBehaviour, IPowerUpThrowerView
    {
        [SerializeField] private Transform _SpawnPoint;
        [SerializeField] private KnifeLoadAnimator _LoadAnimator;

        private readonly List<KnifeView> _LoadedKnives = new List<KnifeView>();
        private bool _IsVisible = true;
        private float _LoadedThrowSpeed;

        public Transform SpawnPoint => _SpawnPoint != null ? _SpawnPoint : transform;
        public IReadOnlyList<KnifeView> LoadedKnives => _LoadedKnives;

        public void SetVisible(bool visible)
        {
            _IsVisible = visible;
            for (int i = 0; i < _LoadedKnives.Count; i++)
            {
                var knife = _LoadedKnives[i];
                if (knife != null)
                {
                    knife.gameObject.SetActive(visible);
                }
            }
        }

        public void LoadKnife(KnifeView knife, KnifeSpawnRequestData spawnData)
        {
            if (knife == null)
                return;

            LoadKnives(new[] { knife }, spawnData, 0f);
        }

        public void LoadKnives(IReadOnlyList<KnifeView> knives, KnifeSpawnRequestData spawnData, float spacing)
        {
            if (knives == null || knives.Count == 0)
            {
                ClearLoadedKnives();
                return;
            }

            DeactivateOldKnivesExcept(knives);
            _LoadedKnives.Clear();
            _LoadedKnives.AddRange(knives);
            _LoadedThrowSpeed = spawnData.ThrowSpeed;

            Transform spawnTransform = SpawnPoint;
            float centeredOffset = (knives.Count - 1) * 0.5f;
            for (int i = 0; i < knives.Count; i++)
            {
                var knife = knives[i];
                if (knife == null) continue;

                knife.RefreshVisualSkin();

                float offsetX = (i - centeredOffset) * spacing;
                Vector3 spawnPosition = spawnTransform.position +
                    new Vector3(spawnData.SpawnOffset.x + offsetX, spawnData.SpawnOffset.y, 0f);
                
                knife.transform.SetPositionAndRotation(spawnPosition, Quaternion.identity);
                knife.transform.SetParent(transform, true);
                knife.RefreshKnifePowerUpVfx();
                
                // Ensure the knife follows the current visibility state
                knife.gameObject.SetActive(_IsVisible);

                if (_LoadAnimator != null)
                {
                    _LoadAnimator.PlayLoadAnimation(knife.VisualRoot, knife.SpriteRenderer);
                }
            }
        }

        public void ThrowLoadedKnife()
        {
            ThrowLoadedKnives();
        }

        public int ThrowLoadedKnives()
        {
            return ThrowLoadedKnivesWithDirectionInternal(Vector2.up);
        }

        public void ThrowLoadedKnifeWithDirection(Vector2 direction)
        {
            if (_LoadedKnives.Count == 0) return;
            ThrowLoadedKnivesWithDirectionInternal(direction);
        }

        private int ThrowLoadedKnivesWithDirectionInternal(Vector2 direction)
        {
            if (_LoadedKnives.Count == 0) return 0;

            int thrownCount = 0;
            for (int i = 0; i < _LoadedKnives.Count; i++)
            {
                var knife = _LoadedKnives[i];
                if (knife == null) continue;

                knife.Launch(direction, _LoadedThrowSpeed);
                thrownCount++;
            }

            _LoadedKnives.Clear();
            _LoadedThrowSpeed = 0f;
            return thrownCount;
        }

        public void ClearLoadedKnives()
        {
            for (int i = 0; i < _LoadedKnives.Count; i++)
            {
                if (_LoadedKnives[i] != null)
                {
                    _LoadedKnives[i].ReturnToPool();
                }
            }
            _LoadedKnives.Clear();
            _LoadedThrowSpeed = 0f;
        }

        private void DeactivateOldKnivesExcept(IReadOnlyList<KnifeView> knives)
        {
            for (int i = 0; i < _LoadedKnives.Count; i++)
            {
                var oldKnife = _LoadedKnives[i];
                if (oldKnife == null) continue;

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
                    oldKnife.ReturnToPool();
                }
            }
        }
    }
}
