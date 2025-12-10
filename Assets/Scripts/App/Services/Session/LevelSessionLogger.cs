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

        private readonly ILogger _Logger;

        public LevelSessionLogger(ILogger logger)
        {
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void LogState(string currentStateName, int stageIndex, int levelIndex)
        {
            if (string.IsNullOrEmpty(currentStateName))
                return;
            
            if (currentStateName.IndexOf("Playing", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                string prefix = $"[Stage {stageIndex + 1}-{levelIndex + 1}]";
                _Logger.Log(LogType.Log, ApplyColor($"{prefix} Level started", StartedColor));
                return;
            }

            if (currentStateName.IndexOf("Completed", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                string prefix = $"[Stage {stageIndex + 1}-{levelIndex + 1}]";
                _Logger.Log(LogType.Log, ApplyColor($"{prefix} Level completed", CompletedColor));
                return;
            }

            if (currentStateName.IndexOf("Failed", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                string prefix = $"[Stage {stageIndex + 1}-{levelIndex + 1}]";
                _Logger.Log(LogType.Log, ApplyColor($"{prefix} Level failed", FailedColor));
            }
        }

        private static string ApplyColor(string message, Color color)
        {
            string hex = ColorUtility.ToHtmlStringRGBA(color);
            return $"<color=#{hex}>{message}</color>";
        }
    }
}
