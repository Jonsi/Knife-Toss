using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using Game.Flow.Game;
using Game.PowerUps.Configs;
using VContainer;

namespace Game.UI.PowerUps
{
    public class PowerUpsMenuController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private PowerUpCatalog _Catalog;
        [SerializeField] private PowerUpListItem _ItemPrefab;
        [SerializeField] private Transform _ListContainer;

        [Header("Navigation")]
        [SerializeField] private Button _BackButton;

        [Inject] private SceneLoaderService _SceneLoaderService;

        private List<PowerUpListItem> _SpawnedItems = new List<PowerUpListItem>();

        private void Start()
        {
            if (_BackButton != null)
            {
                _BackButton.onClick.AddListener(OnBackClicked);
            }

            Initialize();
        }

        private void OnDestroy()
        {
            if (_BackButton != null)
            {
                _BackButton.onClick.RemoveListener(OnBackClicked);
            }
        }

        private void Initialize()
        {
            RefreshList();
        }

        private void RefreshList()
        {
            // Clear existing items
            foreach (var item in _SpawnedItems)
            {
                Destroy(item.gameObject);
            }
            _SpawnedItems.Clear();

            if (_Catalog == null || _ItemPrefab == null || _ListContainer == null)
            {
                Debug.LogWarning("PowerUpsMenuController: Missing references for list generation.");
                return;
            }

            foreach (var config in _Catalog.Entries)
            {
                var item = Instantiate(_ItemPrefab, _ListContainer);
                item.Setup(config, null); // No click action needed if just displaying info
                _SpawnedItems.Add(item);
            }
        }

        private void OnBackClicked()
        {
            _SceneLoaderService.LoadMainMenu().Forget();
        }
    }
}
