using System;
using Game.Entities.Knife.Data;
using Game.Entities.Knife.Views;

namespace Game.KnifeThrower.Interfaces
{
    /// <summary>
    /// Interface for the view component responsible for knife positioning and launching.
    /// </summary>
    public interface IKnifeThrowerView
    {
        /// <summary>
        /// Loads a knife instance in a ready state at the spawn position.
        /// </summary>
        /// <param name="knife">The knife instance to load.</param>
        /// <param name="spawnData">Configuration for spawn offset.</param>
        void LoadKnife(KnifeView knife, KnifeSpawnRequestData spawnData);

        /// <summary>
        /// Launches the currently loaded knife with the given speed.
        /// </summary>
        /// <param name="throwSpeed">Launch speed.</param>
        void ThrowLoadedKnife(float throwSpeed);
    }
}
