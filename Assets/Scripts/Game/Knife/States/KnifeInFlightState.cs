using KnifeToss.Game.Knife.Interfaces;
using UnityEngine;

namespace KnifeToss.Game.Knife.States
{
    /// <summary>
    /// State when knife is in flight after being thrown.
    /// </summary>
    public class KnifeInFlightState : KnifeStateBase
    {
        public KnifeInFlightState(IKnifeViewContext knifeView) : base(knifeView) { }

        public void Launch(Vector2 direction, float speed)
        {
            SetMovement(direction, speed);
        }

        public override void Enter()
        {
            base.Enter();
            // Ensure collider is active while in flight
            knifeView.SetColliderEnabled(true);
        }
    }
}

