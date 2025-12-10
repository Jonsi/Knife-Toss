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
        private readonly IConfigurationProvider<LevelRules> _rulesProvider;
        private readonly IEventChannel<LevelStateChangedEventData> _stateChangedChannel;
        private readonly IEventChannel<KnifeImpactEventData> _impactChannel;
        private readonly LevelSessionLogger _sessionLogger;
        private readonly TargetTrackingService _targetTrackingService;

        private LevelSession _session;
        private int _currentStageIndex;
        private int _currentLevelIndex;

        public bool IsPlaying => _session?.IsPlaying ?? false;
        public int KnivesThrown => _session?.KnivesThrown ?? 0;
        public int KnivesStuck => _session?.KnivesStuck ?? 0;
        public int KnivesRemaining => _session == null ? 0 : _session.KnifeCount - _session.KnivesThrown;

        public LevelSessionService(
            IConfigurationProvider<LevelRules> rulesProvider,
            IEventChannel<LevelStateChangedEventData> stateChangedChannel,
            IEventChannel<KnifeImpactEventData> impactChannel,
            LevelSessionLogger sessionLogger,
            TargetTrackingService targetTrackingService)
        {
            _rulesProvider = rulesProvider ?? throw new ArgumentNullException(nameof(rulesProvider));
            _stateChangedChannel = stateChangedChannel ?? throw new ArgumentNullException(nameof(stateChangedChannel));
            _impactChannel = impactChannel ?? throw new ArgumentNullException(nameof(impactChannel));
            _sessionLogger = sessionLogger ?? throw new ArgumentNullException(nameof(sessionLogger));
            _targetTrackingService = targetTrackingService ?? throw new ArgumentNullException(nameof(targetTrackingService));

            _targetTrackingService.OnAllTargetsDestroyed += HandleAllTargetsDestroyed;
        }

        private void HandleAllTargetsDestroyed()
        {
            if (_session == null || !_session.IsPlaying)
                return;

            string previous = _session.CurrentStateName;
            _session.ForceComplete();
            string current = _session.CurrentStateName;

            RaiseStateChanged(previous, current);
        }

        public void StartSession(int stageIndex, int levelIndex)
        {
            _currentStageIndex = stageIndex;
            _currentLevelIndex = levelIndex;

            LevelRules rules = _rulesProvider.GetConfiguration();
            string previous = _session?.CurrentStateName ?? string.Empty;

            if (_session == null)
            {
                _session = new LevelSession(rules);
            }
            else
            {
                _session.Reset(rules);
            }

            _session.Start();
            RaiseStateChanged(previous, _session.CurrentStateName);
        }

        public bool TryRegisterThrow()
        {
            if (_session == null || !_session.CanThrow())
                return false;

            _session.RegisterThrow();
            return true;
        }

        public void RegisterStick(Vector2 impactPoint)
        {
            if (_session == null || !_session.IsPlaying)
                return;

            string previous = _session.CurrentStateName;
            _session.RegisterStick();
            string current = _session.CurrentStateName;

            _impactChannel.Raise(new KnifeImpactEventData(KnifeImpactType.Stuck, impactPoint));
            RaiseStateChanged(previous, current);
        }

        public void RegisterBounce(Vector2 impactPoint)
        {
            if (_session == null || !_session.IsPlaying)
                return;

            string previous = _session.CurrentStateName;
            _session.RegisterBounceFail();
            string current = _session.CurrentStateName;

            _impactChannel.Raise(new KnifeImpactEventData(KnifeImpactType.Bounced, impactPoint));
            RaiseStateChanged(previous, current);
        }

        private void RaiseStateChanged(string previous, string current)
        {
            if (previous == current)
                return;

            _stateChangedChannel.Raise(new LevelStateChangedEventData(previous, current));
            _sessionLogger.LogState(current, _currentStageIndex, _currentLevelIndex);
        }
    }
}
