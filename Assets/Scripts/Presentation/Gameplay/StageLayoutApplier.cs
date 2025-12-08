using Core.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Presentation.Gameplay
{
    /// <summary>
    /// Applies stage layout settings (background and target textures) when stages change.
    /// </summary>
    public sealed class StageLayoutApplier
    {
        private readonly RawImage backgroundImage;
        private TargetGroupManager targetGroupManager;

        public StageLayoutApplier(RawImage background)
        {
            backgroundImage = background;
        }

        public void SetTargetGroupManager(TargetGroupManager manager)
        {
            targetGroupManager = manager;
        }

        /// <summary>
        /// Applies layout settings for the given stage.
        /// </summary>
        public void ApplyLayout(StageLayoutSettings settings)
        {
            if (settings == null)
            {
                return;
            }

            if (settings.backgroundTexture != null && backgroundImage != null)
            {
                backgroundImage.texture = settings.backgroundTexture;
            }
        }

        /// <summary>
        /// Applies target texture to all active targets.
        /// </summary>
        public void ApplyTargetTexture(Sprite sprite)
        {
            targetGroupManager?.ApplySpriteToAllTargets(sprite);
        }
    }
}

