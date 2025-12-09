using KnifeToss.Core.Domain.Knife.States;
using KnifeToss.Core.Domain;

namespace KnifeToss.Core.Domain.Knife
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
            Loaded = loaded;
            InFlight = inFlight;
            Stuck = stuck;
            Bounced = bounced;
            Pooled = pooled;

            SetState(Loaded);
        }

        public bool IsInFlight => ReferenceEquals(CurrentState, InFlight);
    }
}

