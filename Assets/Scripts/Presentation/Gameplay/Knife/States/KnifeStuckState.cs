using UnityEngine;

namespace Presentation.Presentation.Gameplay.Knife.States
{
    /// <summary>
    /// State when knife is stuck in a target.
    /// </summary>
    public class KnifeStuckState : KnifeStateBase
    {
        public KnifeStuckState(KnifeView knifeView) : base(knifeView) { }

        public override void Enter()
        {
            Speed = 0f;
            Velocity = Vector2.zero;
        }

        public override void Update(float deltaTime)
        {
        }
    }
}

