using UnityEngine;

namespace KnifeToss.Infrastructure.Input
{
    public interface IInputService
    {
        Vector2 ReadAimDirection();
        bool IsFirePressed();
    }

    public class InputService : MonoBehaviour, IInputService
    {
        public Vector2 ReadAimDirection() => UnityEngine.Input.mousePosition;
        public bool IsFirePressed() => UnityEngine.Input.GetMouseButtonDown(0);
    }
}

