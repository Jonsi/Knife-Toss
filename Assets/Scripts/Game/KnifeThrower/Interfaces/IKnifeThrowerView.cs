using System;
using Game.Entities.Knife.Data;

namespace Game.KnifeThrower.Interfaces
{
    /// <summary>
    /// Interface for the view component responsible for spawning knife instances.
    /// </summary>
    public interface IKnifeThrowerView
    {
        /// <summary>
        /// Loads a knife instance in a ready state based on spawn data.
        /// </summary>
        /// <param name="spawnData">Configuration for spawn offset and throw speed.</param>
        void LoadKnife(KnifeSpawnRequestData spawnData);

        /// <summary>
        /// Launches the currently loaded knife with the given speed.
        /// </summary>
        /// <param name="throwSpeed">Launch speed.</param>
        void ThrowLoadedKnife(float throwSpeed);

        /// <summary>
        /// Event raised when a knife is spawned.
        /// </summary>
        event Action<KnifeSpawnRequestData> OnKnifeSpawned;
    }
}
