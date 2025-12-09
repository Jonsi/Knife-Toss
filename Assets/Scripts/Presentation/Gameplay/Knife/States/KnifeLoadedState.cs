using UnityEngine;

namespace Presentation.Gameplay.Knife.States
{
    /// <summary>
    /// State when knife is loaded and ready to be thrown.
    /// </summary>
    public class KnifeLoadedState : KnifeStateBase
    {
        public KnifeLoadedState(KnifeView knifeView) : base(knifeView) { }

        public override void Enter()
        {
            Speed = 0f;
            Velocity = Vector2.zero;
        }
    }
}

