using System;
using System.Globalization;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Common.Logging
{
    /// <summary>
    /// Applies rich-text color formatting to selected log types before forwarding to Unity.
    /// </summary>
    public class ColoredLogHandler : ILogHandler
    {
        private readonly ILogHandler _inner;

        public ColoredLogHandler(ILogHandler inner)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        public void LogFormat(LogType logType, Object context, string format, params object[] args)
        {
#if TEST_ENABLED
            if (TryGetColor(logType, out string color))
            {
                string message = args == null || args.Length == 0
                    ? format
                    : string.Format(CultureInfo.InvariantCulture, format, args);

                _inner.LogFormat(logType, context, "{0}", WrapWithColor(message, color));
                return;
            }
#endif

            _inner.LogFormat(logType, context, format, args);
        }

        public void LogException(Exception exception, Object context)
        {
            _inner.LogException(exception, context);
        }

        private static string WrapWithColor(string message, string color)
        {
            return $"<color={color}>{message}</color>";
        }

        private static bool TryGetColor(LogType logType, out string color)
        {
            switch (logType)
            {
                case LogType.Log:
                    color = "#7DD3FC";
                    return true;
                case LogType.Warning:
                    color = "#FCD34D";
                    return true;
                case LogType.Error:
                case LogType.Assert:
                case LogType.Exception:
                    color = "#FCA5A5";
                    return true;
                default:
                    color = null;
                    return false;
            }
        }
    }
}
