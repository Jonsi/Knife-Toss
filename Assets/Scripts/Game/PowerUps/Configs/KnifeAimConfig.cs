using Game.PowerUps.Data;
using Game.PowerUps.Interfaces;
using UnityEngine;

namespace Game.PowerUps.Configs
{
    /// <summary>
    /// Configuration for the Knife Aim power-up that allows aiming knives toward input position.
    /// </summary>
    [CreateAssetMenu(fileName = "KnifeAimConfig", menuName = "Knife Toss/Power Ups/Knife Aim Config")]
    public class KnifeAimConfig : PowerUpConfigBase
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

        private Vector2 CalculateAimDirection(PowerUpThrowContext throwContext)
        {
            var pointer = UnityEngine.InputSystem.Pointer.current;
            if (pointer == null)
                return Vector2.up;

            Vector2 screenPosition = pointer.position.ReadValue();
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
                return Vector2.up;

            Vector3 screenPosWithZ = new Vector3(screenPosition.x, screenPosition.y, mainCamera.transform.position.z);
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenPosWithZ);
            Vector2 spawnPosition = throwContext.SpawnPoint.position;
            Vector2 direction = ((Vector2)worldPosition - spawnPosition).normalized;

            if (direction.sqrMagnitude < 0.01f)
                return Vector2.up;

            return direction;
        }
    }
}
