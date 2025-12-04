using UnityEngine;

namespace KnifeToss.Core.Events.Level
{
    /// <summary>
    /// Event channel for level started events.
    /// </summary>
    [CreateAssetMenu(fileName = "LevelStartedChannel", menuName = "Knife Toss/Events/Level/Level Started Channel")]
    public class LevelStartedChannel : GenericEventChannel<int>
    {
    }

    /// <summary>
    /// Event channel for level completed events.
    /// </summary>
    [CreateAssetMenu(fileName = "LevelCompletedChannel", menuName = "Knife Toss/Events/Level/Level Completed Channel")]
    public class LevelCompletedChannel : GenericEventChannel<int>
    {
    }

    /// <summary>
    /// Event channel for level failed events.
    /// </summary>
    [CreateAssetMenu(fileName = "LevelFailedChannel", menuName = "Knife Toss/Events/Level/Level Failed Channel")]
    public class LevelFailedChannel : GenericEventChannel<int>
    {
    }

    /// <summary>
    /// Event channel for level transition events.
    /// </summary>
    [CreateAssetMenu(fileName = "LevelTransitionChannel", menuName = "Knife Toss/Events/Level/Level Transition Channel")]
    public class LevelTransitionChannel : GenericEventChannel<int>
    {
    }
}

