using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Flow.Game
{
    [DefaultExecutionOrder(-10000)]
    public sealed class DeferredGameSceneVisibility : MonoBehaviour
    {
        [SerializeField] private GameObject[] _RootsToHide;

        private readonly List<BehaviourState> _HiddenBehaviours = new();
        private bool _IsHidden;

        private void Awake()
        {
            if (!DeferredGameSceneVisibilityState.ShouldHideStagePlayOnLoad)
            {
                return;
            }

            SetRootsVisible(false);
            _IsHidden = true;
            DeferredGameSceneVisibilityState.RegisterLoadedGate(this);
        }

        public static void BeginHiddenLoad()
        {
            DeferredGameSceneVisibilityState.BeginHiddenLoad();
        }

        public static void RevealLoadedScene()
        {
            DeferredGameSceneVisibilityState.RevealLoadedScene();
        }

        public static UniTask WaitForReveal()
        {
            return DeferredGameSceneVisibilityState.WaitForReveal();
        }

        public static void Reset()
        {
            DeferredGameSceneVisibilityState.Reset();
        }

        public void Reveal()
        {
            if (!_IsHidden)
            {
                return;
            }

            SetRootsVisible(true);
            _IsHidden = false;
        }

        private void SetRootsVisible(bool isVisible)
        {
            if (_RootsToHide == null)
            {
                return;
            }

            if (!isVisible)
            {
                _HiddenBehaviours.Clear();

                foreach (GameObject root in _RootsToHide)
                {
                    if (root == null)
                    {
                        continue;
                    }

                    foreach (Behaviour behaviour in root.GetComponentsInChildren<Behaviour>(true))
                    {
                        if (behaviour == null || !behaviour.enabled ||
                            !SceneRuntimeAuthority.IsControlledBehaviour(behaviour))
                        {
                            continue;
                        }

                        _HiddenBehaviours.Add(new BehaviourState(behaviour));
                        behaviour.enabled = false;
                    }
                }

                return;
            }

            for (int i = 0; i < _HiddenBehaviours.Count; i++)
            {
                _HiddenBehaviours[i].Restore();
            }

            _HiddenBehaviours.Clear();
        }
    }

    internal readonly struct BehaviourState
    {
        private readonly Behaviour _Behaviour;

        public BehaviourState(Behaviour behaviour)
        {
            _Behaviour = behaviour;
        }

        public void Restore()
        {
            if (_Behaviour != null)
            {
                _Behaviour.enabled = true;
            }
        }
    }

    internal static class DeferredGameSceneVisibilityState
    {
        private static DeferredGameSceneVisibility _LoadedGate;
        private static UniTaskCompletionSource _RevealTcs;

        public static bool ShouldHideStagePlayOnLoad { get; private set; }

        public static void BeginHiddenLoad()
        {
            ShouldHideStagePlayOnLoad = true;
            _LoadedGate = null;
            _RevealTcs = new UniTaskCompletionSource();
        }

        public static void RegisterLoadedGate(DeferredGameSceneVisibility gate)
        {
            if (!ShouldHideStagePlayOnLoad)
            {
                return;
            }

            _LoadedGate = gate;
        }

        public static void RevealLoadedScene()
        {
            _LoadedGate?.Reveal();
            _RevealTcs?.TrySetResult();
            Reset();
        }

        public static UniTask WaitForReveal()
        {
            if (!ShouldHideStagePlayOnLoad || _RevealTcs == null)
            {
                return UniTask.CompletedTask;
            }

            return _RevealTcs.Task;
        }

        public static void Reset()
        {
            ShouldHideStagePlayOnLoad = false;
            _LoadedGate = null;
            _RevealTcs = null;
        }
    }
}
