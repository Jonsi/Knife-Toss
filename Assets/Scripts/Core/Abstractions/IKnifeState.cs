namespace KnifeToss.Core.Abstractions
{
    /// <summary>
    /// Base interface for knife behavior states in the state machine.
    /// </summary>
    public interface IKnifeState
    {
        /// <summary>
        /// Called when entering this state.
        /// </summary>
        void Enter();
        
        /// <summary>
        /// Called every frame while in this state.
        /// </summary>
        void Update(float deltaTime);
        
        /// <summary>
        /// Called when exiting this state.
        /// </summary>
        void Exit();
    }
}

