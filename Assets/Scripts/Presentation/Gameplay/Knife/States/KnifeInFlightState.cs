using UnityEngine;

namespace Presentation.Gameplay.Knife.States
{
    /// <summary>
    /// State when knife is in flight after being thrown.
    /// </summary>
    public class KnifeInFlightState : KnifeStateBase
    {
        public KnifeInFlightState(KnifeView knifeView) : base(knifeView) { }

        public override void Enter()
        {
            base.Enter();
        }

        public void Launch(Vector2 direction, float speed)
        {
            SetMovement(direction, speed);
        }
    }
}

