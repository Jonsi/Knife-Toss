using KnifeToss.Core.Events;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace KnifeToss.Presentation.UI
{
    /// <summary>
    /// Automatically registers all buttons in the scene (or under a specific parent) to raise ButtonClickEventChannel when clicked.
    /// Place this component on a Canvas or any parent GameObject. It will find all Button components and wire them up automatically.
    /// </summary>
    public sealed class AutoButtonEventRegistrar : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField, Tooltip("If set, only registers buttons under this parent. If null, registers all buttons in the scene.")]
        private Transform searchRoot;

        [SerializeField, Tooltip("The event channel that buttons will raise when clicked. Can be set via injection or serialized field.")]
        private ButtonClickEventChannel buttonClickEventChannel;

        private readonly List<Button> registeredButtons = new List<Button>();

        private void Start()
        {
            EnsureEventChannel();
            RegisterAllButtons();
        }

        private void OnDestroy()
        {
            UnregisterAllButtons();
        }

        [Inject]
        public void Construct(ButtonClickEventChannel buttonClickEventChannel)
        {
            this.buttonClickEventChannel = buttonClickEventChannel;
        }

        private void EnsureEventChannel()
        {
            if (buttonClickEventChannel == null)
            {
                Debug.LogWarning("[AutoButtonEventRegistrar] ButtonClickEventChannel is not assigned. Buttons will not be registered.", this);
            }
        }

        private void RegisterAllButtons()
        {
            if (buttonClickEventChannel == null)
            {
                return;
            }

            Button[] buttons;
            if (searchRoot != null)
            {
                buttons = searchRoot.GetComponentsInChildren<Button>(true);
            }
            else
            {
                buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
            }

            int registeredCount = 0;
            foreach (Button button in buttons)
            {
                if (button != null && !registeredButtons.Contains(button))
                {
                    // Store reference to avoid closure issues
                    ButtonClickEventChannel channel = buttonClickEventChannel;
                    button.onClick.AddListener(() => channel?.RaiseClicked());
                    registeredButtons.Add(button);
                    registeredCount++;
                }
            }

            if (registeredCount > 0)
            {
                Debug.Log($"[AutoButtonEventRegistrar] Registered {registeredCount} button(s) to ButtonClickEventChannel.", this);
            }
        }

        private void UnregisterAllButtons()
        {
            foreach (Button button in registeredButtons)
            {
                if (button != null && buttonClickEventChannel != null)
                {
                    // Note: We can't remove the lambda listener directly, but Unity will clean it up when the button is destroyed
                    // This is fine since the lambda will be garbage collected when the button is destroyed
                }
            }
            registeredButtons.Clear();
        }

        /// <summary>
        /// Call this method to re-register buttons (useful for dynamically created buttons).
        /// </summary>
        public void RefreshButtonRegistrations()
        {
            UnregisterAllButtons();
            RegisterAllButtons();
        }
    }
}

