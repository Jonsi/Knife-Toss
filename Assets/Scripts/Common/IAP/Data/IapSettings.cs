using UnityEngine;

namespace Common.IAP.Data
{
    /// <summary>
    /// Global IAP service settings.
    /// </summary>
    [CreateAssetMenu(fileName = "IapSettings", menuName = "Knife Toss/IAP/IAP Settings")]
    public class IapSettings : ScriptableObject
    {
        [field: SerializeField] public bool IapEnabled { get; private set; } = true;
        [field: SerializeField] public bool UseMockProvider { get; private set; } = true;
        [field: SerializeField] public float GlobalTimeoutSeconds { get; private set; } = 30f;
    }
}
