using System;
using Common.Ads.Data;
using Common.Ads.Providers;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Ads.UI
{
    /// <summary>
    /// MonoBehaviour bridge that wires <see cref="MockAdProvider.PopupCallback"/> to <see cref="MockAdPopup"/>.
    /// Lives on the same persistent GameObject as the game's global scope (DontDestroyOnLoad) so it
    /// remains active across scene transitions and intercepts any ad show call from any scene.
    ///
    /// When <see cref="MockAdProvider"/> triggers a show, this bridge creates a
    /// <see cref="UniTaskCompletionSource{AdResult}"/>, presents the <see cref="MockAdPopup"/>,
    /// and returns the completion source to the provider. The popup resolves it when the
    /// tester taps a button or the auto-timer expires.
    ///
    /// Requires <see cref="MockAdProvider"/> and <see cref="MockAdSettings"/> to be registered
    /// in the same or parent DI scope. Use <c>[VContainer.Inject]</c> for field/method injection.
    /// </summary>
    public class MockAdPopupBridge : MonoBehaviour
    {
        [SerializeField] private MockAdPopup _Popup;
        [SerializeField] private CanvasScaler _CanvasScaler;

        private MockAdProvider _Provider;
        private MockAdSettings _Settings;

        private void Awake()
        {
            if (_CanvasScaler == null)
                _CanvasScaler = GetComponent<CanvasScaler>();

            if (_CanvasScaler == null)
                return;

            _CanvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            _CanvasScaler.referenceResolution = new Vector2(800f, 600f);
            _CanvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            _CanvasScaler.matchWidthOrHeight = 0f;
        }

        /// <summary>
        /// Receives dependencies from VContainer injection.
        /// Called before Unity's <c>Start()</c> lifecycle method.
        /// </summary>
        [VContainer.Inject]
        public void Construct(MockAdProvider provider, MockAdSettings settings)
        {
            _Provider = provider;
            _Settings = settings;
        }

        private void Start()
        {
            if (_Provider != null)
                _Provider.PopupCallback = OnAdRequested;
        }

        private UniTaskCompletionSource<AdResult> OnAdRequested(AdPlacement placement)
        {
            Debug.Log(
                $"[MockAdPopupBridge] Presenting mock ad popup. Placement={placement?.PlacementKey}, Type={placement?.GetType().Name}\n" +
                $"{Environment.StackTrace}");

            var tcs = new UniTaskCompletionSource<AdResult>();

            if (_Popup != null)
                _Popup.Present(placement, tcs, _Settings.SimulatedDelaySeconds, _Settings.DefaultOutcome);
            else
                tcs.TrySetResult(_Settings.DefaultOutcome.ToAdResult());

            return tcs;
        }

        private void OnDestroy()
        {
            if (_Provider != null)
                _Provider.PopupCallback = null;
        }
    }
}
