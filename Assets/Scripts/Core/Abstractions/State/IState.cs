namespace KnifeToss.Core.Abstractions.State
{
    /// <summary>
    /// Generic state contract for state machine-driven logic.
    /// </summary>
    public interface IState
    {
        void Enter();
        void Update(float deltaTime);
        void Exit();
    }
}

