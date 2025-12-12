using Game.Persistence.Data;

namespace Game.Persistence.Interfaces
{
    /// <summary>
    /// Provides save and load operations for player data persistence.
    /// </summary>
    public interface IPlayerDataStore
    {
        PlayerData Load();
        void Save(PlayerData data);
    }
}
