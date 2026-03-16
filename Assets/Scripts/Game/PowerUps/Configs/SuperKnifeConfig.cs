using Game.PowerUps.Interfaces;
using UnityEngine;

namespace Game.PowerUps.Configs
{
    /// <summary>
    /// Configuration for the Super Knife power-up.
    /// </summary>
    [CreateAssetMenu(fileName = "SuperKnifeConfig", menuName = "Knife Toss/Systems/Power Ups/Super Knife Config")]
    public class SuperKnifeConfig : PowerUpConfigBase, IProjectilePowerUp, ICancellablePowerUp
    {
        [field: SerializeField] public GameObject VfxPrefab { get; private set; }
        [field: SerializeField] public Vector3 VfxLocalOffset { get; private set; } = Vector3.zero;
        [field: SerializeField] public Vector3 VfxLocalScale { get; private set; } = Vector3.one;
        [field: SerializeField] public int VfxSortingOrderOffset { get; private set; } = 2;

        /// <summary>
        /// Executes the super knife power-up by throwing the loaded knife.
        /// </summary>
        public override bool Execute(IPowerUpExecutionContext context)
        {
            if (context?.ThrowContext == null)
                return false;

            context.ThrowContext.ThrowerView.ThrowLoadedKnife();
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
