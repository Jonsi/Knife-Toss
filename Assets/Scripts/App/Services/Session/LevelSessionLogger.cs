using System;
using UnityEngine;

namespace App.Services.Session
{
    /// <summary>
    /// Emits level session state logs with minimal rich text coloring.
    /// </summary>
    public class LevelSessionLogger
    {
        private static readonly Color StartedColor = new Color(0.07f, 0.73f, 1f);
        private static readonly Color CompletedColor = new Color(0.27f, 0.85f, 0.27f);
        private static readonly Color FailedColor = new Color(0.95f, 0.36f, 0.36f);

        private readonly ILogger logger;

        public LevelSessionLogger(ILogger logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void LogState(string currentStateName)
        {
            if (string.IsNullOrEmpty(currentStateName))
                return;

            if (currentStateName.IndexOf("Playing", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                logger.Log(LogType.Log, ApplyColor("Level started", StartedColor));
                return;
            }

            if (currentStateName.IndexOf("Completed", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                logger.Log(LogType.Log, ApplyColor("Level completed", CompletedColor));
                return;
            }

            if (currentStateName.IndexOf("Failed", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                logger.Log(LogType.Log, ApplyColor("Level failed", FailedColor));
            }
        }

        private static string ApplyColor(string message, Color color)
        {
            string hex = ColorUtility.ToHtmlStringRGBA(color);
            return $"<color=#{hex}>{message}</color>";
        }
    }
}

