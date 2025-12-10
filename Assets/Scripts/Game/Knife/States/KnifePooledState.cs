using Game.Knife.Interfaces;
using UnityEngine;

namespace Game.Knife.States
{
    /// <summary>
    /// State when knife is pooled and inactive.
    /// </summary>
    public class KnifePooledState : KnifeStateBase
    {
        public KnifePooledState(IKnifeViewContext knifeView) : base(knifeView) { }

        public override void Enter()
        {
            speed = 0f;
            velocity = Vector2.zero;
            knifeView.SetColliderEnabled(false);
            knifeView.SetActive(false);
        }

        public override void Update(float deltaTime)
        {
        }
    }
}

