using TMPro;
using UnityEngine;

namespace Game.UI.Store
{
    /// <summary>
    /// Reusable Store section header whose text is authored with TextMesh Pro.
    /// </summary>
    public sealed class StoreSectionHeaderView : MonoBehaviour
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
