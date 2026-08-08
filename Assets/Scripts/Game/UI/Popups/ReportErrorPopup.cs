using System;
using System.IO;
using Game.Feedback.Data;
using Game.Feedback.Events;
using Game.UI.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Game.UI.Popups
{
    public class ReportErrorPopup : BasePopup
    {
        [SerializeField] private TMP_InputField _ErrorDescriptionInput;
        [SerializeField] private Button _SubmitButton;
        [SerializeField] private Button _CancelButton;
        [SerializeField] private TextMeshProUGUI _StatusText;

        [Inject] private FeedbackEventChannel _FeedbackEventChannel;

        private ActionButtonPresenter _SubmitPresenter;
        
        public event Action OnCancelRequested;

        private void Awake()
        {
            _SubmitPresenter = ActionButtonPresenter.GetOrAdd(_SubmitButton);
            ActionButtonPresenter.GetOrAdd(_CancelButton);

            if (_SubmitButton != null)
                _SubmitButton.onClick.AddListener(OnSubmitClicked);
            
            if (_CancelButton != null)
                _CancelButton.onClick.AddListener(() => OnCancelRequested?.Invoke());
        }

        public void Clear()
        {
             if (_ErrorDescriptionInput != null)
                _ErrorDescriptionInput.text = "";
             
             if (_StatusText != null)
                _StatusText.text = "";
        }

        public void ShowStatus(string message)
        {
            if (_StatusText != null)
                _StatusText.text = message;
        }

        private void OnSubmitClicked()
        {
            string description = _ErrorDescriptionInput != null ? _ErrorDescriptionInput.text : "";
            if (string.IsNullOrWhiteSpace(description))
            {
                ShowStatus("Please enter a description.");
                PresentSubmitBlocked("report.description_required");
                return;
            }
            
            SaveReport(description);
        }

        private void SaveReport(string description)
        {
            string reportPath = Path.Combine(Application.persistentDataPath, $"error_report_{DateTime.Now.Ticks}.txt");
            try
            {
                string content = $"Date: {DateTime.Now}\nDevice: {SystemInfo.deviceModel}\nOS: {SystemInfo.operatingSystem}\n\nDescription:\n{description}";
                File.WriteAllText(reportPath, content);
                ShowStatus("Report saved successfully!");
            }
            catch (Exception e)
            {
                ShowStatus($"Failed to save report: {e.Message}");
                PresentSubmitBlocked("report.save_failed");
                Debug.LogError($"Failed to save error report: {e}");
            }
        }

        private void PresentSubmitBlocked(string cueId)
        {
            _SubmitPresenter = ActionButtonPresenter.GetOrAdd(_SubmitButton, _FeedbackEventChannel);
            _SubmitPresenter?.PresentBlocked(new FeedbackEventData(
                FeedbackKind.Blocked,
                cueId,
                "report_error"));
        }
    }
}
