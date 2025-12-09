namespace KnifeToss.Core.Data.Level
{
    /// <summary>
    /// Payload for communicating a level state transition.
    /// </summary>
    public struct LevelStateChangedEventData
    {
        public string PreviousState;
        public string CurrentState;

        public LevelStateChangedEventData(string previous, string current)
        {
            PreviousState = previous;
            CurrentState = current;
        }
    }
}

