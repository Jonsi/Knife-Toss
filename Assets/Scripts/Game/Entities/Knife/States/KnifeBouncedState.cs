using Game.Entities.Knife.Interfaces;
using UnityEngine;

namespace Game.Entities.Knife.States
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
            KnifeView.Transform.SetParent(null);
            KnifeView.SetColliderEnabled(false);
            Velocity = _InitialVelocity;
        }

        public override void Update(float deltaTime)
        {
            Velocity += new Vector2(0f, GRAVITY * deltaTime);
            KnifeView.Transform.position += (Vector3)(Velocity * deltaTime);
            KnifeView.Transform.Rotate(0f, 0f, _AngularVelocity * deltaTime);
        }
    }
}

