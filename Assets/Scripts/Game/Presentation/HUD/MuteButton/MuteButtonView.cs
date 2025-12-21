using Game.Presentation.Common;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation.HUD.MuteButton
{
    /// <summary>
    /// Displays the mute button label and exposes click events.
    /// </summary>
    public class MuteButtonView : BaseView<MuteButtonModel>
    {
        [SerializeField] private Button _Button;
        [SerializeField] private TMP_Text _Label;

        public Observable<Unit> Clicked => _Button != null ? _Button.OnClickAsObservable() : Observable.Empty<Unit>();

        public void SetMutedLabel(bool isMuted)
        {
            if (_Label == null)
            {
                return;
            }

            _Label.text = isMuted ? "UNMUTE" : "MUTE";
        }
    }
}
