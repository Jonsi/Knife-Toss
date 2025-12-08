using UnityEngine;

namespace Core.Data
{
    /// <summary>
    /// Visual layout settings for a stage, including background and target textures.
    /// </summary>
    [CreateAssetMenu(fileName = "StageLayoutSettings", menuName = "Knife Toss/Configurations/Gameplay/Stage Layout Settings")]
    public class StageLayoutSettings : ScriptableObject
    {
        [Header("Textures")]
        [Tooltip("Background texture for this stage. Applied to the background RawImage.")]
        public Texture2D backgroundTexture;

        [Tooltip("Target texture/sprite for this stage. Applied to all target SpriteRenderers.")]
        public Sprite targetSprite;
    }
}

