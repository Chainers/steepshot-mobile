namespace Steepshot.Core.Presenters
{
    public class SplashPresenter : SignInPresenter
    {
        public bool IsGuest => !User.IsAuthenticated;
    }
}