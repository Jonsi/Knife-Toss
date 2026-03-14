namespace Game.UI.Interfaces
{
    /// <summary>
    /// Applies and releases gameplay pause requests from UI layers.
    /// </summary>
    public interface IGamePauseController
    {
        void AcquirePause(object requester);
        void ReleasePause(object requester);
    }
}
