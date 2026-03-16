using System.Collections.Generic;
using Game.Entities.Knife.Data;
using Game.Entities.Knife.Views;
using UnityEngine;

namespace Game.KnifeThrower.Interfaces
{
    /// <summary>
    /// Common interface for components capable of loading and throwing projectiles.
    /// </summary>
    public interface IProjectileThrower
    {
        /// <summary>
        /// Gets the transform used for spawning projectiles.
        /// </summary>
        Transform SpawnPoint { get; }

        /// <summary>
        /// Gets the currently loaded projectile instances.
        /// </summary>
        IReadOnlyList<KnifeView> LoadedKnives { get; }

        /// <summary>
        /// Loads a single projectile instance in a ready state.
        /// </summary>
        /// <param name="knife">The projectile instance to load.</param>
        /// <param name="spawnData">Configuration for spawn offset.</param>
        void LoadKnife(KnifeView knife, KnifeSpawnRequestData spawnData);

        /// <summary>
        /// Loads multiple projectile instances in a ready state at spaced positions.
        /// </summary>
        /// <param name="knives">Projectile instances to load.</param>
        /// <param name="spawnData">Configuration for spawn offset.</param>
        /// <param name="spacing">Spacing between projectiles.</param>
        void LoadKnives(IReadOnlyList<KnifeView> knives, KnifeSpawnRequestData spawnData, float spacing);

        /// <summary>
        /// Launches the currently loaded projectiles using the configured loaded speed.
        /// </summary>
        void ThrowLoadedKnife();

        /// <summary>
        /// Launches all currently loaded projectiles using the configured loaded speed.
        /// </summary>
        /// <returns>Number of projectiles launched.</returns>
        int ThrowLoadedKnives();

        /// <summary>
        /// Launches the currently loaded projectiles with the given direction and configured loaded speed.
        /// </summary>
        /// <param name="direction">Launch direction.</param>
        void ThrowLoadedKnifeWithDirection(Vector2 direction);

        /// <summary>
        /// Sets whether the loaded projectiles should be visible.
        /// </summary>
        /// <param name="visible">True to show, false to hide.</param>
        void SetVisible(bool visible);
    }
}
