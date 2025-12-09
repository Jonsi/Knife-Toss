using Core.Abstractions;
using UnityEngine;

namespace Presentation.Gameplay.Knife.States
{
    /// <summary>
    /// Base state for knife behavior that tracks velocity and updates position.
    /// </summary>
    public abstract class KnifeStateBase : IKnifeState
    {
        protected readonly KnifeView KnifeView;
        protected Vector2 Velocity;
        protected float Speed;

        protected KnifeStateBase(KnifeView knifeView)
        {
            KnifeView = knifeView;
        }

        public virtual void Enter() { }
        
        public virtual void Update(float deltaTime)
        {
            if (Speed > 0f)
            {
                KnifeView.transform.position += (Vector3)(Velocity * deltaTime);
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

