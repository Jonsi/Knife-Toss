using KnifeToss.Core.Abstractions.Gameplay.Knife;
using KnifeToss.Core.Abstractions.State;
using UnityEngine;

namespace KnifeToss.Core.Domain.Knife.States
{
    /// <summary>
    /// Base state for knife behavior that tracks velocity and updates position.
    /// </summary>
    public abstract class KnifeStateBase : IState
    {
        protected readonly IKnifeViewContext KnifeView;
        protected Vector2 Velocity;
        protected float Speed;

        protected KnifeStateBase(IKnifeViewContext knifeView)
        {
            KnifeView = knifeView;
        }

        public virtual void Enter() { }
        
        public virtual void Update(float deltaTime)
        {
            if (Speed > 0f)
            {
                KnifeView.Transform.position += (Vector3)(Velocity * deltaTime);
            }
        }
        
        public virtual void Exit() { }

        protected void SetMovement(Vector2 direction, float speed)
        {
            Speed = speed;
            Velocity = direction.normalized * speed;
        }
    }
}

