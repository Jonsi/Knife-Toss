using System;
using App.Services.Target;
using KnifeToss.Core.Abstractions.Config;
using KnifeToss.Core.Abstractions.Events;
using KnifeToss.Core.Abstractions.Gameplay.Session;
using KnifeToss.Core.Data.Knife;
using KnifeToss.Core.Data.Level;
using KnifeToss.Core.Domain.Level;
using UnityEngine;

namespace App.Services.Session
{
    /// <summary>
    /// Tracks level progress, raises impact/state events, and blocks throws when needed.
    /// </summary>
    public class LevelSessionService : ILevelSessionService
    {
        private readonly IConfigurationProvider<LevelRules> _RulesProvider;
        private readonly IEventChannel<LevelStateChangedEventData> _StateChangedChannel;
        private readonly IEventChannel<KnifeImpactEventData> _ImpactChannel;
        private readonly LevelSessionLogger _SessionLogger;
        private readonly TargetTrackingService _TargetTrackingService;

        private LevelSession _Session;
        private int _CurrentStageIndex;
        private int _CurrentLevelIndex;

        public bool IsPlaying => _Session?.IsPlaying ?? false;
        public int KnivesThrown => _Session?.KnivesThrown ?? 0;
        public int KnivesStuck => _Session?.KnivesStuck ?? 0;
        public int KnivesRemaining => _Session == null ? 0 : _Session.KnifeCount - _Session.KnivesThrown;

        public LevelSessionService(
            IConfigurationProvider<LevelRules> rulesProvider,
            IEventChannel<LevelStateChangedEventData> stateChangedChannel,
            IEventChannel<KnifeImpactEventData> impactChannel,
            LevelSessionLogger sessionLogger,
            TargetTrackingService targetTrackingService)
        {
            _RulesProvider = rulesProvider ?? throw new ArgumentNullException(nameof(rulesProvider));
            _StateChangedChannel = stateChangedChannel ?? throw new ArgumentNullException(nameof(stateChangedChannel));
            _ImpactChannel = impactChannel ?? throw new ArgumentNullException(nameof(impactChannel));
            _SessionLogger = sessionLogger ?? throw new ArgumentNullException(nameof(sessionLogger));
            _TargetTrackingService = targetTrackingService ?? throw new ArgumentNullException(nameof(targetTrackingService));

            _TargetTrackingService.OnAllTargetsDestroyed += HandleAllTargetsDestroyed;
        }

        private void HandleAllTargetsDestroyed()
        {
            if (_Session == null || !_Session.IsPlaying)
                return;

            string previous = _Session.CurrentStateName;
            _Session.ForceComplete();
            string current = _Session.CurrentStateName;

            RaiseStateChanged(previous, current);
        }

        public void StartSession(int stageIndex, int levelIndex)
        {
            _CurrentStageIndex = stageIndex;
            _CurrentLevelIndex = levelIndex;

            LevelRules rules = _RulesProvider.GetConfiguration();
            string previous = _Session?.CurrentStateName ?? string.Empty;

            if (_Session == null)
            {
                _Session = new LevelSession(rules);
            }
            else
            {
                _Session.Reset(rules);
            }

            _Session.Start();
            RaiseStateChanged(previous, _Session.CurrentStateName);
        }

        public bool TryRegisterThrow()
        {
            if (_Session == null || !_Session.CanThrow())
                return false;

            _Session.RegisterThrow();
            return true;
        }

        public void RegisterStick(Vector2 impactPoint)
        {
            if (_Session == null || !_Session.IsPlaying)
                return;

            string previous = _Session.CurrentStateName;
            _Session.RegisterStick();
            string current = _Session.CurrentStateName;

            _ImpactChannel.Raise(new KnifeImpactEventData(KnifeImpactType.Stuck, impactPoint));
            RaiseStateChanged(previous, current);
        }

        public void RegisterBounce(Vector2 impactPoint)
        {
            if (_Session == null || !_Session.IsPlaying)
                return;

            string previous = _Session.CurrentStateName;
            _Session.RegisterBounceFail();
            string current = _Session.CurrentStateName;

            _ImpactChannel.Raise(new KnifeImpactEventData(KnifeImpactType.Bounced, impactPoint));
            RaiseStateChanged(previous, current);
        }

        private void RaiseStateChanged(string previous, string current)
        {
            if (previous == current)
                return;

            _StateChangedChannel.Raise(new LevelStateChangedEventData(previous, current));
            _SessionLogger.LogState(current, _CurrentStageIndex, _CurrentLevelIndex);
        }
    }
}
