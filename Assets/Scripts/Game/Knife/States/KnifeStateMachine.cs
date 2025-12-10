using Common.Patterns.State;

namespace Game.Knife.States
{
    /// <summary>
    /// State machine for knife states; composes and owns all knife state instances.
    /// </summary>
    public class KnifeStateMachine : StateMachine
    {
        public readonly KnifeLoadedState Loaded;
        public readonly KnifeInFlightState InFlight;
        public readonly KnifeStuckState Stuck;
        public readonly KnifeBouncedState Bounced;
        public readonly KnifePooledState Pooled;

        public KnifeStateMachine(KnifeLoadedState loaded, KnifeInFlightState inFlight, KnifeStuckState stuck, KnifeBouncedState bounced, KnifePooledState pooled)
        {
            this.Loaded = loaded;
            this.InFlight = inFlight;
            this.Stuck = stuck;
            this.Bounced = bounced;
            this.Pooled = pooled;

            SetState(this.Loaded);
        }

        public bool IsInFlight => ReferenceEquals(CurrentState, InFlight);
    }
}

