using System;
using Game.PowerUps.Configs;
using Game.Entities.Knife.Views;
using UnityEngine;

namespace Game.Pooling
{
    /// <summary>
    /// Prefab entry for knives, optionally tied to a specific power-up configuration.
    /// </summary>
    [Serializable]
    public class KnifePrefabEntry : PrefabEntry
    {
        public KnifeView GetKnifePrefab()
        {
            if (Prefab == null)
                return null;

            return Prefab.GetComponent<KnifeView>();
        }
    }
}
