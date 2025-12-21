using System.Collections.Generic;
using Game.Presentation.Common;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation.HUD.KnifeMagazine
{
    /// <summary>
    /// Displays the knife magazine HUD state.
    /// </summary>
    public class KnifeMagazineView : BaseView<KnifeMagazineModel>
    {
        [SerializeField] private Transform _Container;
        [SerializeField] private GameObject _IconPrefab;
        [SerializeField] private Color _ActiveColor = Color.white;
        [SerializeField] private Color _InactiveColor = Color.gray;

        private readonly List<Image> _Icons = new List<Image>();

        public void SetKnifeMagazine(int totalKnives, int knivesRemaining)
        {
            if (_Container == null || _IconPrefab == null)
            {
                return;
            }

            totalKnives = Mathf.Max(0, totalKnives);
            knivesRemaining = Mathf.Clamp(knivesRemaining, 0, totalKnives);

            while (_Icons.Count < totalKnives)
            {
                var iconObject = Instantiate(_IconPrefab, _Container);
                iconObject.transform.localScale = Vector3.one;
                var image = iconObject.GetComponent<Image>();
                if (image != null)
                {
                    _Icons.Add(image);
                }
            }

            for (int i = totalKnives; i < _Icons.Count; i++)
            {
                if (_Icons[i] != null)
                {
                    _Icons[i].gameObject.SetActive(false);
                }
            }

            for (int i = 0; i < _Icons.Count; i++)
            {
                var icon = _Icons[i];
                if (icon == null)
                {
                    continue;
                }

                bool isActive = i < knivesRemaining;
                icon.gameObject.SetActive(i < totalKnives);
                icon.color = isActive ? _ActiveColor : _InactiveColor;
            }
        }
    }
}
