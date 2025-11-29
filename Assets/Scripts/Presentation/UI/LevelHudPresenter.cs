using KnifeToss.Core.Domain;
using TMPro;
using UnityEngine;

namespace KnifeToss.Presentation.UI
{
    /// <summary>
    /// Updates the HUD with information from the current level session.
    /// </summary>
    public class LevelHudPresenter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI knivesLeftText;

        public void Render(LevelSession session)
        {
            if (session == null)
                return;

            if (levelText != null)
            {
                levelText.text = $"Level {session.LevelIndex}";
            }

            if (knivesLeftText != null)
            {
                int remaining = Mathf.Max(0, session.Rules.RequiredKnives - session.KnivesStuck);
                knivesLeftText.text = $"Knives Left: {remaining}";
            }
        }
    }
}
