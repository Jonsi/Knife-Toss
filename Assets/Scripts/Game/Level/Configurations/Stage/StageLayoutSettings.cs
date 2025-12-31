using System;
using UnityEngine;

namespace Game.Level.Data
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

        [Tooltip("Icon to display for this stage.")]
        [field: SerializeField] public Sprite StageIcon { get; private set; }
    }
}

