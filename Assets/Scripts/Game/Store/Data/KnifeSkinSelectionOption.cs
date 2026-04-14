using Game.Store.Configs;
using UnityEngine;

namespace Game.Store.Data
{
    /// <summary>
    /// Read-only view model for a knife skin option that can be shown in the main menu selector.
    /// </summary>
    public readonly struct KnifeSkinSelectionOption
    {
        public KnifeSkinSelectionOption(string skinId, string displayName, Sprite previewSprite, KnifeSkinConfig config = null)
        {
            SkinId = skinId ?? string.Empty;
            DisplayName = displayName ?? string.Empty;
            PreviewSprite = previewSprite;
            Config = config;
        }

        public string SkinId { get; }
        public string DisplayName { get; }
        public Sprite PreviewSprite { get; }
        public KnifeSkinConfig Config { get; }

        public bool IsDefault => string.IsNullOrEmpty(SkinId);
    }
}
