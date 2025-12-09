using KnifeToss.Core.Abstractions.Gameplay.Knife;
using UnityEngine;

namespace KnifeToss.Core.Domain.Knife.States
{
    /// <summary>
    /// State when knife is loaded and ready to be thrown.
    /// </summary>
    public class KnifeLoadedState : KnifeStateBase
    {
        public KnifeLoadedState(IKnifeViewContext knifeView) : base(knifeView) { }

        public override void Enter()
        {
            Speed = 0f;
            Velocity = Vector2.zero;
            KnifeView.SetColliderEnabled(true);
            KnifeView.SetActive(true);
        }
    }
}

