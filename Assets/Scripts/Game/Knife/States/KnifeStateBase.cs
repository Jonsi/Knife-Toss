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
        protected readonly IKnifeViewContext knifeView;
        protected Vector2 velocity;
        protected float speed;

        protected KnifeStateBase(IKnifeViewContext knifeView)
        {
            this.knifeView = knifeView;
        }

        public virtual void Enter() { }
        
        public virtual void Update(float deltaTime)
        {
            if (speed > 0f)
            {
                knifeView.Transform.position += (Vector3)(velocity * deltaTime);
            }
        }
        
        public virtual void Exit() { }

        protected void SetMovement(Vector2 direction, float speed)
        {
            this.speed = speed;
            velocity = direction.normalized * speed;
        }
    }
}

