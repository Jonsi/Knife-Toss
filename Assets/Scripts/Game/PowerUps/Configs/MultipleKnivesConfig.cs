using Game.PowerUps.Interfaces;
using UnityEngine;

namespace Game.PowerUps.Configs
{
    /// <summary>
    /// Configuration for the Multiple Knives power-up.
    /// </summary>
    [CreateAssetMenu(fileName = "MultipleKnivesConfig", menuName = "Knife Toss/Systems/Power Ups/Multiple Knives Config")]
    public class MultipleKnivesConfig : PowerUpConfigBase, IProjectilePowerUp, ICancellablePowerUp
    {
        [field: SerializeField, Min(1)] public int KnifeCount { get; private set; } = 3;
        [field: SerializeField, Min(0f)] public float KnifeSpacing { get; private set; } = 0.35f;
        [field: SerializeField] public float ThrowSpeed { get; private set; } = 10f;

        /// <summary>
        /// Executes the multiple knives power-up by throwing all loaded knives.
        /// </summary>
        public override bool Execute(IPowerUpExecutionContext context)
        {
            if (context?.ThrowContext == null)
                return false;

            int thrown = context.ThrowContext.ThrowerView.ThrowLoadedKnives(ThrowSpeed);
            return thrown > 0;
        }

        public void OnCancelled()
        {
            // No specific cleanup needed
        }
    }
}
