using KnifeToss.Core.Abstractions.Gameplay.Knife;
using UnityEngine;

namespace KnifeToss.Core.Domain.Knife.States
{
    /// <summary>
    /// State when knife has bounced and is falling.
    /// </summary>
    public class KnifeBouncedState : KnifeStateBase
    {
        private const float Gravity = -20f;
        private float _angularVelocity;
        private Vector2 _initialVelocity = new Vector2(0f, 6f);

        public KnifeBouncedState(IKnifeViewContext knifeView) : base(knifeView) { }

        public void ConfigureBounce(Vector2 initialVelocity, float angularVelocity)
        {
            _initialVelocity = initialVelocity;
            _angularVelocity = angularVelocity;
        }

        public override void Enter()
        {
            base.Enter();
            KnifeView.Transform.SetParent(null);
            KnifeView.SetColliderEnabled(false);
            Velocity = _initialVelocity;
        }

        public override void Update(float deltaTime)
        {
            Velocity += new Vector2(0f, Gravity * deltaTime);
            KnifeView.Transform.position += (Vector3)(Velocity * deltaTime);
            KnifeView.Transform.Rotate(0f, 0f, _angularVelocity * deltaTime);
        }
    }
}

