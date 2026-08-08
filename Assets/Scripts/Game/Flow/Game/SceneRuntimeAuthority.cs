using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.Flow.Game
{
    /// <summary>
    /// Defines the scene-local components that can contend during additive scene handoff.
    /// A transition suspends the outgoing scene before revealing the incoming scene.
    /// </summary>
    internal static class SceneRuntimeAuthority
    {
        public static SceneRuntimeAuthoritySuspension Suspend(Scene scene)
        {
            var disabledBehaviours = new List<Behaviour>();
            if (!scene.IsValid() || !scene.isLoaded)
            {
                return new SceneRuntimeAuthoritySuspension(disabledBehaviours);
            }

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (Behaviour behaviour in root.GetComponentsInChildren<Behaviour>(true))
                {
                    if (behaviour == null || !behaviour.enabled || !IsControlledBehaviour(behaviour))
                    {
                        continue;
                    }

                    disabledBehaviours.Add(behaviour);
                    behaviour.enabled = false;
                }
            }

            return new SceneRuntimeAuthoritySuspension(disabledBehaviours);
        }

        public static bool IsControlledBehaviour(Behaviour behaviour)
        {
            return behaviour is Camera
                || behaviour is AudioListener
                || behaviour is Canvas
                || behaviour is GraphicRaycaster
                || behaviour is EventSystem
                || behaviour is BaseInputModule;
        }
    }

    internal sealed class SceneRuntimeAuthoritySuspension
    {
        private readonly IReadOnlyList<Behaviour> _DisabledBehaviours;
        private bool _WasRestored;

        public SceneRuntimeAuthoritySuspension(IReadOnlyList<Behaviour> disabledBehaviours)
        {
            _DisabledBehaviours = disabledBehaviours;
        }

        public void Restore()
        {
            if (_WasRestored)
            {
                return;
            }

            _WasRestored = true;
            for (int i = 0; i < _DisabledBehaviours.Count; i++)
            {
                Behaviour behaviour = _DisabledBehaviours[i];
                if (behaviour != null)
                {
                    behaviour.enabled = true;
                }
            }
        }
    }
}
