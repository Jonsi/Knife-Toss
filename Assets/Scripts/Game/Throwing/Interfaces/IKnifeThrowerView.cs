using KnifeToss.Game.Knife.Data;

namespace KnifeToss.Game.Throwing.Interfaces
{
    /// <summary>
    /// Interface for the view component responsible for spawning knife instances.
    /// </summary>
    public interface IKnifeThrowerView
    {
        /// <summary>
        /// Spawns and launches a knife based on the provided spawn request data.
        /// </summary>
        /// <param name="spawnData">Configuration for spawn offset and throw speed.</param>
        void SpawnKnife(KnifeSpawnRequestData spawnData);
    }
}

