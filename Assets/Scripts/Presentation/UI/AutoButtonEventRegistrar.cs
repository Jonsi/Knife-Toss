using KnifeToss.Core.Events;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace KnifeToss.Presentation.UI
{




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


                }
            }
            registeredButtons.Clear();
        }




        public void RefreshButtonRegistrations()
        {
            UnregisterAllButtons();
            RegisterAllButtons();
        }
    }
}

