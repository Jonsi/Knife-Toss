using UnityEngine;

namespace Common.Analytics
{
    /// <summary>
    /// Controls mock analytics log formatting.
    /// </summary>
    [CreateAssetMenu(fileName = "AnalyticsSettings", menuName = "Knife Toss/Settings/Analytics Settings")]
    public class AnalyticsSettings : ScriptableObject
    {
        private const string DefaultPrefix = "[Analytics]";

        [SerializeField] private string _LogPrefix = DefaultPrefix;
        [SerializeField] private Color _LogColor = new Color(0.2f, 0.85f, 1f, 1f);

        public string LogPrefix => string.IsNullOrWhiteSpace(_LogPrefix) ? DefaultPrefix : _LogPrefix;

        public Color LogColor => _LogColor;
    }
}
