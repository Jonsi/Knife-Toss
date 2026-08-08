namespace Game.UI.Common
{
    /// <summary>
    /// Mutually-exclusive visual states used by <see cref="ActionButtonPresenter"/>.
    /// </summary>
    public enum ActionButtonPresentationState
    {
        Ready = 0,
        Pressed = 1,
        Disabled = 2,
        Loading = 3,
        Blocked = 4
    }

    /// <summary>
    /// Presentation-only state model for reusable action buttons.
    /// Precedence is Blocked, Loading, Disabled, Pressed, then Ready.
    /// </summary>
    public sealed class ActionButtonStateModel
    {
        public bool IsAvailable { get; private set; } = true;
        public bool IsPressed { get; private set; }
        public bool IsLoading { get; private set; }
        public bool IsBlocked { get; private set; }

        public ActionButtonPresentationState State
        {
            get
            {
                if (IsBlocked)
                    return ActionButtonPresentationState.Blocked;
                if (IsLoading)
                    return ActionButtonPresentationState.Loading;
                if (!IsAvailable)
                    return ActionButtonPresentationState.Disabled;
                if (IsPressed)
                    return ActionButtonPresentationState.Pressed;

                return ActionButtonPresentationState.Ready;
            }
        }

        public bool CanSubmit => IsAvailable && !IsLoading;

        public void SetAvailable(bool available)
        {
            IsAvailable = available;
            if (!available)
            {
                IsPressed = false;
            }
        }

        public void SetPressed(bool pressed)
        {
            IsPressed = pressed && IsAvailable && !IsLoading;
        }

        public bool TryBeginSubmission()
        {
            if (!CanSubmit)
                return false;

            IsPressed = false;
            IsBlocked = false;
            IsLoading = true;
            return true;
        }

        public void CompleteSubmission()
        {
            IsLoading = false;
        }

        public void SetBlocked(bool blocked)
        {
            IsBlocked = blocked;
            if (blocked)
            {
                IsPressed = false;
            }
        }

        public void ResetTransient()
        {
            IsPressed = false;
            IsLoading = false;
            IsBlocked = false;
        }
    }
}
