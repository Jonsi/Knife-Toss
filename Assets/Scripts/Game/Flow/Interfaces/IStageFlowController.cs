using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Flow.Interfaces
{
    /// <summary>
    /// Interface for controlling the stage flow downward from the Game layer.
    /// </summary>
    public interface IStageFlowController
    {
        /// <summary>
        /// Starts the stage at the given collection and local stage indices after loading the world via Addressables.
        /// </summary>
        /// <param name="collectionIndex">World (stage collection) index.</param>
        /// <param name="stageIndex">Stage index within that collection.</param>
        /// <param name="cancellationToken">Cancellation for the load.</param>
        UniTask LoadStage(int collectionIndex, int stageIndex, CancellationToken cancellationToken = default);
    }
}
