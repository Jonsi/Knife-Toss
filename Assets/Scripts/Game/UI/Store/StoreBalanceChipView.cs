using TMPro;
using UnityEngine;

namespace Game.UI.Store
{
    /// <summary>
    /// Top-right currency balance chip.
    /// </summary>
    public class StoreBalanceChipView : MonoBehaviour
    {
        [field: SerializeField] public TMP_Text BalanceText { get; private set; }

        public void SetBalance(int value)
        {
            if (BalanceText != null)
            {
                BalanceText.text = value.ToString();
            }
        }
    }
}
