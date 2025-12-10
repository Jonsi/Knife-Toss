using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Common.Logging
{
    /// <summary>
    /// Delegates log handling while allowing test-environment customization.
    /// </summary>
    public class TestEnvLogHandler : ILogHandler
    {
#if TEST_ENV
        private readonly ILogHandler inner;
        private readonly Func<bool> isEnabled;
        private readonly bool includeStackTrace;

        public TestEnvLogHandler(ILogHandler inner, Func<bool> isEnabled, bool includeStackTrace)
        {
            this.inner = inner ?? throw new ArgumentNullException(nameof(inner));
            this.isEnabled = isEnabled ?? (() => false);
            this.includeStackTrace = includeStackTrace;
        }

        public void LogException(Exception exception, Object context)
        {
            if (ShouldLog())
            {
                inner.LogException(exception, context);
            }
        }

        public void LogFormat(LogType logType, Object context, string format, params object[] args)
        {
            if (!ShouldLog())
            {
                return;
            }

            string effectiveFormat = includeStackTrace ? $"{format}\n{Environment.StackTrace}" : format;
            inner.LogFormat(logType, context, effectiveFormat, args);
        }

        private bool ShouldLog() => isEnabled();
#else
        public TestEnvLogHandler(ILogHandler inner, Func<bool> isEnabled, bool includeStackTrace)
        {
        }

        public void LogException(Exception exception, Object context)
        {
        }

        public void LogFormat(LogType logType, Object context, string format, params object[] args)
        {
        }
#endif
    }
}