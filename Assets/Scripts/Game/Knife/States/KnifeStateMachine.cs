using KnifeToss.Common.State;

namespace KnifeToss.Game.Knife.States
{
    /// <summary>
    /// State machine for knife states; composes and owns all knife state instances.
    /// </summary>
    public class KnifeStateMachine : StateMachine
    {
        public readonly KnifeLoadedState loaded;
        public readonly KnifeInFlightState inFlight;
        public readonly KnifeStuckState stuck;
        public readonly KnifeBouncedState bounced;
        public readonly KnifePooledState pooled;

        public KnifeStateMachine(KnifeLoadedState loaded, KnifeInFlightState inFlight, KnifeStuckState stuck, KnifeBouncedState bounced, KnifePooledState pooled)
        {
            this.loaded = loaded;
            this.inFlight = inFlight;
            this.stuck = stuck;
            this.bounced = bounced;
            this.pooled = pooled;

            SetState(this.loaded);
        }

        public bool IsInFlight => ReferenceEquals(CurrentState, inFlight);
    }
}

