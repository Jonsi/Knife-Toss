using KnifeToss.Game.Knife.Interfaces;
using UnityEngine;

namespace KnifeToss.Game.Knife.States
{
    /// <summary>
    /// State when knife is stuck in a target.
    /// </summary>
    public class KnifeStuckState : KnifeStateBase
    {
        public KnifeStuckState(IKnifeViewContext knifeView) : base(knifeView) { }

        public override void Enter()
        {
            Speed = 0f;
            Velocity = Vector2.zero;
            KnifeView.SetColliderEnabled(true);
        }

        public override void Update(float deltaTime)
        {
        }
    }
}

