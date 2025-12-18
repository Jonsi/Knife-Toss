using System.Collections.Generic;
using UnityEngine;

namespace Game.PowerUps.Configs
{
    /// <summary>
    /// Catalog containing all available power-up configurations.
    /// </summary>
    [CreateAssetMenu(fileName = "PowerUpCatalog", menuName = "Knife Toss/Power Ups/Power Up Catalog")]
    public class PowerUpCatalog : ScriptableObject
    {
        [field: SerializeField] public List<PowerUpConfigBase> Entries { get; private set; } = new List<PowerUpConfigBase>();
    }
}
