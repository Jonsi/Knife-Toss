using UnityEngine;
using UnityEngine.UI;

namespace KnifeToss.Presentation.UI
{
    /// <summary>
    /// Handles showing fail/complete states without exposing UI concerns to the controller.
    /// </summary>
    public class GameStatePresenter : MonoBehaviour
    {
        [SerializeField] private GameObject failText;
        [SerializeField] private Button nextLevelButton;

        public void ShowFail(bool show)
        {
            if (failText != null)
            {
                failText.SetActive(show);
            }
        }

        public void ShowLevelComplete(bool show)
        {
            if (nextLevelButton != null)
            {
                nextLevelButton.gameObject.SetActive(show);
            }
        }

        public void ResetState()
        {
            ShowFail(false);
            ShowLevelComplete(false);
        }
    }
}
