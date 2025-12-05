using Core.Abstractions;

namespace Infrastructure.Persistence
{
    /// <summary>
    /// Simple static access point for runtime player data service.
    /// </summary>
    public static class PlayerDataLocator
    {
        public static IPlayerDataService Runtime { get; internal set; }
    }
}
