using System.Collections.Generic;
using Game.Presentation.Common;
using Game.PowerUps.Configs;
using Game.PowerUps.Views;
using R3;
using UnityEngine;

namespace Game.Presentation.HUD.PowerUps
{
    /// <summary>
    /// Displays power-up buttons and exposes selection events.
    /// </summary>
    public class PowerUpHudView : BaseView<PowerUpHudModel>
    {
        [SerializeField] private PowerUpCatalog _Catalog;
        [SerializeField] private PowerUpButtonView _ButtonPrefab;
        [SerializeField] private Transform _Container;

        private readonly List<PowerUpButtonView> _Buttons = new List<PowerUpButtonView>();
        private readonly Subject<PowerUpConfigBase> _ButtonClicks = new Subject<PowerUpConfigBase>();

        public Observable<PowerUpConfigBase> ButtonClicked => _ButtonClicks;

        public void SetPowerUpCounts(int superKnifeCount, int multipleKnivesCount, int collectiblesBladeCount)
        {
            EnsureButtonsBuilt();

            for (int i = 0; i < _Buttons.Count; i++)
            {
                var button = _Buttons[i];
                if (button == null)
                {
                    continue;
                }

                button.SetCount(GetCount(superKnifeCount, multipleKnivesCount, collectiblesBladeCount, button.Config));
            }
        }

        public void SetSelectedPowerUp(PowerUpConfigBase selectedConfig)
        {
            EnsureButtonsBuilt();

            for (int i = 0; i < _Buttons.Count; i++)
            {
                var button = _Buttons[i];
                if (button == null)
                {
                    continue;
                }

                button.UpdateSelected(button.Config == selectedConfig);
            }
        }

        private void EnsureButtonsBuilt()
        {
            if (_Buttons.Count > 0)
            {
                return;
            }

            if (_Catalog == null || _ButtonPrefab == null || _Container == null)
            {
                return;
            }

            for (int i = 0; i < _Catalog.Entries.Count; i++)
            {
                var config = _Catalog.Entries[i];
                if (config == null)
                {
                    continue;
                }

                var view = Instantiate(_ButtonPrefab, _Container);
                view.Initialize(config, HandleButtonClicked);
                _Buttons.Add(view);
            }
        }

        private void HandleButtonClicked(PowerUpConfigBase config)
        {
            if (config == null)
            {
                return;
            }

            _ButtonClicks.OnNext(config);
        }

        private static int GetCount(int superKnifeCount, int multipleKnivesCount, int collectiblesBladeCount, PowerUpConfigBase config)
        {
            return config switch
            {
                SuperKnifeConfig => superKnifeCount,
                MultipleKnivesConfig => multipleKnivesCount,
                CollectiblesBladeConfig => collectiblesBladeCount,
                _ => 0
            };
        }

        private void OnDestroy()
        {
            _ButtonClicks.OnCompleted();
        }
    }
}
