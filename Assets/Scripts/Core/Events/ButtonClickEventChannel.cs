using System;
using UnityEngine;

namespace KnifeToss.Core.Events
{
    [CreateAssetMenu(fileName = "ButtonClickEventChannel", menuName = "Knife Toss/Events/Button Click Channel")]
    public sealed class ButtonClickEventChannel : ScriptableObject
    {
        public event Action Clicked;

        public void RaiseClicked()
        {
            Clicked?.Invoke();
        }
    }
}
