using Game.Knife.Interfaces;
using UnityEngine;

namespace Game.Knife.States
{
    /// <summary>
    /// State when knife is stuck in a target.
    /// </summary>
    public class KnifeStuckState : KnifeStateBase
    {
        public KnifeStuckState(IKnifeViewContext knifeView) : base(knifeView) { }

        public override void Enter()
        {
            speed = 0f;
            velocity = Vector2.zero;
            knifeView.SetColliderEnabled(true);
        }

        public override void Update(float deltaTime)
        {
        }
    }
}

