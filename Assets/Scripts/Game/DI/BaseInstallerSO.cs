using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.DI
{
    /// <summary>
    /// Base class for ScriptableObject installers that own their dependencies.
    /// </summary>
    public abstract class BaseInstallerSO : ScriptableObject, IInstaller
    {
        public abstract void Install(IContainerBuilder builder);
    }
}
