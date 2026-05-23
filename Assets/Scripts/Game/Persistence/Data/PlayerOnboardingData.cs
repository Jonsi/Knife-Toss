using System;

namespace Game.Persistence.Data
{
    /// <summary>
    /// Tracks one-time onboarding moments already seen by the player.
    /// </summary>
    [Serializable]
    public class PlayerOnboardingData
    {
        public bool HasTappedPlayGesture;
        public bool HasTappedThrowGesture;
        public bool HasTappedSuperKnifeGesture;
        public bool HasTappedSlowTimeGesture;
    }
}
