using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Presentation.Gameplay
{



    public sealed class GameplayObjectProvider
    {
        private readonly GameObject targetPrefab;
        private readonly GameObject knifeThrowerPrefab;
        private readonly Transform runtimeRoot;
        private readonly IObjectResolver resolver;

        private KnifeThrower throwerInstance;
        private TargetGroupManager targetGroupManager;

        /// <summary>
        /// Performs the GameplayObjectProvider operation.
        /// </summary>
        public GameplayObjectProvider(
            GameObject targetPrefab,
            GameObject knifeThrowerPrefab,
            Transform lifetimeTransform,
            IObjectResolver resolver)
        {
            this.targetPrefab = targetPrefab;
            this.knifeThrowerPrefab = knifeThrowerPrefab;
            this.resolver = resolver;

            runtimeRoot = new GameObject("GameplayRoot").transform;
            runtimeRoot.SetParent(lifetimeTransform, false);
        }

        /// <summary>
        /// Performs the GetTarget operation.
        /// </summary>
        public RotatingTarget GetTarget()
        {
            TargetGroupManager group = GetTargetGroupManager();
            return group != null ? group.PrimaryTarget?.GetComponent<RotatingTarget>() : null;
        }

        /// <summary>
        /// Performs the GetKnifeThrower operation.
        /// </summary>
        public KnifeThrower GetKnifeThrower()
        {
            if (throwerInstance != null)
                return throwerInstance;

            if (knifeThrowerPrefab == null)
            {
                throw new System.InvalidOperationException("KnifeThrower prefab is not assigned.");
            }

            GameObject instance = Object.Instantiate(knifeThrowerPrefab, runtimeRoot);
            instance.name = "KnifeThrowerInstance";
            EnsureKnifeThrowRelay(instance);
            resolver.InjectGameObject(instance);
            throwerInstance = instance.GetComponent<KnifeThrower>();
            if (throwerInstance == null)
            {
                throw new System.InvalidOperationException("KnifeThrower prefab must contain a KnifeThrower component.");
            }
            return throwerInstance;
        }

        /// <summary>
        /// Performs the GetTargetGroupManager operation.
        /// </summary>
        public TargetGroupManager GetTargetGroupManager()
        {
            if (targetGroupManager != null)
            {
                return targetGroupManager;
            }

            GameObject container = new GameObject("TargetGroupManager");
            container.transform.SetParent(runtimeRoot, false);
            targetGroupManager = container.AddComponent<TargetGroupManager>();
            resolver.InjectGameObject(container);
            targetGroupManager.Initialize(targetPrefab, resolver);
            return targetGroupManager;
        }

        /// <summary>
        /// Performs the EnsureKnifeThrowRelay operation.
        /// </summary>
        private static void EnsureKnifeThrowRelay(GameObject host)
        {
            if (host.GetComponent<KnifeThrowRelay>() == null)
            {
                host.AddComponent<KnifeThrowRelay>();
            }
        }
    }
}
