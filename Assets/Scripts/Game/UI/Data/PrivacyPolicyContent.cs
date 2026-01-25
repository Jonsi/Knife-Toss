using UnityEngine;

namespace Game.UI.Data
{
    [CreateAssetMenu(fileName = "PrivacyPolicyContent", menuName = "Knife Toss/UI/Privacy Policy Content")]
    public class PrivacyPolicyContent : ScriptableObject
    {
        [TextArea(10, 50)]
        public string PolicyText;
    }
}
