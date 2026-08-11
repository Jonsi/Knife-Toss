namespace Game.UI.Controllers
{
    /// <summary>
    /// Presentation-only contract for opening and closing the scene-owned Settings flow.
    /// Buttons depend on this contract rather than a gameplay HUD controller.
    /// </summary>
    public interface ISettingsPopupPresenter
    {
        void ShowSettings();
        void HideSettings();
        void ToggleSettings();
    }
}
