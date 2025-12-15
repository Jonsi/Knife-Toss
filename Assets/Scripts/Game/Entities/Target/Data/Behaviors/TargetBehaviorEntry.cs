using System;
using UnityEngine;

namespace Game.Entities.Target.Data.Behaviors
{
    /// <summary>
    /// Entry wrapping a local behavior configuration.
    /// </summary>
    [Serializable]
    public class TargetLocalBehaviorEntry : UnityEngine.ISerializationCallbackReceiver
    {
        [UnityEngine.SerializeField] private TargetLocalBehaviorKind _Kind = TargetLocalBehaviorKind.ConstantSpin;
        [SerializeReference] private object _Config = new Local.ConstantSpinBehaviorConfig();

        public ILocalBehaviorConfig Config => _Config as ILocalBehaviorConfig;

        public static TargetLocalBehaviorEntry From(ILocalBehaviorConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            return new TargetLocalBehaviorEntry
            {
                _Kind = config.Kind,
                _Config = TargetBehaviorEntryCloner.CloneConfig(config)
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
                return;
            }

            _Config = TargetBehaviorEntryCloner.CloneConfig(_Config);
        }
    }

    /// <summary>
    /// Entry wrapping a world behavior configuration.
    /// </summary>
    [Serializable]
    public class TargetWorldBehaviorEntry : UnityEngine.ISerializationCallbackReceiver
    {
        [UnityEngine.SerializeField] private TargetWorldBehaviorKind _Kind = TargetWorldBehaviorKind.Orbit;
        [SerializeReference] private object _Config = new World.OrbitBehaviorConfig();

        public IWorldBehaviorConfig Config => _Config as IWorldBehaviorConfig;

        public static TargetWorldBehaviorEntry From(IWorldBehaviorConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            return new TargetWorldBehaviorEntry
            {
                _Kind = config.Kind,
                _Config = TargetBehaviorEntryCloner.CloneConfig(config)
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
                return;
            }

            _Config = TargetBehaviorEntryCloner.CloneConfig(_Config);
        }

    }

    internal static class TargetBehaviorEntryCloner
    {
        public static object CloneConfig(object source)
        {
            if (source == null)
                return null;

            string json = JsonUtility.ToJson(source);
            return JsonUtility.FromJson(json, source.GetType());
        }
    }
}
