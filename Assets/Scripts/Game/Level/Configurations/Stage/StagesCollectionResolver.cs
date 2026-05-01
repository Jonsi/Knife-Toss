using Game.Flow.Game;
using Game.Level.Catalogs;

namespace Game.Level.Configurations.Stage
{
    /// <summary>
    /// Resolves the active stage collection from session state.
    /// </summary>
    public class StagesCollectionResolver
    {
        private readonly StagesCollectionComposite _Composite;
        private readonly GameData _GameData;

        /// <summary>
        /// Creates a resolver for the composite and current session data.
        /// </summary>
        public StagesCollectionResolver(StagesCollectionComposite composite, GameData gameData)
        {
            _Composite = composite;
            _GameData = gameData;
        }

        /// <summary>
        /// Gets the stage collection for the current session world index.
        /// </summary>
        public StagesCollection GetActiveCollection()
        {
            return _Composite?.GetCollection(_GameData.CurrentCollectionIndex);
        }
    }
}
