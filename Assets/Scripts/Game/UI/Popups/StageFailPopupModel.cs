using Common.MVP.Core;
using Game.Flow.Level;
using System;

namespace Game.UI.Popups
{
    public class StageFailPopupModel : IModel
    {
        public LevelData LevelData { get; }
        public Action RetryAction { get; }
        public Action RestartAction { get; }
        public Action CloseAction { get; }

        public StageFailPopupModel(
            LevelData levelData,
            Action retryAction,
            Action restartAction,
            Action closeAction)
        {
            LevelData = levelData;
            RetryAction = retryAction;
            RestartAction = restartAction;
            CloseAction = closeAction;
        }
    }
}
