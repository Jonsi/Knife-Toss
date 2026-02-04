using Game.PowerUps.Interfaces;
using UnityEngine;

namespace Game.PowerUps.Configs
{
    /// <summary>
    /// Configuration for the Super Knife power-up.
    /// </summary>
    [CreateAssetMenu(fileName = "SuperKnifeConfig", menuName = "Knife Toss/Power Ups/Super Knife Config")]
    public class SuperKnifeConfig : PowerUpConfigBase, IProjectilePowerUp, ICancellablePowerUp
    {
        [field: SerializeField] public float ThrowSpeed { get; private set; } = 10f;

        /// <summary>
        /// Executes the super knife power-up by throwing the loaded knife.
        /// </summary>
        public override bool Execute(IPowerUpExecutionContext context)
        {
            if (context?.ThrowContext == null)
                return false;

            context.ThrowContext.ThrowerView.ThrowLoadedKnife(ThrowSpeed);
            return true;
        }

        public override void PrepareKnives(System.Collections.Generic.IReadOnlyList<Game.Entities.Knife.Views.KnifeView> knives)
        {
            if (knives == null) return;
            foreach (var knife in knives)
            {
                if (knife != null) knife.IsSuperKnife = true;
            }
        }

        public void OnCancelled()
        {
            // No specific cleanup needed for super knife
        }
    }
}
