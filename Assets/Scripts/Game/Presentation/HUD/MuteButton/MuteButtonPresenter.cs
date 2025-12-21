using Common.MVP.Core;
using Game.UI.Services;
using R3;
using VContainer.Unity;

namespace Game.Presentation.HUD.MuteButton
{
    /// <summary>
    /// Presenter that bridges settings mute state to the HUD.
    /// </summary>
    public class MuteButtonPresenter : BasePresenter<MuteButtonView, MuteButtonModel>, IStartable
    {
        private readonly SettingsService _SettingsService;

        public MuteButtonPresenter(MuteButtonView view, SettingsService settingsService)
            : base(view, new MuteButtonModel())
        {
            _SettingsService = settingsService;
        }

        public void Start()
        {
            View.Initialize(Model);
            Model.IsMuted
                .Subscribe(isMuted => View.SetMutedLabel(isMuted))
                .AddTo(disposables);

            View.Clicked
                .Subscribe(_ => ToggleMute())
                .AddTo(disposables);

            if (_SettingsService != null)
            {
                ObserveMuteChanges()
                    .Subscribe(isMuted => Model.IsMuted.Value = isMuted)
                    .AddTo(disposables);

                Model.IsMuted.Value = _SettingsService.IsMuted;
            }
        }

        private void ToggleMute()
        {
            if (_SettingsService == null)
            {
                return;
            }

            _SettingsService.SetMuted(!_SettingsService.IsMuted);
        }

        private Observable<bool> ObserveMuteChanges()
        {
            return Observable.Create<bool>(observer =>
            {
                void Handler(bool isMuted) => observer.OnNext(isMuted);
                _SettingsService.OnMuteChanged += Handler;
                return Disposable.Create(() => _SettingsService.OnMuteChanged -= Handler);
            });
        }
    }
}
