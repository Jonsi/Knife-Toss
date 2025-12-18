using System.Collections.Generic;
using Game.Entities.Collectible.Views;

namespace Game.PowerUps.Services
{
    /// <summary>
    /// Tracks active collectibles for power-up execution.
    /// </summary>
    public class CollectibleRegistry
    {
        private readonly List<CollectibleView> _Items = new List<CollectibleView>();

        /// <summary>
        /// Adds a collectible to the registry.
        /// </summary>
        public void Register(CollectibleView collectible)
        {
            if (collectible == null)
                return;

            if (_Items.Contains(collectible))
                return;

            _Items.Add(collectible);
        }

        /// <summary>
        /// Removes a collectible from the registry.
        /// </summary>
        public void Unregister(CollectibleView collectible)
        {
            if (collectible == null)
                return;

            _Items.Remove(collectible);
        }

        /// <summary>
        /// Gets all registered collectibles.
        /// </summary>
        public IReadOnlyList<CollectibleView> GetAll()
        {
            return _Items;
        }
    }
}
