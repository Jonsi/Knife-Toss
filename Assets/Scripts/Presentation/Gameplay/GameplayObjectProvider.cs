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

        private RotatingTarget targetInstance;
        private KnifeThrower throwerInstance;

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
            if (targetInstance != null)
                return targetInstance;

            if (targetPrefab == null)
            {
                throw new System.InvalidOperationException("Target prefab is not assigned.");
            }

            GameObject instance = Object.Instantiate(targetPrefab, runtimeRoot);
            instance.name = "TargetInstance";
            resolver.InjectGameObject(instance);
            targetInstance = instance.GetComponent<RotatingTarget>();
            if (targetInstance == null)
            {
                throw new System.InvalidOperationException("Target prefab must contain a RotatingTarget component.");
            }
            return targetInstance;
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

        private static void EnsureKnifeThrowRelay(GameObject host)
        {
            if (host.GetComponent<KnifeThrowRelay>() == null)
            {
                host.AddComponent<KnifeThrowRelay>();
            }
        }
    }
}
