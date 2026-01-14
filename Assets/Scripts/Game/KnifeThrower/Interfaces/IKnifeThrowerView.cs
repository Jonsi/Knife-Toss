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
        /// Loads multiple knife instances in a ready state at spaced positions.
        /// </summary>
        /// <param name="knives">Knife instances to load.</param>
        /// <param name="spawnData">Configuration for spawn offset.</param>
        /// <param name="spacing">Spacing between knives on the X axis.</param>
        void LoadKnives(System.Collections.Generic.IReadOnlyList<KnifeView> knives, KnifeSpawnRequestData spawnData, float spacing);

        /// <summary>
        /// Launches the currently loaded knife with the given speed.
        /// </summary>
        /// <param name="throwSpeed">Launch speed.</param>
        void ThrowLoadedKnife(float throwSpeed);

        /// <summary>
        /// Launches all currently loaded knives with the given speed.
        /// </summary>
        /// <param name="throwSpeed">Launch speed.</param>
        /// <returns>Number of knives launched.</returns>
        int ThrowLoadedKnives(float throwSpeed);

        /// <summary>
        /// Launches the currently loaded knife with the given direction and speed.
        /// </summary>
        /// <param name="direction">Launch direction.</param>
        /// <param name="throwSpeed">Launch speed.</param>
        void ThrowLoadedKnifeWithDirection(UnityEngine.Vector2 direction, float throwSpeed);

        /// <summary>
        /// Gets the transform used for spawning knives.
        /// </summary>
        UnityEngine.Transform SpawnPoint { get; }

        /// <summary>
        /// Gets the currently loaded knife instance.
        /// </summary>
        Game.Entities.Knife.Views.KnifeView LoadedKnife { get; }

        /// <summary>
        /// Gets the currently loaded knife instances.
        /// </summary>
        System.Collections.Generic.IReadOnlyList<Game.Entities.Knife.Views.KnifeView> LoadedKnives { get; }
    }
}
