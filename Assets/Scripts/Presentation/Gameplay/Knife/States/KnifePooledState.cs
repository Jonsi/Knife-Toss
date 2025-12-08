using UnityEngine;

namespace Presentation.Gameplay.Knife.States
{
    /// <summary>
    /// State when knife is pooled and inactive.
    /// </summary>
    public class KnifePooledState : KnifeStateBase
    {
        public KnifePooledState(KnifeView knifeView) : base(knifeView) { }

        public override void Enter()
        {
            Speed = 0f;
            Velocity = Vector2.zero;
            KnifeView.gameObject.SetActive(false);
        }

        public override void Update(float deltaTime)
        {
        }
    }
}

