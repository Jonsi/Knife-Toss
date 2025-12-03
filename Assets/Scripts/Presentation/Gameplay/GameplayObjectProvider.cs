using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace KnifeToss.Presentation.Gameplay
{



    public sealed class GameplayObjectProvider
    {
        private readonly GameObject targetPrefab;
        private readonly GameObject knifeThrowerPrefab;
        private readonly Transform runtimeRoot;
        private readonly IObjectResolver resolver;

        private KnifeThrower throwerInstance;
        private TargetGroupManager targetGroupManager;

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

        public RotatingTarget GetTarget()
        {
            TargetGroupManager group = GetTargetGroupManager();
            return group != null ? group.PrimaryTarget?.GetComponent<RotatingTarget>() : null;
        }

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
            resolver.InjectGameObject(instance);
            EnsureKnifeThrowRelay(instance);
            throwerInstance = instance.GetComponent<KnifeThrower>();
            if (throwerInstance == null)
            {
                throw new System.InvalidOperationException("KnifeThrower prefab must contain a KnifeThrower component.");
            }
            return throwerInstance;
        }

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

        private static void EnsureKnifeThrowRelay(GameObject host)
        {
            if (host.GetComponent<KnifeThrowRelay>() == null)
            {
                host.AddComponent<KnifeThrowRelay>();
            }
        }
    }
}
