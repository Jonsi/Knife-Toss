using KnifeToss.Application.Services;
using KnifeToss.Core.Abstractions;
using KnifeToss.Core.Data;
using KnifeToss.Core.Domain;
using KnifeToss.Core.Events;
using KnifeToss.Infrastructure.Leveling;
using KnifeToss.Infrastructure.Persistence;
using KnifeToss.Presentation.Gameplay;
using UnityEngine;

namespace KnifeToss.Installers
{
    /// <summary>
    /// Composition root responsible for assembling gameplay services and exposing them to presenters.
    /// </summary>
    public class GameplayInstaller : MonoBehaviour
    {
        [Header("Configurations")]
        [SerializeField] private LevelConfig levelConfig;
        [SerializeField] private KnifeImpactEventChannel knifeImpactChannel;
        [SerializeField] private int startLevel = 1;

        [Header("Scene References")]
        [SerializeField] private Transform targetTransform;
        [SerializeField] private RotatingTarget rotatingTarget;
        [SerializeField] private KnifeThrower knifeThrower;
        [SerializeField] private GameObject knifePrefab;

        private const string HighestLevelKey = "HighestLevel";

        public LevelSession Session { get; private set; }
        public LevelSessionService SessionService { get; private set; }
        public KnifeImpactEventChannel ImpactChannel => knifeImpactChannel;
        public KnifeThrower Thrower => knifeThrower;

        private void Awake()
        {
            ResolveSceneDependencies();
            BuildServices();
        }

        private void ResolveSceneDependencies()
        {
            if (targetTransform == null)
            {
                GameObject target = GameObject.FindGameObjectWithTag("Target");
                if (target != null)
                {
                    targetTransform = target.transform;
                }
            }

            if (rotatingTarget == null && targetTransform != null)
            {
                rotatingTarget = targetTransform.GetComponent<RotatingTarget>();
            }

            if (knifeThrower == null)
            {
                knifeThrower = FindFirstObjectByType<KnifeThrower>();
            }
        }

        private void BuildServices()
        {
            Session = new LevelSession();
            ILevelCatalog catalog = new LevelConfigCatalog(levelConfig);
            ILevelProgressStore progressStore = new PlayerPrefsProgressStore(HighestLevelKey);
            var knivesHierarchy = new KnivesHierarchyManager(transform, targetTransform);
            var preStuckSpawner = new PreStuckKnifeSpawner(targetTransform, knifePrefab);

            SessionService = new LevelSessionService(
                Session,
                catalog,
                progressStore,
                rotatingTarget,
                knifeThrower,
                knivesHierarchy,
                preStuckSpawner,
                startLevel);
        }
    }
}
