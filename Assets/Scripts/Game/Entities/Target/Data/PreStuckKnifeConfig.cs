using System;
using UnityEngine;

namespace Game.Entities.Target.Data
{
    /// <summary>
    /// Configuration for knives that start embedded in a target.
    /// </summary>
    [Serializable]
    public class PreStuckKnifeConfig
    {
        /// <summary>
        /// Gets the angle on the target where the knife is placed.
        /// </summary>
        [field: SerializeField] public float Angle { get; private set; }
    }
}
