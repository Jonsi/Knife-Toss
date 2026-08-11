using UnityEngine;

namespace Game.UI.Common
{
    /// <summary>
    /// Marks a deliberately undersized action. A non-empty reason is required before the
    /// validator accepts the exception, keeping exceptions visible and reviewable.
    /// </summary>
    [AddComponentMenu("UI/Validation/Touch Target Exception")]
    [DisallowMultipleComponent]
    public sealed class TouchTargetException : MonoBehaviour
    {
        [SerializeField, TextArea] private string _Reason;

        public string Reason => string.IsNullOrWhiteSpace(_Reason) ? string.Empty : _Reason.Trim();

        public void SetReason(string reason)
        {
            _Reason = reason;
        }
    }
}
