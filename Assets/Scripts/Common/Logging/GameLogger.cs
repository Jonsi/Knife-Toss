using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Common.Logging
{
    /// <summary>
    /// Shared logger implementation with a static facade for convenient access.
    /// </summary>
    public class GameLogger : ILogger
    {
        private static ILogger _sharedLogger = Debug.unityLogger;
        private ILogHandler _logHandler;

        public GameLogger(ILogHandler logHandler)
        {
            _logHandler = logHandler ?? throw new ArgumentNullException(nameof(logHandler));
            filterLogType = LogType.Log;
            logEnabled = true;
        }

        public ILogHandler logHandler
        {
            get => _logHandler;
            set => _logHandler = value ?? throw new ArgumentNullException(nameof(value));
        }

        public bool logEnabled { get; set; }

        public LogType filterLogType { get; set; }

#if TEST_ENABLED
        public static void SetSharedLogger(ILogger logger)
        {
            _sharedLogger = logger ?? Debug.unityLogger;
        }

        public static void Log(object message)
        {
            Current.Log(message);
        }

        public static void Log(string tag, object message)
        {
            Current.Log(tag, message);
        }

        public static void Log(string tag, object message, Object context)
        {
            Current.Log(tag, message, context);
        }

        public static void Log(LogType logType, object message)
        {
            Current.Log(logType, message);
        }

        public static void Log(LogType logType, object message, Object context)
        {
            Current.Log(logType, message, context);
        }

        public static void LogWarning(object message)
        {
            Current.LogWarning(null, message);
        }

        public static void LogWarning(string tag, object message)
        {
            Current.LogWarning(tag, message);
        }

        public static void LogWarning(string tag, object message, Object context)
        {
            Current.LogWarning(tag, message, context);
        }

        public static void LogError(object message)
        {
            Current.LogError(null, message);
        }

        public static void LogError(string tag, object message)
        {
            Current.LogError(tag, message);
        }

        public static void LogError(string tag, object message, Object context)
        {
            Current.LogError(tag, message, context);
        }

        public static void LogException(Exception exception)
        {
            Current.LogException(exception);
        }
#else
        public static void SetSharedLogger(ILogger logger)
        {
        }

        public static void Log(object message)
        {
        }

        public static void Log(string tag, object message)
        {
        }

        public static void Log(string tag, object message, Object context)
        {
        }

        public static void Log(LogType logType, object message)
        {
        }

        public static void Log(LogType logType, object message, Object context)
        {
        }

        public static void LogWarning(object message)
        {
        }

        public static void LogWarning(string tag, object message)
        {
        }

        public static void LogWarning(string tag, object message, Object context)
        {
        }

        public static void LogError(object message)
        {
        }

        public static void LogError(string tag, object message)
        {
        }

        public static void LogError(string tag, object message, Object context)
        {
        }

        public static void LogException(Exception exception)
        {
        }
#endif

        public bool IsLogTypeAllowed(LogType logType)
        {
            if (!logEnabled)
            {
                return false;
            }

            if (logType == LogType.Exception)
            {
                return true;
            }

            return filterLogType != LogType.Exception && logType <= filterLogType;
        }

        void ILogger.Log(LogType logType, object message)
        {
            LogInternal(logType, null, message, null);
        }

        void ILogger.Log(LogType logType, object message, Object context)
        {
            LogInternal(logType, null, message, context);
        }

        void ILogger.Log(LogType logType, string tag, object message)
        {
            LogInternal(logType, tag, message, null);
        }

        void ILogger.Log(LogType logType, string tag, object message, Object context)
        {
            LogInternal(logType, tag, message, context);
        }

        void ILogger.Log(object message)
        {
            LogInternal(LogType.Log, null, message, null);
        }

        void ILogger.Log(string tag, object message)
        {
            LogInternal(LogType.Log, tag, message, null);
        }

        void ILogger.Log(string tag, object message, Object context)
        {
            LogInternal(LogType.Log, tag, message, context);
        }

        void ILogger.LogWarning(string tag, object message)
        {
            LogInternal(LogType.Warning, tag, message, null);
        }

        void ILogger.LogWarning(string tag, object message, Object context)
        {
            LogInternal(LogType.Warning, tag, message, context);
        }

        void ILogger.LogError(string tag, object message)
        {
            LogInternal(LogType.Error, tag, message, null);
        }

        void ILogger.LogError(string tag, object message, Object context)
        {
            LogInternal(LogType.Error, tag, message, context);
        }

        void ILogger.LogFormat(LogType logType, string format, params object[] args)
        {
            LogFormatInternal(logType, null, format, args);
        }

        void ILogger.LogException(Exception exception)
        {
            LogExceptionInternal(exception, null);
        }

        void ILogHandler.LogFormat(LogType logType, Object context, string format, params object[] args)
        {
            LogFormatInternal(logType, context, format, args);
        }

        void ILogHandler.LogException(Exception exception, Object context)
        {
            LogExceptionInternal(exception, context);
        }

        private void LogInternal(LogType logType, string tag, object message, Object context)
        {
            if (!IsLogTypeAllowed(logType))
            {
                return;
            }

            _logHandler.LogFormat(logType, context, "{0}", FormatMessage(tag, message));
        }

        private void LogFormatInternal(LogType logType, Object context, string format, params object[] args)
        {
            if (!IsLogTypeAllowed(logType))
            {
                return;
            }

            _logHandler.LogFormat(logType, context, format, args);
        }

        private void LogExceptionInternal(Exception exception, Object context)
        {
            if (!IsLogTypeAllowed(LogType.Exception))
            {
                return;
            }

            _logHandler.LogException(exception, context);
        }

        private static string FormatMessage(string tag, object message)
        {
            string text = message?.ToString() ?? "null";
            return string.IsNullOrEmpty(tag) ? text : $"[{tag}] {text}";
        }

        private static ILogger Current => _sharedLogger ?? Debug.unityLogger;
    }
}
