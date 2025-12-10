using KnifeToss.Game.Knife.Interfaces;
using UnityEngine;

namespace KnifeToss.Game.Knife.States
{
    /// <summary>
    /// State when knife is pooled and inactive.
    /// </summary>
    public class KnifePooledState : KnifeStateBase
    {
        public KnifePooledState(IKnifeViewContext knifeView) : base(knifeView) { }

        public override void Enter()
        {
            Speed = 0f;
            Velocity = Vector2.zero;
            KnifeView.SetColliderEnabled(false);
            KnifeView.SetActive(false);
        }

        public override void Update(float deltaTime)
        {
        }
    }
}

