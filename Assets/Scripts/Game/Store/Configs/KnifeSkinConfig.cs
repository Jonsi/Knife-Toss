using UnityEngine;

namespace Game.Store.Configs
{
    /// <summary>
    /// Definition asset for a knife skin.
    /// Only the stable Id is persisted in player data.
    /// </summary>
    [CreateAssetMenu(fileName = "KnifeSkinConfig", menuName = "Knife Toss/Systems/Store/Knife Skin Config")]
    public class KnifeSkinConfig : ScriptableObject
    {
        [field: SerializeField] public string Id { get; private set; }
        [field: SerializeField] public Sprite Icon { get; private set; }
    }
}
