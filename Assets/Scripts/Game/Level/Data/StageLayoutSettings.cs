using System;
using UnityEngine;

namespace KnifeToss.Game.Level.Data
{
    /// <summary>
    /// Visual settings for a stage layout (backgrounds, sprites).
    /// </summary>
    [Serializable]
    public class StageLayoutSettings
    {
        [Tooltip("Sprite used for the background.")]
        [field: SerializeField] public Sprite BackgroundSprite { get; private set; }
        
        [Tooltip("Sprite used for targets in this stage.")]
        [field: SerializeField] public Sprite TargetSprite { get; private set; }
    }
}

