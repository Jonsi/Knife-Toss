using System;

namespace KnifeToss.Presentation.Gameplay
{
    public interface IThrowInput
    {
        event Action Performed;
        void Enable();
        void Disable();
    }
}
