using TMPro;
using UnityEngine;

namespace Game.UI.Store
{
    /// <summary>
    /// Shared status-pill prefab used for owned, equip, and equipped states.
    /// </summary>
    public sealed class StoreStatusPillView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _LabelText;

        public TMP_Text LabelText => _LabelText;

        public void SetLabel(string text)
        {
            if (_LabelText != null)
            {
                _LabelText.text = text ?? string.Empty;
            }
        }
    }
}
