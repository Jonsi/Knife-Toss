using KnifeToss.Game.Knife.Interfaces;
using UnityEngine;

namespace KnifeToss.Game.Knife.States
{
    /// <summary>
    /// State when knife has bounced and is falling.
    /// </summary>
    public class KnifeBouncedState : KnifeStateBase
    {
        private const float GRAVITY = -20f;
        private float _AngularVelocity;
        private Vector2 _InitialVelocity = new Vector2(0f, 6f);

        public KnifeBouncedState(IKnifeViewContext knifeView) : base(knifeView) { }

        public void ConfigureBounce(Vector2 initialVelocity, float angularVelocity)
        {
            _InitialVelocity = initialVelocity;
            _AngularVelocity = angularVelocity;
        }

        public override void Enter()
        {
            base.Enter();
            knifeView.Transform.SetParent(null);
            knifeView.SetColliderEnabled(false);
            velocity = _InitialVelocity;
        }

        public override void Update(float deltaTime)
        {
            velocity += new Vector2(0f, GRAVITY * deltaTime);
            knifeView.Transform.position += (Vector3)(velocity * deltaTime);
            knifeView.Transform.Rotate(0f, 0f, _AngularVelocity * deltaTime);
        }
    }
}

