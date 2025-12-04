using System;
using UnityEngine;

namespace KnifeToss.Core.Data
{
    /// <summary>
    /// Placement settings for a hitable object type on a target.
    /// </summary>
    [Serializable]
    public sealed class HitableObjectPlacement
    {
        [Tooltip("Which hitable object configuration to spawn.")]
        public HitableObjectConfig config;

        [Tooltip("How many of this object to spawn around the target.")]
        public int count = 0;

        [Tooltip("Extra distance added on top of the target radius when placing the object.")]
        public float radialOffset = 0.35f;

        [Tooltip("Angular spacing in degrees between spawned objects.")]
        public float angularSpacing = 60f;

        [Tooltip("Rotate the prefab so it faces away from the target center.")]
        public bool orientOutwards = true;
    }
}
