using Game.Level.Catalogs;
using Game.Level.Services;

namespace Game.Level.Configurations.Stage
{
    /// <summary>
    /// Resolves the active stage collection from the addressable-loaded world service.
    /// </summary>
    public class StagesCollectionResolver
    {
        private readonly WorldAssetService _WorldAssetService;

        /// <summary>
        /// Creates a resolver bound to the world asset service.
        /// </summary>
        public StagesCollectionResolver(WorldAssetService worldAssetService)
        {
            _WorldAssetService = worldAssetService;
        }

        /// <summary>
        /// Gets the stage collection for the session after the matching world has been loaded.
        /// </summary>
        public StagesCollection GetActiveCollection()
        {
            return _WorldAssetService?.ActiveCollection;
        }
    }
}
