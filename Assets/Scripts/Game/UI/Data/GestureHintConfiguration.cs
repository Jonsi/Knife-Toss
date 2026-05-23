using UnityEngine;

namespace Game.UI.Data
{
    /// <summary>
    /// Holds tuned UI positions for gesture hints.
    /// </summary>
    [CreateAssetMenu(fileName = "GestureHintConfiguration", menuName = "Knife Toss/UI/Gesture Hint Configuration")]
    public class GestureHintConfiguration : ScriptableObject
    {
        [field: SerializeField] public Vector2 TapOnPlayPosition { get; private set; }
        [field: SerializeField] public Vector2 TapToThrowPosition { get; private set; }
        [field: SerializeField] public Vector2 TapSuperKnifePowerUpPosition { get; private set; }
        [field: SerializeField] public Vector2 TapSlowTimePowerUpPosition { get; private set; }
    }
}
