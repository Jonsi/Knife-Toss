using TMPro;
using UnityEngine;

namespace Game.PVP.UI
{
    public class PVPPlayerHudView : MonoBehaviour
    {
        [Header("Status")]
        [SerializeField] private TMP_Text _StatusText;
        
        public void SetResult(bool isWinner)
        {
            if (_StatusText != null)
            {
                _StatusText.text = isWinner ? "YOU WON!" : "YOU LOST!";
                _StatusText.color = isWinner ? Color.green : Color.red;
            }
        }

        public void Clear()
        {
            if (_StatusText != null)
            {
                _StatusText.text = "";
            }
        }
    }
}
