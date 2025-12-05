using UnityEngine;

namespace Presentation.Gameplay
{
    /// <summary>
    /// Keeps selected rotation axes fixed for the attached transform.
    /// Useful for UI children that should not spin with their parent.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AxisRotationLocker : MonoBehaviour
    {
        [SerializeField] private bool lockX;
        [SerializeField] private bool lockY;
        [SerializeField] private bool lockZ = true;
        [SerializeField] private bool useLocalRotation;

        private Vector3 lockedEulerAngles;

        private void Awake()
        {
            CacheLockedAngles();
        }

        private void OnEnable()
        {
            CacheLockedAngles();
            ApplyLock();
        }

        private void LateUpdate()
        {
            ApplyLock();
        }

        private void CacheLockedAngles()
        {
            lockedEulerAngles = useLocalRotation ? transform.localEulerAngles : transform.eulerAngles;
        }

        private void ApplyLock()
        {
            Vector3 euler = useLocalRotation ? transform.localEulerAngles : transform.eulerAngles;

            if (lockX)
            {
                euler.x = lockedEulerAngles.x;
            }

            if (lockY)
            {
                euler.y = lockedEulerAngles.y;
            }

            if (lockZ)
            {
                euler.z = lockedEulerAngles.z;
            }

            if (useLocalRotation)
            {
                transform.localRotation = Quaternion.Euler(euler);
            }
            else
            {
                transform.rotation = Quaternion.Euler(euler);
            }
        }
    }
}
