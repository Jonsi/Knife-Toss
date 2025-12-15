using System;

namespace Game.Entities.Target.Data.Behaviors
{
    /// <summary>
    /// Serializable entry that links a behavior kind and its configuration.
    /// </summary>
    [Serializable]
    public class TargetBehaviorEntry : UnityEngine.ISerializationCallbackReceiver
    {
        [UnityEngine.SerializeField] private bool _Enabled = true;
        [UnityEngine.SerializeField] private TargetBehaviorKind _Kind = TargetBehaviorKind.ConstantSpin;
        [UnityEngine.SerializeField, UnityEngine.HideInInspector] private TargetBehaviorKind _LastKind = TargetBehaviorKind.ConstantSpin;
        [UnityEngine.SerializeReference] private TargetBehaviorConfig _Config = new Local.ConstantSpinBehaviorConfig();

        /// <summary>
        /// Gets a value indicating whether the behavior is active.
        /// </summary>
        public bool Enabled => _Enabled;

        /// <summary>
        /// Gets the selected behavior kind.
        /// </summary>
        public TargetBehaviorKind Kind => _Kind;

        /// <summary>
        /// Gets the configuration object.
        /// </summary>
        public TargetBehaviorConfig Config => _Config;

        /// <summary>
        /// Creates an entry from an existing configuration.
        /// </summary>
        /// <param name="config">Configuration instance.</param>
        /// <returns>Behavior entry.</returns>
        public static TargetBehaviorEntry From(TargetBehaviorConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            return new TargetBehaviorEntry
            {
                _Enabled = true,
                _Kind = config.Kind,
                _Config = config
            };
        }

        void UnityEngine.ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            EnsureConfigType();
        }

        void UnityEngine.ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            EnsureConfigType();
        }

        /// <summary>
        /// Ensures configuration type matches the selected kind.
        /// </summary>
        internal void Sync()
        {
            EnsureConfigType();
        }

        private void EnsureConfigType()
        {
            if (_Config != null && _LastKind == _Kind)
                return;

            Type configType = TargetBehaviorMetadata.GetConfigType(_Kind);
            if (_Config == null || _Config.GetType() != configType)
            {
                _Config = (TargetBehaviorConfig)Activator.CreateInstance(configType);
            }

            _LastKind = _Kind;
        }
    }
}
