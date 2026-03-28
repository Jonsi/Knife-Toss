using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Common.Analytics.Interfaces;
using UnityEngine;

namespace Common.Analytics.Services
{
    /// <summary>
    /// Logger-backed analytics implementation used until a real backend is connected.
    /// </summary>
    public class MockAnalyticsService : IAnalyticsService
    {
        private readonly ILogger _logger;
        private readonly AnalyticsSettings _settings;

        public MockAnalyticsService(ILogger logger, AnalyticsSettings settings)
        {
            _logger = logger ?? Debug.unityLogger;
            _settings = settings;
        }

        public void TrackEvent(AnalyticsEvent analyticsEvent)
        {
            if (analyticsEvent == null)
            {
                return;
            }

            _logger.Log(FormatMessage(analyticsEvent));
        }

        private string FormatMessage(AnalyticsEvent analyticsEvent)
        {
            string prefix = _settings != null ? _settings.LogPrefix : "[Analytics]";
            string parameters = FormatParameters(analyticsEvent.Parameters);
            string message = string.IsNullOrEmpty(parameters)
                ? $"{prefix} {analyticsEvent.Name}"
                : $"{prefix} {analyticsEvent.Name} | {parameters}";

            string colorHex = ColorUtility.ToHtmlStringRGBA(
                _settings != null ? _settings.LogColor : Color.cyan);

            return $"<color=#{colorHex}>{message}</color>";
        }

        private static string FormatParameters(IReadOnlyDictionary<string, object> parameters)
        {
            if (parameters == null || parameters.Count == 0)
            {
                return string.Empty;
            }

            return string.Join(", ", parameters.Select(FormatParameter));
        }

        private static string FormatParameter(KeyValuePair<string, object> parameter)
        {
            return $"{parameter.Key}={FormatValue(parameter.Value)}";
        }

        private static string FormatValue(object value)
        {
            if (value == null)
            {
                return "null";
            }

            if (value is bool boolean)
            {
                return boolean ? "true" : "false";
            }

            if (value is IFormattable formattable)
            {
                return formattable.ToString(null, CultureInfo.InvariantCulture);
            }

            return value.ToString();
        }
    }
}
