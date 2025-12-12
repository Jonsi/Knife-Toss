using System;

namespace Game.Persistence.Data
{
    /// <summary>
    /// Represents player settings preferences stored between sessions.
    /// </summary>
    [Serializable]
    public class PlayerSettingsData
    {
        public bool IsMuted;
    }
}
