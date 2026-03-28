using System;
using System.Collections.Generic;

namespace Common.Analytics
{
    /// <summary>
    /// Immutable analytics event payload.
    /// </summary>
    public sealed class AnalyticsEvent
    {
        private static readonly IReadOnlyDictionary<string, object> EmptyParameters =
            new Dictionary<string, object>();

        public AnalyticsEvent(string name, IReadOnlyDictionary<string, object> parameters = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Analytics event name must be provided.", nameof(name));
            }

            Name = name;
            Parameters = parameters ?? EmptyParameters;
        }

        public string Name { get; }

        public IReadOnlyDictionary<string, object> Parameters { get; }
    }
}
