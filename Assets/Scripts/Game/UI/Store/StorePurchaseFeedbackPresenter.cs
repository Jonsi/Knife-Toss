using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Game.UI.Store
{
    /// <summary>
    /// Handles loading and toast feedback for store purchases.
    /// </summary>
    public class StorePurchaseFeedbackPresenter : MonoBehaviour
    {
        private const int ToastDurationMs = 1600;

        [field: SerializeField] public GameObject LoadingOverlay { get; private set; }
        [field: SerializeField] public TMP_Text ToastText { get; private set; }
        [field: SerializeField] public GameObject ToastRoot { get; private set; }

        private int _ToastVersion;

        public void ShowLoading(bool show)
        {
            if (LoadingOverlay != null)
            {
                LoadingOverlay.SetActive(show);
            }
        }

        public void ShowSuccess(string message)
        {
            ShowToast(message);
        }

        public void ShowError(string message)
        {
            ShowToast(message);
        }

        private void ShowToast(string message)
        {
            if (ToastRoot == null || ToastText == null)
            {
                return;
            }

            _ToastVersion++;
            var version = _ToastVersion;

            ToastText.text = message;
            ToastRoot.SetActive(true);
            HideToastDelayed(version).Forget();
        }

        private async UniTaskVoid HideToastDelayed(int version)
        {
            await UniTask.Delay(ToastDurationMs, DelayType.DeltaTime, PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy());
            if (_ToastVersion != version)
            {
                return;
            }

            if (ToastRoot != null)
            {
                ToastRoot.SetActive(false);
            }
        }
    }
}
