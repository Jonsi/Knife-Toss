using System;

namespace Game.Entities.Target.Data.Behaviors
{
    /// <summary>
    /// Entry wrapping a local behavior configuration.
    /// </summary>
    [Serializable]
    public class TargetLocalBehaviorEntry : UnityEngine.ISerializationCallbackReceiver
    {
        [UnityEngine.SerializeField] private bool _Enabled = true;
        [UnityEngine.SerializeField] private TargetLocalBehaviorKind _Kind = TargetLocalBehaviorKind.ConstantSpin;
        [UnityEngine.SerializeReference] private object _Config = new Local.ConstantSpinBehaviorConfig();

        public bool Enabled => _Enabled;
        public ILocalBehaviorConfig Config => _Config as ILocalBehaviorConfig;

        public static TargetLocalBehaviorEntry From(ILocalBehaviorConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            return new TargetLocalBehaviorEntry
            {
                _Enabled = true,
                _Kind = config.Kind,
                _Config = config
            };
        }

        public void Sync()
        {
            EnsureConfigType();
        }

        void UnityEngine.ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            EnsureConfigType();
        }

        void UnityEngine.ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            EnsureConfigType();
        }

        private void EnsureConfigType()
        {
            Type configType = TargetBehaviorMetadata.GetLocalConfigType(_Kind);
            if (_Config == null || _Config.GetType() != configType)
            {
                _Config = Activator.CreateInstance(configType);
            }
        }
    }

    /// <summary>
    /// Entry wrapping a world behavior configuration.
    /// </summary>
    [Serializable]
    public class TargetWorldBehaviorEntry : UnityEngine.ISerializationCallbackReceiver
    {
        [UnityEngine.SerializeField] private bool _Enabled = true;
        [UnityEngine.SerializeField] private TargetWorldBehaviorKind _Kind = TargetWorldBehaviorKind.Orbit;
        [UnityEngine.SerializeReference] private object _Config = new World.OrbitBehaviorConfig();

        public bool Enabled => _Enabled;
        public IWorldBehaviorConfig Config => _Config as IWorldBehaviorConfig;

        public static TargetWorldBehaviorEntry From(IWorldBehaviorConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            return new TargetWorldBehaviorEntry
            {
                _Enabled = true,
                _Kind = config.Kind,
                _Config = config
            };
        }

        public void Sync()
        {
            EnsureConfigType();
        }

        void UnityEngine.ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            EnsureConfigType();
        }

        void UnityEngine.ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            EnsureConfigType();
        }

        private void EnsureConfigType()
        {
            Type configType = TargetBehaviorMetadata.GetWorldConfigType(_Kind);
            if (_Config == null || _Config.GetType() != configType)
            {
                _Config = Activator.CreateInstance(configType);
            }
        }
    }
}
