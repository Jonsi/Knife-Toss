using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game.PVP.UI
{
    public class PVPHudController : MonoBehaviour
    {
        [Header("Player HUDs")]
        [SerializeField] private PVPPlayerHudView _Player1Hud;
        [SerializeField] private PVPPlayerHudView _Player2Hud;

        [Header("Shared UI")]
        [SerializeField] private GameObject _SharedGameOverPanel;
        [SerializeField] private Button _RematchButton;
        [SerializeField] private Button _ExitButton;

        public event Action OnRematchClicked;
        public event Action OnExitClicked;

        private void Start()
        {
            if (_RematchButton != null)
                _RematchButton.onClick.AddListener(OnRematch);
            
            if (_ExitButton != null)
                _ExitButton.onClick.AddListener(OnExit);
                
            HideWinnerOverlay();
        }

        private void OnDestroy()
        {
            if (_RematchButton != null) _RematchButton.onClick.RemoveListener(OnRematch);
            if (_ExitButton != null) _ExitButton.onClick.RemoveListener(OnExit);
        }

        public void UpdateTurnIndicator(bool isPlayer1Turn)
        {
            // Clear text during gameplay
            if (_Player1Hud != null) 
                _Player1Hud.Clear();

            if (_Player2Hud != null) 
                _Player2Hud.Clear();
        }

        public void ShowWinner(bool player1Wins)
        {
            // Update individual player messages
            if (_Player1Hud != null)
                _Player1Hud.SetResult(player1Wins);

            if (_Player2Hud != null)
                _Player2Hud.SetResult(!player1Wins);

            // Show shared controls
            if (_SharedGameOverPanel != null)
                _SharedGameOverPanel.SetActive(true);
        }

        public void HideWinnerOverlay()
        {
            // Just ensure they are back to some default state or waiting for turn update
            if (_SharedGameOverPanel != null)
                _SharedGameOverPanel.SetActive(false);
        }

        private void OnRematch() => OnRematchClicked?.Invoke();
        private void OnExit() => OnExitClicked?.Invoke();
    }
}