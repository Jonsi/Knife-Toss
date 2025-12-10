namespace KnifeToss.Game.Level.Data
{
    /// <summary>
    /// Payload for communicating a level state transition.
    /// </summary>
    public struct LevelStateChangedEventData
    {
        public string previousState;
        public string currentState;

        public LevelStateChangedEventData(string previous, string current)
        {
            previousState = previous;
            currentState = current;
        }
    }
}

