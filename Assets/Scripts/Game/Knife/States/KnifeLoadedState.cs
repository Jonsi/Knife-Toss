using Game.Knife.Interfaces;
using UnityEngine;

namespace Game.Knife.States
{
    /// <summary>
    /// State when knife is loaded and ready to be thrown.
    /// </summary>
    public class KnifeLoadedState : KnifeStateBase
    {
        public KnifeLoadedState(IKnifeViewContext knifeView) : base(knifeView) { }

        public override void Enter()
        {
            speed = 0f;
            velocity = Vector2.zero;
            knifeView.SetColliderEnabled(false);
            knifeView.SetActive(true);
        }
    }
}

