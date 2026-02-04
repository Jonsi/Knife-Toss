using Game.PowerUps.Data;
using Game.PowerUps.Interfaces;
using UnityEngine;

namespace Game.PowerUps.Configs
{
    /// <summary>
    /// Configuration for the Knife Aim power-up that allows aiming knives toward input position.
    /// </summary>
    [CreateAssetMenu(fileName = "KnifeAimConfig", menuName = "Knife Toss/Power Ups/Knife Aim Config")]
    public class KnifeAimConfig : PowerUpConfigBase, IProjectilePowerUp, ICancellablePowerUp
    {
        [field: SerializeField] public float ThrowSpeed { get; private set; } = 10f;

        /// <summary>
        /// Executes the knife aim power-up by throwing the loaded knife toward the input position.
        /// </summary>
        public override bool Execute(IPowerUpExecutionContext context)
        {
            if (context?.ThrowContext == null)
                return false;

            var throwContext = context.ThrowContext;
            Vector2 aimDirection = CalculateAimDirection(throwContext);
            throwContext.ThrowerView.ThrowLoadedKnifeWithDirection(aimDirection, ThrowSpeed);
            return true;
        }

        public void OnCancelled()
        {
            // No specific cleanup needed
        }

        private Vector2 CalculateAimDirection(PowerUpThrowContext throwContext)
        {
            var loadedKnives = throwContext.ThrowerView.LoadedKnives;
            if (loadedKnives != null && loadedKnives.Count > 0 && loadedKnives[0] != null)
            {
                // Use the current rotation of the knife which was set during Tick in ThrowService
                return loadedKnives[0].transform.up;
            }

            return Vector2.up;
        }
    }
}
