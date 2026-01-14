using Game.PowerUps.Interfaces;
using UnityEngine;

namespace Game.PowerUps.Configs
{
    /// <summary>
    /// Configuration for the Super Knife power-up.
    /// </summary>
    [CreateAssetMenu(fileName = "SuperKnifeConfig", menuName = "Knife Toss/Power Ups/Super Knife Config")]
    public class SuperKnifeConfig : PowerUpConfigBase
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
    }
}
