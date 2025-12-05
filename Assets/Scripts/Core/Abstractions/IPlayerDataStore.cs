using Core.Data;

namespace Core.Abstractions
{
    /// <summary>
    /// Low-level persistence boundary for player data.
    /// </summary>
    public interface IPlayerDataStore
    {
        PlayerData LoadOrCreate();
        void Save(PlayerData data);
        void Delete();
    }
}
