using Common.Patterns.State;
using Game.Knife.Interfaces;
using UnityEngine;

namespace Game.Knife.States
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
            this.KnifeView = knifeView;
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
            this.Speed = speed;
            Velocity = direction.normalized * speed;
        }
    }
}

